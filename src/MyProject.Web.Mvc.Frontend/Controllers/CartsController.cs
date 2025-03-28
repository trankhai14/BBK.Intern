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
using MyProject.Users;
using MyProject.Web.Models.Carts;

namespace MyProject.Web.Controllers
{
	[AbpMvcAuthorize]
	public class CartsController : MyProjectControllerBase
	{
		private readonly ICartAppService _cartAppService;
		private readonly IProductAppService _productAppService;
		private readonly IUserAppService _userAppService;

		public CartsController(ICartAppService cartAppService, IProductAppService productAppService, IUserAppService userAppService)
		{
			_cartAppService = cartAppService;
			_productAppService = productAppService;
			_userAppService = userAppService;
		}

		public async Task<ActionResult> Index()
		{
			ViewData["HideFooter"] = true;

			// Lấy UserId từ AbpSession
			var userId = AbpSession.UserId ?? 0; // Nếu null thì gán 0
			var nameUser = await _userAppService.GetNameUser(userId);

			var cartItems = new CartViewListModel
			{
				UserId = userId,
				NameUser = nameUser,
				Carts = new List<CartViewModel>() // Khởi tạo danh sách rỗng
			};

			// Lấy danh sách giỏ hàng
			var carts = await _cartAppService.GetAllCart();

			foreach (var item in carts)
			{
				var product = await _productAppService.GetAsync(new Abp.Application.Services.Dto.EntityDto<int>
				{
					Id = item.ProductId
				});

				// Thêm sản phẩm vào danh sách giỏ hàng
				cartItems.Carts.Add(new CartViewModel
				{
					Id = product.Id,
					Name = product.Name,
					Price = product.Price,
					TotalPrice = item.Quantity * product.Price,
					Quantity = item.Quantity,
					Image = product.Image
				});
			}

			return View(cartItems);
		}



	}
}
