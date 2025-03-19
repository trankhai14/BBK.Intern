using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyProject.Carts;
using MyProject.Carts.Dto;
using MyProject.Categories;
using MyProject.Controllers;
using MyProject.Product.Dtos;
using MyProject.Products;
using MyProject.Web.Models.Carts;

namespace MyProject.Web.Controllers
{
	[AbpMvcAuthorize]
	public class CartsController : MyProjectControllerBase
	{
		private readonly ICartAppService _cartAppService;
		private readonly IProductAppService _productAppService;

		public CartsController(ICartAppService cartAppService, IProductAppService productAppService)
		{
			_cartAppService = cartAppService;
			_productAppService = productAppService;
		}

		public async Task<ActionResult> Index()
		{
			ViewData["HideFooter"] = true;
			var cartItems = new List<CartViewModel>();
			var carts = await _cartAppService.GetAllCart();
			foreach(var item in carts)
			{
				var product = await _productAppService.GetAsync(new Abp.Application.Services.Dto.EntityDto<int>
				{
					Id = item.ProductId
					
				});
				//product.Price = item.Quantity * product.Price;
				cartItems.Add(new CartViewModel
				{
					Id = product.Id,
					Name = product.Name,
					Price = product.Price,
					TotalPrice = item.Quantity * product.Price,
					Quantity = item.Quantity,
					Image = product.Image,
				});
			}

			return View(cartItems);
		}



	}
}
