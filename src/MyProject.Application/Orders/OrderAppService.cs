using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using MyProject.Authorization;
using MyProject.Inventories;
using MyProject.OrderDetails;
using MyProject.Orders.Dto;

namespace MyProject.Orders
{
	//[AbpAuthorize(PermissionNames.Pages_Orders)]
	public class OrderAppService : ApplicationService, IOrderAppService
	{
		private readonly IRepository<Order> _orderAppService;
		private readonly IOrderDetailAppService _orderDetailAppService;
		private readonly IInventoryAppService _inventoryAppService;

		public OrderAppService(
			IRepository<Order> orderAppService,
			IOrderDetailAppService orderDetailAppService,
			IInventoryAppService inventoryAppService)
		{
			_orderAppService = orderAppService;
			_orderDetailAppService = orderDetailAppService;
			_inventoryAppService = inventoryAppService;
		}

		public async Task<PagedResultDto<OrderListDto>> GetAllOrder(GetAllOrdersInput input)
		{

		var orders = _orderAppService.GetAll();

		if (!string.IsNullOrEmpty(input.NameUser))
		{
			orders = orders.Where(o => o.NameUser.Contains(input.NameUser));
		}

		if (input.OrderStatus != null)
		{
			orders = orders.Where(o => o.OrderStatus == input.OrderStatus);
		}

		if (input.PaymentMethod != null)
		{
			orders = orders.Where(o => o.PaymentMethod == input.PaymentMethod);
		}

		// Filter theo khoảng ngày
		if (input.FromDate.HasValue)
		{
			var fromDate = input.FromDate.Value.Date;
			orders = orders.Where(o => o.CreationTime.Date >= fromDate);
		}

		if (input.ToDate.HasValue)
		{
			var toDate = input.ToDate.Value.Date.AddDays(1).AddTicks(-1); // Đến cuối ngày
			orders = orders.Where(o => o.CreationTime <= toDate);
		}

			var counts = await orders.CountAsync();

			var orderDtos = orders.OrderByDescending(o => o.CreationTime).PageBy(input).Select(o => new OrderListDto
			{
				Id = o.Id,
				UserId = o.UserId,
				NameUser = o.NameUser,
				TotalAmount = o.totalAmount,
				DiscountAmount = o.DiscountAmount,
				PaymentMethod = o.PaymentMethod,
				CreationTime = o.CreationTime,
				OrderStatus = o.OrderStatus,
				PhoneNumber = o.PhoneNumber,
				ShippingAddress = o.ShippingAddress,
				PaymentReference = o.PaymentReference,
				IsPaid = o.IsPaid,
				PaidTime = o.PaidTime
			}).ToList();

			return new PagedResultDto<OrderListDto>(counts, orderDtos);
		}
		[AbpAuthorize(PermissionNames.Pages_Orders_Create)]
		public async Task<int> CreateOrder(CreateOrderDto input)
		{
			try
			{
				var order = new Order
				{
					UserId = input.UserId,
					NameUser = input.NameUser,
					totalAmount = input.TotalAmount,
					DiscountAmount = input.DiscountAmount,
					OrderStatus = input.OrderStatus,
					PaymentMethod = input.PaymentMethod,
					PhoneNumber = input.PhoneNumber,
					ShippingAddress = input.ShippingAddress,
					PaymentReference = input.PaymentReference,
					IsPaid = input.IsPaid,
					PaidTime = input.PaidTime,
					PaymentExpiredAt = input.PaymentExpiredAt,
					CustomerNote = input.CustomerNote
				};
				var result = await _orderAppService.InsertAndGetIdAsync(order);
				return result;
			}
			catch (Exception ex)
			{
				throw;
			}
		}

		public async Task<List<OrderOutput>> GetStatusOrder(int? orderStatus = 5)
		{
			if (AbpSession.UserId != null)
			{
				var userId = AbpSession.UserId;

				//List<Order> orderList;
				List<Order> orderList;

				if (orderStatus == 5)
				{
					orderList = await _orderAppService.GetAllListAsync(x => x.UserId == userId);
				}
				else
				{
					orderList = await _orderAppService.GetAllListAsync(x => x.OrderStatus == orderStatus && x.UserId == userId);
				}

				//var orderIds = orderList.Select(o => o.Id ).ToList();
				//return orderIds;
				var orderOutputs = orderList.Select(o => new OrderOutput
				{
					OrderId = o.Id,
					OrderStatus = o.OrderStatus,
				}).ToList();

				return orderOutputs;
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Lấy danh sách đơn hàng theo trạng thái với phân trang, sắp xếp theo đơn hàng mới nhất
		/// </summary>
		public async Task<PagedResultDto<OrderOutput>> GetStatusOrderPaged(int? orderStatus = 5, int page = 1, int pageSize = 10)
		{
			if (AbpSession.UserId == null)
			{
				return new PagedResultDto<OrderOutput>(0, new List<OrderOutput>());
			}

			var userId = AbpSession.UserId.Value;
			var orders = _orderAppService.GetAll().Where(x => x.UserId == userId);

			// Lọc theo trạng thái nếu không phải là "Tất cả" (5)
			if (orderStatus.HasValue && orderStatus.Value != 5)
			{
				orders = orders.Where(x => x.OrderStatus == orderStatus.Value);
			}

			// Sắp xếp theo CreationTime DESC (đơn hàng mới nhất trước)
			orders = orders.OrderByDescending(x => x.CreationTime);

			// Đếm tổng số đơn hàng
			var totalCount = await orders.CountAsync();

			// Phân trang
			var pagedOrders = await orders
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			// Map sang OrderOutput
			var orderOutputs = pagedOrders.Select(o => new OrderOutput
			{
				OrderId = o.Id,
				OrderStatus = o.OrderStatus,
			}).ToList();

			return new PagedResultDto<OrderOutput>(totalCount, orderOutputs);
		}
		public class OrderOutput
		{
			public int OrderId { get; set; }
			public int OrderStatus { get; set; }
		}
		public async Task<OrderListDto> GetOrderById(int orderId)
		{
			var order = await _orderAppService.FirstOrDefaultAsync(orderId);

			return new OrderListDto
			{
				Id = order.Id,
				UserId = order.UserId,
				NameUser = order.NameUser,
				OrderStatus = order.OrderStatus,
				TotalAmount = order.totalAmount,
				PaymentMethod = order.PaymentMethod,
				DiscountAmount = order.DiscountAmount,
				CreationTime = order.CreationTime,
				PhoneNumber = order.PhoneNumber,
				ShippingAddress = order.ShippingAddress,
				PaymentReference = order.PaymentReference,
				IsPaid = order.IsPaid,
				PaidTime = order.PaidTime
			};
		}

		[AbpAuthorize(PermissionNames.Pages_Orders_Edit)]
		public async Task UpdateOrder(UpdateOrderDto input)
		{
			var order = await _orderAppService.GetAsync(input.OrderId);

			// Validation: Kiểm tra logic nghiệp vụ hợp lý
			// Nếu đơn hàng đã hủy hoặc thành công, không cho phép cập nhật trạng thái
			if (order.OrderStatus == (int)OrderStatus.Canceled || order.OrderStatus == (int)OrderStatus.Success)
			{
				throw new UserFriendlyException("Không thể cập nhật đơn hàng đã hủy hoặc đã hoàn thành.");
			}

			// Validation: Nếu cập nhật IsPaid = true, phải đảm bảo OrderStatus hợp lý
			if (input.IsPaid.HasValue && input.IsPaid.Value)
			{
				// Khi thanh toán thành công, tự động cập nhật OrderStatus = Confirmed nếu chưa được set
				if (order.OrderStatus == (int)OrderStatus.Pending)
				{
					order.OrderStatus = (int)OrderStatus.Confirmed;
					order.PaidTime = input.PaidTime ?? DateTime.Now;
				}
				else if (input.OrderStatus == (int)OrderStatus.Pending)
				{
					// Nếu đang set OrderStatus = Pending nhưng IsPaid = true, không hợp lý
					throw new UserFriendlyException("Không thể đặt trạng thái 'Chờ xử lý' cho đơn hàng đã thanh toán.");
				}
			}

			// Validation: Nếu set OrderStatus = Success hoặc Shipping, phải đảm bảo đã thanh toán
			if (input.OrderStatus == (int)OrderStatus.Success || input.OrderStatus == (int)OrderStatus.Shipping)
			{
				if (!order.IsPaid && (!input.IsPaid.HasValue || !input.IsPaid.Value))
				{
					throw new UserFriendlyException("Không thể chuyển đơn hàng sang 'Đang giao hàng' hoặc 'Thành công' khi chưa thanh toán.");
				}
			}

			// Validation: Nếu set OrderStatus = Confirmed, phải đảm bảo đã thanh toán
			if (input.OrderStatus == (int)OrderStatus.Confirmed)
			{
				if (!order.IsPaid && (!input.IsPaid.HasValue || !input.IsPaid.Value))
				{
					throw new UserFriendlyException("Không thể xác nhận đơn hàng khi chưa thanh toán.");
				}
			}

			// Cập nhật OrderStatus nếu hợp lệ
			if (input.OrderStatus != order.OrderStatus)
			{
				// Chỉ cho phép chuyển trạng thái theo flow hợp lý
				if (IsValidStatusTransition(order.OrderStatus, input.OrderStatus))
				{
					order.OrderStatus = input.OrderStatus;
				}
				else
				{
					throw new UserFriendlyException($"Không thể chuyển từ trạng thái '{GetStatusName(order.OrderStatus)}' sang '{GetStatusName(input.OrderStatus)}'.");
				}
			}

			// Cập nhật PaymentMethod
			order.PaymentMethod = input.PaymentMethod;

			// Cập nhật IsPaid
			if (input.IsPaid.HasValue)
			{
				order.IsPaid = input.IsPaid.Value;
				if (input.IsPaid.Value)
				{
					order.PaidTime = input.PaidTime ?? DateTime.Now;
					// Tự động cập nhật OrderStatus = Confirmed khi thanh toán thành công
					if (order.OrderStatus == (int)OrderStatus.Pending)
					{
						order.OrderStatus = (int)OrderStatus.Confirmed;
					}
				}
				else
				{
					order.PaidTime = null;
					// Nếu hủy thanh toán, chỉ cho phép nếu đơn hàng chưa được xác nhận
					if (order.OrderStatus != (int)OrderStatus.Pending)
					{
						throw new UserFriendlyException("Không thể hủy thanh toán cho đơn hàng đã được xác nhận.");
					}
				}
			}

			if (!string.IsNullOrWhiteSpace(input.PaymentReference))
			{
				order.PaymentReference = input.PaymentReference;
			}

			// lưu vào db
			await _orderAppService.UpdateAsync(order);
		}

		/// <summary>
		/// Kiểm tra xem việc chuyển trạng thái có hợp lệ không
		/// </summary>
		private bool IsValidStatusTransition(int currentStatus, int newStatus)
		{
			// Cho phép chuyển trạng thái theo flow:
			// Pending -> Confirmed -> Shipping -> Success
			// Bất kỳ trạng thái nào -> Canceled
			// Canceled và Success không thể chuyển sang trạng thái khác

			if (currentStatus == (int)OrderStatus.Canceled || currentStatus == (int)OrderStatus.Success)
			{
				return false; // Không thể chuyển từ Canceled hoặc Success
			}

			if (newStatus == (int)OrderStatus.Canceled)
			{
				return true; // Luôn cho phép hủy đơn hàng (trừ khi đã Success)
			}

			// Flow hợp lý:
			switch (currentStatus)
			{
				case (int)OrderStatus.Pending:
					// Pending chỉ có thể chuyển sang Confirmed hoặc Canceled
					return newStatus == (int)OrderStatus.Confirmed || newStatus == (int)OrderStatus.Canceled;

				case (int)OrderStatus.Confirmed:
					// Confirmed có thể chuyển sang Shipping, Canceled
					return newStatus == (int)OrderStatus.Shipping || newStatus == (int)OrderStatus.Canceled;

				case (int)OrderStatus.Shipping:
					// Shipping chỉ có thể chuyển sang Success hoặc Canceled
					return newStatus == (int)OrderStatus.Success || newStatus == (int)OrderStatus.Canceled;

				default:
					return false;
			}
		}

		/// <summary>
		/// Lấy tên trạng thái đơn hàng
		/// </summary>
		private string GetStatusName(int status)
		{
			return status switch
			{
				(int)OrderStatus.Pending => "Chờ xử lý",
				(int)OrderStatus.Confirmed => "Đã xác nhận",
				(int)OrderStatus.Shipping => "Đang giao hàng",
				(int)OrderStatus.Canceled => "Đã hủy",
				(int)OrderStatus.Success => "Thành công",
				_ => "Không xác định"
			};
		}

		/// <summary>
		/// Tự động hủy các đơn hàng đã hết hạn thanh toán
		/// Nên được gọi định kỳ bởi background job hoặc scheduled task
		/// </summary>
		public async Task<int> CancelExpiredOrders()
		{
			var now = DateTime.UtcNow;
			var expiredOrders = await _orderAppService.GetAllListAsync(o =>
				o.OrderStatus == (int)OrderStatus.Pending &&
				!o.IsPaid &&
				o.PaymentExpiredAt.HasValue &&
				o.PaymentExpiredAt.Value < now);

			if (!expiredOrders.Any())
			{
				return 0;
			}

			int canceledCount = 0;
			foreach (var order in expiredOrders)
			{
				try
				{
					// Lấy danh sách chi tiết đơn hàng để giải phóng inventory
					var orderDetails = await _orderDetailAppService.GetOrderListById(order.Id);

					// Giải phóng inventory đã reserve cho từng sản phẩm
					foreach (var detail in orderDetails)
					{
						try
						{
							await _inventoryAppService.ReleaseReservedInventory(detail.ProductId, detail.Quantity);
						}
						catch (Exception ex)
						{
							Logger.Warn($"Không thể giải phóng inventory cho sản phẩm #{detail.ProductId} trong đơn hàng #{order.Id}: {ex.Message}", ex);
						}
					}

					// Cập nhật trạng thái đơn hàng thành Canceled
					order.OrderStatus = (int)OrderStatus.Canceled;
					await _orderAppService.UpdateAsync(order);

					canceledCount++;
					Logger.Info($"Đã tự động hủy đơn hàng #{order.Id} do hết hạn thanh toán. Hạn thanh toán: {order.PaymentExpiredAt}");
				}
				catch (Exception ex)
				{
					Logger.Error($"Lỗi khi hủy đơn hàng #{order.Id}: {ex.Message}", ex);
				}
			}

			return canceledCount;
		}

		/// <summary>
		/// Lấy danh sách đơn hàng Pending chưa thanh toán và chưa hết hạn
		/// </summary>
		public async Task<List<OrderListDto>> GetPendingUnpaidOrdersAsync()
		{
			var orders = await _orderAppService.GetAllListAsync(o =>
				o.OrderStatus == (int)OrderStatus.Pending &&
				!o.IsPaid &&
				(!o.PaymentExpiredAt.HasValue || o.PaymentExpiredAt.Value > DateTime.UtcNow));

			return orders.Select(o => new OrderListDto
			{
				Id = o.Id,
				UserId = o.UserId,
				NameUser = o.NameUser,
				TotalAmount = o.totalAmount,
				DiscountAmount = o.DiscountAmount,
				PaymentMethod = o.PaymentMethod,
				CreationTime = o.CreationTime,
				OrderStatus = o.OrderStatus,
				PhoneNumber = o.PhoneNumber,
				ShippingAddress = o.ShippingAddress,
				PaymentReference = o.PaymentReference,
				IsPaid = o.IsPaid,
				PaidTime = o.PaidTime
			}).ToList();
		}

		/// <summary>
		/// Lấy đơn hàng theo PaymentReference
		/// </summary>
		public async Task<OrderListDto> GetOrderByPaymentReference(string paymentReference)
		{
			var order = await _orderAppService.FirstOrDefaultAsync(o => o.PaymentReference == paymentReference);
			if (order == null)
			{
				return null;
			}

			return new OrderListDto
			{
				Id = order.Id,
				UserId = order.UserId,
				NameUser = order.NameUser,
				OrderStatus = order.OrderStatus,
				TotalAmount = order.totalAmount,
				PaymentMethod = order.PaymentMethod,
				DiscountAmount = order.DiscountAmount,
				CreationTime = order.CreationTime,
				PhoneNumber = order.PhoneNumber,
				ShippingAddress = order.ShippingAddress,
				PaymentReference = order.PaymentReference,
				IsPaid = order.IsPaid,
				PaidTime = order.PaidTime
			};
		}
	}
}
