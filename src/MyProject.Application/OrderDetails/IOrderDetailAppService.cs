using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services;
using MyProject.OrderDetails.Dto;
using MyProject.Orders;

namespace MyProject.OrderDetails
{
	public interface IOrderDetailAppService : IApplicationService
	{
		Task<OrderDetail> CreateOrderDetail(OrderDetailDto input);
		Task<List<OrderDetailDto>> GetAllOrder(long? orderId);
		Task<List<OrderDetailDto>> GetOrderByIdAndStatus(List<int> orderIds);
		Task<List<OrderDetailDto>> GetOrderListById(int orderId);
	}
}
