using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using MyProject.Orders.Dto;
using static MyProject.Orders.OrderAppService;

namespace MyProject.Orders
{
	public interface IOrderAppService : IApplicationService
	{
		Task<PagedResultDto<OrderListDto>> GetAllOrder(GetAllOrdersInput input);
		Task<int> CreateOrder(CreateOrderDto input);
		Task<List<OrderOutput>> GetStatusOrder(int? orderStatus = 5);
		/// <summary>
		/// Lấy danh sách đơn hàng theo trạng thái với phân trang, sắp xếp theo đơn hàng mới nhất
		/// </summary>
		Task<PagedResultDto<OrderOutput>> GetStatusOrderPaged(int? orderStatus = 5, int page = 1, int pageSize = 10);
		//Task<List<int>> GetStatusOrder(int? orderStatus = 5);
		Task UpdateOrder(UpdateOrderDto input);
		Task<OrderListDto> GetOrderById(int orderId);
		/// <summary>
		/// Tự động hủy các đơn hàng đã hết hạn thanh toán
		/// </summary>
		Task<int> CancelExpiredOrders();

		/// <summary>
		/// Lấy danh sách đơn hàng Pending chưa thanh toán và chưa hết hạn
		/// </summary>
		Task<List<OrderListDto>> GetPendingUnpaidOrdersAsync();

		/// <summary>
		/// Lấy đơn hàng theo PaymentReference
		/// </summary>
		Task<OrderListDto> GetOrderByPaymentReference(string paymentReference);
	}
}
