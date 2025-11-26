using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyProject.Carts;
using MyProject.Carts.Dto;
using MyProject.Categories;
using MyProject.Controllers;
using MyProject.FlashSales;
using MyProject.FlashSales.Dto;
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
		private readonly IFlashSaleAppService _flashSaleAppService;

		public CartsController(ICartAppService cartAppService, IProductAppService productAppService, IUserAppService userAppService, IFlashSaleAppService flashSaleAppService)
		{
			_cartAppService = cartAppService;
			_productAppService = productAppService;
			_userAppService = userAppService;
			_flashSaleAppService = flashSaleAppService;
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

				// Kiểm tra sản phẩm có trong FlashSale đang diễn ra không
				FlashSaleProductDto flashSaleProduct = null;
				bool isFlashSale = false;
				decimal flashSalePrice = product.Price;
				decimal originalPrice = product.Price;

				try
				{
					flashSaleProduct = await _flashSaleAppService.GetFlashSaleProductByProductId(product.Id);
					if (flashSaleProduct != null)
					{
						isFlashSale = true;
						flashSalePrice = flashSaleProduct.FlashSalePrice;
						originalPrice = flashSaleProduct.OriginalPrice;
					}
				}
				catch
				{
					// Nếu không có FlashSale, bỏ qua và dùng giá bình thường
				}

				// Tính tổng tiền: nếu có FlashSale thì dùng giá FlashSale, không thì dùng giá bình thường
				decimal finalPrice = isFlashSale ? flashSalePrice : product.Price;
				decimal totalPrice = item.Quantity * finalPrice;

				// Thêm sản phẩm vào danh sách giỏ hàng
				cartItems.Carts.Add(new CartViewModel
				{
					Id = product.Id,
					Name = product.Name,
					Price = finalPrice, // Giá hiển thị (có thể là giá FlashSale hoặc giá bình thường)
					TotalPrice = totalPrice,
					Quantity = item.Quantity,
					Image = product.Image,
					// Thông tin FlashSale
					IsFlashSale = isFlashSale,
					FlashSalePrice = flashSalePrice,
					OriginalPrice = originalPrice
				});
			}

			return View(cartItems);
		}



	}
}
