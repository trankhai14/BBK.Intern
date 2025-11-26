using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using MyProject.Inventories;
using MyProject.OrderDetails;
using MyProject.Orders;
using MyProject.Orders.Dto;
using MyProject.Payments.Dtos;
using OrderEntity = MyProject.Orders.Order;
using OrderStatusEnum = MyProject.Orders.OrderStatus;

namespace MyProject.Payments
{
	/// <summary>
	/// Service kiểm tra và xác nhận thanh toán
	/// </summary>
	public class PaymentVerificationService : MyProjectAppServiceBase, IPaymentVerificationService
	{
		private readonly IRepository<PaymentTransaction> _paymentTransactionRepository;
		private readonly IRepository<OrderEntity> _orderRepository;
		private readonly IOrderDetailAppService _orderDetailAppService;
		private readonly IInventoryAppService _inventoryAppService;
		private readonly IOrderAppService _orderAppService;

		public PaymentVerificationService(
			IRepository<PaymentTransaction> paymentTransactionRepository,
			IRepository<OrderEntity> orderRepository,
			IOrderDetailAppService orderDetailAppService,
			IInventoryAppService inventoryAppService,
			IOrderAppService orderAppService)
		{
			_paymentTransactionRepository = paymentTransactionRepository;
			_orderRepository = orderRepository;
			_orderDetailAppService = orderDetailAppService;
			_inventoryAppService = inventoryAppService;
			_orderAppService = orderAppService;
		}

		/// <summary>
		/// Kiểm tra giao dịch từ ngân hàng
		/// Hiện tại chỉ kiểm tra trong database (đã được lưu từ webhook hoặc import)
		/// Có thể mở rộng để gọi API ngân hàng hoặc đọc file statement
		/// </summary>
		public async Task<PaymentVerificationResult> VerifyPaymentAsync(
			string paymentReference,
			decimal expectedAmount,
			DateTime fromDate,
			DateTime toDate)
		{
			// Tìm giao dịch trong database đã được lưu (từ webhook hoặc import)
			var transaction = await _paymentTransactionRepository.GetAll()
				.Where(t => t.PaymentReference == paymentReference
					&& t.Amount == expectedAmount
					&& t.TransactionTime >= fromDate
					&& t.TransactionTime <= toDate
					&& t.Status == PaymentTransactionStatus.Verified)
				.OrderByDescending(t => t.TransactionTime)
				.FirstOrDefaultAsync();

			if (transaction != null)
			{
				return new PaymentVerificationResult
				{
					IsVerified = true,
					Transaction = MapToDto(transaction),
					Message = "Đã tìm thấy giao dịch thanh toán",
					Method = VerificationMethod.Webhook // Hoặc API nếu từ API
				};
			}

			// Nếu chưa tìm thấy, có thể mở rộng để:
			// 1. Gọi API ngân hàng để kiểm tra
			// 2. Đọc file statement từ ngân hàng
			// 3. Trả về false để yêu cầu xác nhận thủ công

			return new PaymentVerificationResult
			{
				IsVerified = false,
				Message = "Chưa tìm thấy giao dịch thanh toán. Vui lòng kiểm tra lại hoặc liên hệ hỗ trợ.",
				Method = VerificationMethod.Manual
			};
		}

		/// <summary>
		/// Lưu giao dịch vào database
		/// </summary>
		public async Task<PaymentTransactionDto> SaveTransactionAsync(int orderId, PaymentTransactionDto transactionDto)
		{
			var order = await _orderRepository.GetAsync(orderId);

			// Kiểm tra xem đã có giao dịch với TransactionId này chưa (tránh duplicate)
			var existingTransaction = await _paymentTransactionRepository.GetAll()
				.Where(t => t.TransactionId == transactionDto.TransactionId && !string.IsNullOrEmpty(transactionDto.TransactionId))
				.FirstOrDefaultAsync();

			if (existingTransaction != null)
			{
				// Đã tồn tại, cập nhật lại
				existingTransaction.Status = PaymentTransactionStatus.Verified;
				existingTransaction.VerifiedBy = transactionDto.VerifiedBy ?? "System";
				existingTransaction.VerifiedAt = DateTime.UtcNow;
				await _paymentTransactionRepository.UpdateAsync(existingTransaction);
				await CurrentUnitOfWork.SaveChangesAsync();
				return MapToDto(existingTransaction);
			}

			// Tạo mới
			var transaction = new PaymentTransaction
			{
				OrderId = orderId,
				PaymentReference = transactionDto.PaymentReference ?? order.PaymentReference,
				Amount = transactionDto.Amount,
				BankCode = transactionDto.BankCode,
				BankAccount = transactionDto.BankAccount,
				TransactionId = transactionDto.TransactionId,
				TransactionTime = transactionDto.TransactionTime,
				Content = transactionDto.Content,
				Status = transactionDto.Status == PaymentTransactionStatus.Verified 
					? PaymentTransactionStatus.Verified 
					: PaymentTransactionStatus.Pending,
				VerifiedBy = transactionDto.VerifiedBy,
				VerifiedAt = transactionDto.VerifiedAt,
				Notes = transactionDto.Notes
			};

			var id = await _paymentTransactionRepository.InsertAndGetIdAsync(transaction);
			await CurrentUnitOfWork.SaveChangesAsync();

			var savedTransaction = await _paymentTransactionRepository.GetAsync(id);
			return MapToDto(savedTransaction);
		}

		/// <summary>
		/// Lấy giao dịch đã được xác nhận cho đơn hàng
		/// </summary>
		public async Task<PaymentTransactionDto> GetVerifiedTransactionAsync(int orderId)
		{
			var transaction = await _paymentTransactionRepository.GetAll()
				.Where(t => t.OrderId == orderId && t.Status == PaymentTransactionStatus.Verified)
				.OrderByDescending(t => t.VerifiedAt)
				.FirstOrDefaultAsync();

			return transaction != null ? MapToDto(transaction) : null;
		}

		/// <summary>
		/// Tự động xác nhận thanh toán nếu tìm thấy giao dịch khớp
		/// </summary>
		public async Task<bool> AutoConfirmPaymentAsync(int orderId)
		{
			var order = await _orderRepository.GetAsync(orderId);

			if (order.IsPaid)
			{
				return true; // Đã thanh toán rồi
			}

			// Kiểm tra xem có giao dịch đã được xác nhận chưa
			var transaction = await GetVerifiedTransactionAsync(orderId);
			if (transaction == null)
			{
				return false; // Chưa có giao dịch
			}

			// Xác nhận thanh toán
			try
			{
				var details = await _orderDetailAppService.GetOrderListById(orderId);
				foreach (var detail in details)
				{
					await _inventoryAppService.CommitReservedInventory(detail.ProductId, detail.Quantity);
				}

				await _orderAppService.UpdateOrder(new UpdateOrderDto
				{
					OrderId = orderId,
					OrderStatus = (int)OrderStatusEnum.Confirmed,
					PaymentMethod = order.PaymentMethod,
					IsPaid = true,
					PaidTime = transaction.TransactionTime
				});

				Logger.Info($"Đã tự động xác nhận thanh toán cho đơn hàng #{orderId}");
				return true;
			}
			catch (Exception ex)
			{
				Logger.Error($"Lỗi khi tự động xác nhận thanh toán cho đơn hàng #{orderId}", ex);
				return false;
			}
		}

		/// <summary>
		/// Đánh dấu giao dịch là đã xác nhận
		/// </summary>
		public async Task<bool> MarkTransactionAsVerifiedAsync(int orderId, VerificationMethod method)
		{
			var transaction = await _paymentTransactionRepository.GetAll()
				.Where(t => t.OrderId == orderId)
				.OrderByDescending(t => t.TransactionTime)
				.FirstOrDefaultAsync();

			if (transaction == null)
			{
				return false;
			}

			transaction.Status = PaymentTransactionStatus.Verified;
			transaction.VerifiedBy = method.ToString();
			transaction.VerifiedAt = DateTime.UtcNow;

			await _paymentTransactionRepository.UpdateAsync(transaction);
			await CurrentUnitOfWork.SaveChangesAsync();

			return true;
		}

		/// <summary>
		/// Xác nhận giao dịch thủ công (bởi nhân viên)
		/// </summary>
		public async Task<bool> VerifyTransactionManuallyAsync(int transactionId)
		{
			var transaction = await _paymentTransactionRepository.GetAsync(transactionId);

			if (transaction.Status == PaymentTransactionStatus.Verified)
			{
				return true; // Đã xác nhận rồi
			}

			transaction.Status = PaymentTransactionStatus.Verified;
			transaction.VerifiedBy = "Manual";
			transaction.VerifiedAt = DateTime.UtcNow;

			await _paymentTransactionRepository.UpdateAsync(transaction);
			await CurrentUnitOfWork.SaveChangesAsync();

			// Tự động xác nhận thanh toán cho đơn hàng
			await AutoConfirmPaymentAsync(transaction.OrderId);

			return true;
		}

		/// <summary>
		/// Map Entity sang DTO
		/// </summary>
		private PaymentTransactionDto MapToDto(PaymentTransaction transaction)
		{
			if (transaction == null) return null;

			return new PaymentTransactionDto
			{
				Id = transaction.Id,
				OrderId = transaction.OrderId,
				PaymentReference = transaction.PaymentReference,
				Amount = transaction.Amount,
				BankCode = transaction.BankCode,
				BankAccount = transaction.BankAccount,
				TransactionId = transaction.TransactionId,
				TransactionTime = transaction.TransactionTime,
				Content = transaction.Content,
				Status = transaction.Status,
				StatusName = GetStatusName(transaction.Status),
				VerifiedBy = transaction.VerifiedBy,
				VerifiedAt = transaction.VerifiedAt,
				Notes = transaction.Notes,
				CreationTime = transaction.CreationTime
			};
		}

		/// <summary>
		/// Lấy tên trạng thái
		/// </summary>
		private string GetStatusName(PaymentTransactionStatus status)
		{
			return status switch
			{
				PaymentTransactionStatus.Pending => "Chờ xác nhận",
				PaymentTransactionStatus.Verified => "Đã xác nhận",
				PaymentTransactionStatus.Failed => "Thất bại",
				PaymentTransactionStatus.Expired => "Hết hạn",
				_ => "Không xác định"
			};
		}
	}
}

