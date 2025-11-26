using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyProject.Controllers;
using MyProject.OrderDetails;
using MyProject.Orders;
using MyProject.Products;
using MyProject.CustomerProfiles;
using MyProject.CustomerProfiles.Dto;
using MyProject.Web.Models.Orders;

namespace MyProject.Web.Controllers
{
	public class OrdersController: MyProjectControllerBase
	{
		private readonly IOrderDetailAppService _orderDetailAppService;
		private readonly IOrderAppService _orderAppService;
		private readonly IProductAppService _productAppService;
		private readonly ICustomerProfileAppService _customerProfileAppService;


		public OrdersController(IOrderDetailAppService orderDetailAppService, IProductAppService productAppService, IOrderAppService orderAppService, ICustomerProfileAppService customerProfileAppService)
		{
			_orderDetailAppService = orderDetailAppService;
			_productAppService = productAppService;
			_orderAppService = orderAppService;
			_customerProfileAppService = customerProfileAppService;
		}
		public ActionResult Index()
		{
			return View();
		}

		public async Task<IActionResult> DetailOrder(int orderId)
		{
			// Lấy thông tin đơn hàng
			var order = await _orderAppService.GetOrderById(orderId);
			
			// Lấy chi tiết đơn hàng
			var orderDetail = await _orderDetailAppService.GetAllOrder(orderId);

			// Lấy thông tin sản phẩm
			var productIds = orderDetail.Select(x => x.ProductId).Distinct().ToList();
			var products = await _productAppService.GetProductByIds(productIds);

			// Lấy thông tin CustomerProfile từ UserId của đơn hàng
			CustomerProfileDto customerProfile = null;
			if (order.UserId > 0)
			{
				var profilesInput = new GetAllCustomerProfilesInput
				{
					UserId = order.UserId,
					MaxResultCount = 100
				};
				var profilesResult = await _customerProfileAppService.GetAll(profilesInput);
				// Lấy profile mặc định hoặc profile đầu tiên
				customerProfile = profilesResult.Items.FirstOrDefault(p => p.IsDefault) ?? profilesResult.Items.FirstOrDefault();
			}

			var model = new DetailOrderViewModel
			{
				Order = order,
				OrderList = orderDetail,
				ProductList = products,
				CustomerProfile = customerProfile
			};
			return View("DetailOrder", model);
		}

		public async Task<IActionResult> EditOrderModal(int orderId)
		{
			var order = await _orderAppService.GetOrderById(orderId);

			var model = new OrderViewModel
			{
				Order = order,
			};

			return PartialView("EditOrderModal", model);
		}
	}
}
