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
		public HomeController
			(
			IProductAppService productAppService,
			IWebAppService webAppService,
			ICategoryAppService categoryAppService,
			ISliderAppService sliderAppService,
			ICartAppService cartAppService
			)
		{
			_productAppService = productAppService;
			_webAppService = webAppService;
			_categoryAppService = categoryAppService;
			_sliderAppService = sliderAppService;
			_cartAppService = cartAppService;
		}

		//public async Task<IActionResult> Index(int page = 1, int pageSize = 8)
		//{
		//	var productsResult = await _productAppService.GetAll(new GetAllProductsInput
		//	{
		//		MaxResultCount = pageSize,
		//		SkipCount = (page - 1) * pageSize,
		//	});
		//	//var countCart = await _cartAppService.CountCart();
		//	var sliders = await _sliderAppService.GetSliderByActive();



		//	var model = new ProductViewModel(productsResult.Items)
		//	{
		//		CurrentPage = page,
		//		TotalPages = (int)Math.Ceiling((double)productsResult.TotalCount / pageSize),
		//	};

		//	model.SliderList = sliders;
		//	//model.countItemCart = countCart;


		//	List<CategoryProductViewModel> categoryProductViewModels = new List<CategoryProductViewModel>();
		//	#region Get all category
		//	var categories = await _categoryAppService.GetAllCategory();
		//	if (categories != null)
		//	{
		//		foreach (var item in categories)
		//		{
		//			var productsOfCategory = await _productAppService.GetAll(new GetAllProductsInput
		//			{
		//				CategoryId = item.Id
		//			});
		//			categoryProductViewModels.Add(new CategoryProductViewModel
		//			{
		//				CategoryId = item.Id,
		//				CategoryName = item.CategoryName,
		//				Products = productsOfCategory.Items.ToList()
		//			});
		//		}
		//	}


		//	#endregion

		//	return View(model);
		//}
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
			WebViewModel webViewModel = new WebViewModel();
			var result = await _productAppService.Search(new GetAllProductsInput
			{
				Keyword = input.Keyword
			});

			var count = result.Items.Count();

			if (result != null)
			{
				webViewModel.Products = result.Items.ToList();
				webViewModel.Keyword = input.Keyword;
				webViewModel.count = count;
			}
			return View("_ProductListPartial", webViewModel);
		}

		public async Task<IActionResult> GetProductByCategory(int page = 1, int pageSize = 10, int? categoryId = null)
		{
			var productsResult = await _productAppService.Search(new GetAllProductsInput
			{
				MaxResultCount = pageSize,
				SkipCount = (page - 1) * pageSize,
				CategoryId = categoryId,
				//CategoryName = categoryName
			});

			var category = await _categoryAppService.DetailCategory(new EntityDto<int>(categoryId.Value));

			var model = new ProductViewModel(productsResult.Items)
			{
				CurrentPage = page,
				TotalPages = (int)Math.Ceiling((double)productsResult.TotalCount / pageSize),
				CategoryId = categoryId,
				CategoryName = category.CategoryName
			};

			return View("_ProductGetByCategory", model);
		}

		public async Task<IActionResult> GetDetailProduct(EntityDto<int> productId)
		{
			var product = await _productAppService.GetAsync(productId);

			var model = new DetailProductModel()
			{
				Id = product.Id,
				Name = product.Name,
				Description = product.Description,
				Price = product.Price,
				Image = product.Image,
			};
			return View("_DetailProductWeb", model);
		}


		//public async Task<IActionResult> PageAllProduct(int page, int pageSize = 10)
		//{
		//	var allProducts = await _productAppService.GetAllProducts();
		//	var products = allProducts
		//			.Skip((page - 1) * pageSize)
		//			.Take(pageSize)
		//			.ToList();

		//	if (products.Any())
		//	{
		//		var model = new ProductViewModel(products);
		//		return PartialView("_GetProductPage", model);
		//	}

		//	return Content("");
		//}

		public async Task<IActionResult> PageAllProduct(int? categoryId)
		{
			var allProducts = await _productAppService.GetAllProducts();
			string NameBreakCrum = "Tất cả sản phẩm";

			//lấy name của category
			if (categoryId != null) {
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

		public async Task<IActionResult> LoadMoreProducts(int? categoryId, string sortOrder , int page, int pageSize = 10)
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

			return null; // Trả về rỗng nếu không còn sản phẩm
		}


	}
}