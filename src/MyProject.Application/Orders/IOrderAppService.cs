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
		//Task<List<int>> GetStatusOrder(int? orderStatus = 5);
		Task UpdateOrder(UpdateOrderDto input);
		Task<OrderListDto> GetOrderById(int orderId);
	}
}
