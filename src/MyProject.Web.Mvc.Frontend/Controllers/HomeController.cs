using Microsoft.AspNetCore.Mvc;
using Abp.AspNetCore.Mvc.Authorization;
using MyProject.Controllers;
using MyProject.Products;
using MyProject.Websites;
using Abp.Application.Services.Dto;
using MyProject.Product.Dtos;
using MyProject.Products.Dtos;
using MyProject.Web.Dto;
using MyProject.Web.Models.Web;
using System.Threading.Tasks;
using System;
using MyProject.Web.Models.Products;
using System.Linq;
using System.Collections.Generic;
using System.Drawing.Design;
using MyProject.Categories;
using MyProject.Sliders;
using MyProject.Carts;
using MyProject.Web.Models.Home;
using Abp.Collections.Extensions;
using MyProject.Web.Models.Orders;
using MyProject.OrderDetails;
using MyProject.Orders;
using static MyProject.Orders.OrderAppService;
using MyProject.Users;
using Microsoft.AspNetCore.Identity;
using MyProject.Authorization.Users;

namespace MyProject.Web.Controllers
{
	//[AbpMvcAuthorize]
	public class HomeController : MyProjectControllerBase
	{
		private readonly IProductAppService _productAppService;
		private readonly ICategoryAppService _categoryAppService;
		private readonly IWebAppService _webAppService;
		private readonly ISliderAppService _sliderAppService;
		private readonly ICartAppService _cartAppService;
		private readonly IOrderAppService _orderAppService;
		private readonly IOrderDetailAppService _orderDetailAppService;
		private readonly IUserAppService _userAppService;

		
		public HomeController
			(
			IProductAppService productAppService,
			IWebAppService webAppService,
			ICategoryAppService categoryAppService,
			ISliderAppService sliderAppService,
			ICartAppService cartAppService,
			IOrderAppService orderAppService,
			IOrderDetailAppService orderDetailAppService,
			IUserAppService userAppService
			)
		{
			_productAppService = productAppService;
			_webAppService = webAppService;
			_categoryAppService = categoryAppService;
			_sliderAppService = sliderAppService;
			_cartAppService = cartAppService;
			_orderAppService = orderAppService;
			_orderDetailAppService = orderDetailAppService;
			_userAppService = userAppService;
		}

		public async Task<IActionResult> Index(int page = 1, int pageSize = 5)
		{
			// Lấy danh sách sản phẩm theo phân trang
			var productsResult = await _productAppService.GetAll(new GetAllProductsInput
			{
				MaxResultCount = 20,
				SkipCount = 0,
				Sorting = "CreationTime DESC",
			});

			// Lấy slider
			var sliders = await _sliderAppService.GetSliderByActive();

			// Chia danh sách sản phẩm theo phân trang (hiển thị 5 sản phẩm một lần)
			var paginatedProducts = productsResult.Items.Skip((page - 1) * pageSize).Take(pageSize).ToList();

			// Khởi tạo ProductViewModel
			var productViewModel = new ProductViewModel(productsResult.Items)
			{
				CurrentPage = page,
				TotalPages = (int)Math.Ceiling((double)productsResult.Items.Count / pageSize),
				SliderList = sliders
			};


			// Lấy danh sách chuyên mục + sản phẩm
			List<CategoryProductViewModel> categoryProductViewModels = new List<CategoryProductViewModel>();

			var categories = await _categoryAppService.GetAllCategory();
			if (categories != null)
			{
				foreach (var item in categories)
				{
					var productsOfCategory = await _productAppService.Search(new GetAllProductsInput
					{
						CategoryId = item.Id
					});

					categoryProductViewModels.Add(new CategoryProductViewModel
					{
						CategoryId = item.Id,
						CategoryName = item.CategoryName,
						Products = productsOfCategory.Items.Take(10).ToList()
					});
				}
			}

			var homePageViewModel = new HomePageViewModel
			{
				ProductData = productViewModel,
				CategoryProducts = categoryProductViewModels
			};

			return View(homePageViewModel);
		}



		public async Task<IActionResult> SearchProductsWeb(GetAllProductWeb input)
		{
			//ProductViewModel webViewModel = new ProductViewModel();
			var result = await _productAppService.Search(new GetAllProductsInput
			{
				Keyword = input.Keyword
			});

			var count = result.Items.Count();

			if (result != null)
			{
				var model = new ProductViewModel(result.Items)
				{
					count = count
				};
				return View("_ProducResultSearch", model);
			}
			return View("_ProducResultSearch", new ProductViewModel(new List<ProductListDto>()));
		}

	

		public async Task<IActionResult> GetDetailProduct(EntityDto<int> productId)
		{
			var product = await _productAppService.GetAsync(productId);
			var category = await _categoryAppService.GetCategoryById(product.CategoryId);

			var model = new DetailProductModel()
			{
				Id = product.Id,
				Name = product.Name,
				Description = product.Description,
				Price = product.Price,
				Image = product.Image,
			};

			model.CategoryName = category.CategoryName;
			return View("_DetailProductWeb", model);
		}

		public async Task<IActionResult> PageAllProduct(int? categoryId)
		{
			var allProducts = await _productAppService.GetAllProducts();
			string NameBreakCrum = "Tất cả sản phẩm";

			//lấy name của category
			if (categoryId != null)
			{
				var category = await _categoryAppService.GetCategoryById(categoryId);
				if (category != null)
				{
					NameBreakCrum = category.CategoryName;
					allProducts = allProducts.Where(p => p.CategoryId == categoryId.Value).ToList();
				}
			}


			var selectedProducts = allProducts.Take(10).ToList();
			var model = new ProductViewModel(selectedProducts)
			{
				CategoryId = categoryId,
				CategoryName = NameBreakCrum,
			};
			return View("_AllProducts", model); // Trả về View
		}

		public async Task<IActionResult> LoadMoreProducts(int? categoryId, string? sortOrder, int page, int pageSize = 10)
		{
			var allProducts = await _productAppService.GetAllProducts();

			switch (sortOrder)
			{
				case "price_asc":
					allProducts = allProducts.OrderBy(p => p.Price).ToList();
					break;
				case "price_desc":
					allProducts = allProducts.OrderByDescending(p => p.Price).ToList();
					break;
				default:
					break;
			}

			if (categoryId.HasValue)
			{
				allProducts = allProducts.Where(p => p.CategoryId == categoryId.Value).ToList();
			}

			// sắp xếp theo giá

			var products = allProducts
					.Skip((page - 1) * pageSize)
					.Take(pageSize)
					.ToList();

			if (products.Any())
			{
				return PartialView("_GetProductPage", new ProductViewModel(products)); // Trả về PartialView
			}

			return NoContent();  // Trả về rỗng nếu không còn sản phẩm
		}


		public IActionResult UserProfile()
		{
			return View("ProfileUser");
		}

		public async Task<IActionResult> FilterStatus(int? orderStatus = 5)
		{

			var orderOutputs = await _orderAppService.GetStatusOrder(orderStatus);
			var orderIds = orderOutputs.Select(o => o.OrderId).ToList(); // Lấy danh sách ID
			var orderList = await _orderDetailAppService.GetOrderByIdAndStatus(orderIds);
			var orderStatusDict = orderOutputs.ToDictionary(o => o.OrderId, o => o.OrderStatus);

			// Gán OrderStatus cho từng đơn hàng trong orderList
			foreach (var order in orderList)
			{
				if (orderStatusDict.TryGetValue(order.OrderId, out var status))
				{
					order.OrderStatus = status;
				}
			}

			// Lấy danh sách ID sản phẩm cần lấy thông tin
			var productIds = orderList.Select(o => o.ProductId).Distinct().ToList();
			var productList = await _productAppService.GetProductByIds(productIds);
			var model = new FilterStatusOrderViewModel
			{
				ListOrder = orderList,
				OrderStatus = orderStatus,
				Products = productList
			};
			return PartialView("FilterStatus", model);
		}

		public async Task<IActionResult> LoadPartialView(string nameView)
		{
			if (nameView == "_UserInfos")
			{
				var userId = AbpSession.UserId ?? 0; // 0 là giá trị mặc định
				var user = await _userAppService.GetUserById(userId);
				var model = new ProfileUser
				{
					User = user,
				};
				return PartialView("_UserInfos", model);
			}
				return PartialView("_OrderList");
		}

		public async Task<IActionResult> GetInforDetailOrder(int orderId)
		{
			var order = await _orderAppService.GetOrderById(orderId);
			var orderDetail = await _orderDetailAppService.GetOrderListById(orderId);

			var productIds = orderDetail.Select(od => od.ProductId).ToList();
			var products = await _productAppService.GetProductByIds(productIds);

			var model = new OrderViewSuccess
			{
				Order = order,
				OrderListDetail = orderDetail,
				ProductList = products
			};

			return PartialView("GetInforDetailOrder", model);
		}
	}
}