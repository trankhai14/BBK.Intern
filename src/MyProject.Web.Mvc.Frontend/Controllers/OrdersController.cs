using Abp.Domain.Uow;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyProject.Carts;
using MyProject.Controllers;
using MyProject.OrderDetails;
using MyProject.Orders;
using MyProject.Orders.Dto;
using MyProject.Products;
using MyProject.Web.Models.Orders;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MyProject.Web.Controllers
{
	public class OrdersController : MyProjectControllerBase
	{
		private readonly IOrderAppService _orderAppService;
		private readonly IOrderDetailAppService _orderDetailAppService;
		private readonly ICartAppService _cartAppService;
		private readonly IProductAppService _productAppService;


		public OrdersController(IOrderAppService orderAppService, IOrderDetailAppService orderDetailAppService, ICartAppService cartAppService, IProductAppService productAppService)
		{
			_orderAppService = orderAppService;
			_orderDetailAppService = orderDetailAppService;
			_cartAppService = cartAppService;
			_productAppService = productAppService;
		}

		public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequestDto input)
		{
			if (input == null || input.Items == null || !input.Items.Any())
			{
				return Json(new { success = false, message = "Dữ liệu không hợp lệ!" });
			}

			try
			{
				// Tạo đơn hàng
				var newOrder = await _orderAppService.CreateOrder(new CreateOrderDto
				{
					UserId = input.UserId,
					NameUser = input.NameUser,
					TotalAmount = input.TotalAmount,
					DiscountAmount = input.DiscountAmount,
					PaymentMethod = input.PaymentMethod,
					OrderStatus = 0
				});
				// Lưu từng sản phẩm vào bảng OrderDetails
				foreach (var item in input.Items)
				{
					await _orderDetailAppService.CreateOrderDetail(new OrderDetails.Dto.OrderDetailDto
					{
						OrderId = newOrder,
						ProductId = item.ProductId,
						Quantity = item.Quantity,
						UnitPrice = item.UnitPrice,
						DiscountPrice = item.DiscountPrice,
					});
				}

				await _cartAppService.ClearCart(input.UserId);

				// Trả về OrderId để hiển thị trong trang thành công
				return Json(new { orderId = newOrder });
			}
			catch (Exception ex)
			{
				return StatusCode(500, "Lỗi khi đặt hàng: " + ex.Message);
			}
		}

		public async Task<IActionResult> Success(int orderId)
		{
			var order = await _orderAppService.GetOrderById(orderId);
			var orderDetail = await _orderDetailAppService.GetOrderListById(orderId);
			var productIds = orderDetail.Select(od => od.ProductId).ToList();
			var products = await _productAppService.GetProductByIds(productIds);

			var model = new OrderViewSuccess
			{
				Order = order,
				OrderListDetail = orderDetail,
				ProductList = products,
			};

			return View("Success", model);
		}
	}
}
