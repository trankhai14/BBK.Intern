using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using MyProject.OrderDetails.Dto;
using MyProject.Orders;
using MyProject.Orders.Dto;

namespace MyProject.OrderDetails
{
	public class OrderDetailAppService : MyProjectAppServiceBase, IOrderDetailAppService
	{
		private readonly IRepository<OrderDetail> _orderDetailAppService;

		public OrderDetailAppService(IRepository<OrderDetail> orderDetailAppService)
		{
			_orderDetailAppService = orderDetailAppService;
		}

		public async Task<List<OrderDetailDto>> GetAllOrder(long? orderId)
		{

			var orders = _orderDetailAppService.GetAll();

			if (orderId.HasValue)
			{
				orders = orders.Where(x => x.OrderId == orderId.Value);
			}

			var orderList = await orders.ToListAsync(); // Chuyển `IQueryable` thành `List`

			return orderList.Select(order => new OrderDetailDto
			{
				Id = order.Id,
				OrderId = order.OrderId,
				ProductId = order.ProductId,
				Quantity = order.Quantity,
				UnitPrice = order.UnitPrice,
				DiscountPrice = order.DiscountPrice,
				CreationTime = order.CreationTime,
			}).ToList();
		}




		public async Task<OrderDetail> CreateOrderDetail(OrderDetailDto input)
		{
			var orderDetail = new OrderDetail
			{
				ProductId = input.ProductId,
				OrderId = input.OrderId,
				Quantity = input.Quantity,
				UnitPrice = input.UnitPrice,
				DiscountPrice = input.DiscountPrice,
			};

			await _orderDetailAppService.InsertAsync(orderDetail);

			return orderDetail;
		}

		public async Task<List<OrderDetailDto>> GetOrderByIdAndStatus(List<int> orderIds)
		{
			var orderList = await _orderDetailAppService.GetAll().Where(o => orderIds.Contains(o.OrderId)).ToListAsync();
			var orderListDtos = orderList.Select(od => new OrderDetailDto
			{
				Id = od.Id,
				OrderId = od.OrderId,
				ProductId = od.ProductId,
				Quantity = od.Quantity,
				UnitPrice = od.UnitPrice,
				DiscountPrice = od.DiscountPrice,
			}).ToList();

			return orderListDtos;
		}

		public async Task<List<OrderDetailDto>> GetOrderListById(int orderId)
		{
			var orderDetails = await _orderDetailAppService.GetAllListAsync(od => od.OrderId == orderId);

			return orderDetails.Select(orderDetail => new OrderDetailDto
			{
				Id = orderDetail.Id,
				OrderId = orderDetail.OrderId,
				ProductId = orderDetail.ProductId,
				Quantity = orderDetail.Quantity,
				UnitPrice = orderDetail.UnitPrice,
				DiscountPrice = orderDetail.DiscountPrice,
				CreationTime = orderDetail.CreationTime
			}).ToList();
		}
	}
}
