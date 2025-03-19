using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyProject.Products;
using MyProject.Products.Dtos;
using MyProject.Websites;

namespace MyProject.Web.Views.Shared.Components.Body
{
	public class BodyViewComponent : MyProjectViewComponent
	{
		private readonly IProductFontendAppService _productFontendAppService;
		public BodyViewComponent(IProductFontendAppService productFontendAppService)
		{
			_productFontendAppService = productFontendAppService;
		}
		public async Task<IViewComponentResult> InvokeAsync(int page = 1, int pageSize = 8)
		{
			var products = await _productFontendAppService.GetAllProduct(new GetAllProductsInput 
			{
				MaxResultCount = pageSize,
				SkipCount = (page - 1) * pageSize,
			});

			var model = new BodyViewModel(products.Items)
			{
				CurrentPage = page,
			};

			return View(model);
		}
	}
}
