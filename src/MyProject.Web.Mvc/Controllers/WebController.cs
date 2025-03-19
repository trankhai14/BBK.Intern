using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyProject.Controllers;
using MyProject.EntityFrameworkCore;
using MyProject.Product.Dtos;
using MyProject.Products;
using MyProject.Products.Dtos;
using MyProject.TaskAppService;
using MyProject.Tasks;
using MyProject.Web.Dto;
using MyProject.Web.Models.Products;
using MyProject.Web.Models.Tasks;
using MyProject.Web.Models.Web;
using MyProject.Websites;

namespace MyProject.Web.Controllers
{
	public class WebController : MyProjectControllerBase
	{
		private readonly IProductAppService _productAppService;
		private readonly IWebAppService _webAppService;

		public WebController(IProductAppService productAppService, IWebAppService webAppService)
		{
			_productAppService = productAppService;
			_webAppService = webAppService;
		}


		public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
		{
			var productsResult = await _productAppService.GetAll(new GetAllProductsInput
			{
				MaxResultCount = pageSize,
				SkipCount = (page - 1) * pageSize
			});

			var model = new ProductViewModel(productsResult.Items)
			{
				CurrentPage = page,
				TotalPages = (int)Math.Ceiling((double)productsResult.TotalCount / pageSize)
			};

			return View(model);
		}


		public async Task<IActionResult> SearchProductsWeb(GetAllProductWeb input)
		{
			WebViewModel webViewModel = new WebViewModel();
			var result = await _productAppService.Search(new GetAllProductsInput
			{
				Keyword = input.Keyword
			});

			if (result != null)
			{
				webViewModel.Products = result.Items.ToList();
				webViewModel.Keyword = input.Keyword;
			}
			return View("_ProductListPartial", webViewModel);
		}

		public async Task<IActionResult> DetailProductWeb(int productId)
		{
			WebViewModel webViewModel = new WebViewModel();

			// Gọi service lấy thông tin chi tiết sản phẩm
			var product = await _productAppService.Detail(new EntityDto<int>(productId));

			if (product != null)
			{
				webViewModel.ProductList = new ProductListDto
								{
										Id = product.Id,
										Name = product.Name,
										Description = product.Description,
										Price = product.Price,
										State = product.State,
										CreationTime = product.CreationTime
								}; // Gán sản phẩm vào ViewModel
			}
				return View("_DetailProductWeb", webViewModel); // Hiển thị view

			// Nếu không tìm thấy sản phẩm, quay về trang danh sách sản phẩm
			//return RedirectToAction("Index");
		}
	}
}

