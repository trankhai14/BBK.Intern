using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using MyProject.Payments.Dtos;

namespace MyProject.Payments
{
	/// <summary>
	/// Interface cho service kiểm tra và xác nhận thanh toán
	/// </summary>
	public interface IPaymentVerificationService : IApplicationService
	{
		/// <summary>
		/// Kiểm tra giao dịch từ ngân hàng (qua API hoặc file statement)
		/// </summary>
		Task<PaymentVerificationResult> VerifyPaymentAsync(string paymentReference, decimal expectedAmount, DateTime fromDate, DateTime toDate);

		/// <summary>
		/// Lưu giao dịch vào database
		/// </summary>
		Task<PaymentTransactionDto> SaveTransactionAsync(int orderId, PaymentTransactionDto transaction);

		/// <summary>
		/// Lấy giao dịch đã được xác nhận cho đơn hàng
		/// </summary>
		Task<PaymentTransactionDto> GetVerifiedTransactionAsync(int orderId);

		/// <summary>
		/// Tự động xác nhận thanh toán nếu tìm thấy giao dịch khớp
		/// </summary>
		Task<bool> AutoConfirmPaymentAsync(int orderId);

		/// <summary>
		/// Đánh dấu giao dịch là đã xác nhận
		/// </summary>
		Task<bool> MarkTransactionAsVerifiedAsync(int orderId, VerificationMethod method);

		/// <summary>
		/// Xác nhận giao dịch thủ công (bởi nhân viên)
		/// </summary>
		Task<bool> VerifyTransactionManuallyAsync(int transactionId);
	}
}

