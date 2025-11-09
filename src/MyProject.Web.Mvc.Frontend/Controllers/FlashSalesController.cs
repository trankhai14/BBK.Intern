using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyProject.Controllers;
using MyProject.FlashSales;
using MyProject.FlashSales.Dto;
using MyProject.Web.Models.FlashSales;

namespace MyProject.Web.Controllers
{
	/// <summary>
	/// Controller xử lý FlashSale cho Frontend
	/// </summary>
	public class FlashSalesController : MyProjectControllerBase
	{
		private readonly IFlashSaleAppService _flashSaleAppService;

		public FlashSalesController(IFlashSaleAppService flashSaleAppService)
		{
			_flashSaleAppService = flashSaleAppService;
		}

		/// <summary>
		/// Trang danh sách FlashSale đang diễn ra
		/// </summary>
		public async Task<IActionResult> Index()
		{
			// Lấy danh sách FlashSale đang diễn ra
			var flashSales = await _flashSaleAppService.GetOngoingFlashSales();

			// Nếu không có FlashSale nào, trả về view với danh sách rỗng
			var model = new FlashSaleListViewModel
			{
				FlashSales = flashSales ?? new List<FlashSaleDto>()
			};

			return View(model);
		}

		/// <summary>
		/// Trang chi tiết FlashSale
		/// </summary>
		public async Task<IActionResult> Detail(int id)
		{
			try
			{
				// Lấy thông tin FlashSale
				var flashSale = await _flashSaleAppService.GetById(id);

				if (flashSale == null)
				{
					return NotFound();
				}

				var model = new FlashSaleDetailViewModel
				{
					FlashSale = flashSale
				};

				return View(model);
			}
			catch (Exception)
			{
				return NotFound();
			}
		}

		/// <summary>
		/// API: Lấy danh sách sản phẩm trong FlashSale (JSON)
		/// </summary>
		[HttpGet]
		public async Task<IActionResult> GetFlashSaleProducts(int flashSaleId)
		{
			try
			{
				var products = await _flashSaleAppService.GetFlashSaleProductsByFlashSaleId(flashSaleId);
				return Json(new { success = true, data = products });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = ex.Message });
			}
		}

		/// <summary>
		/// API: Kiểm tra sản phẩm có trong FlashSale đang diễn ra không (JSON)
		/// </summary>
		[HttpGet]
		public async Task<IActionResult> CheckProductInFlashSale(int productId)
		{
			try
			{
				var flashSaleProduct = await _flashSaleAppService.GetFlashSaleProductByProductId(productId);
				
				if (flashSaleProduct == null)
				{
					return Json(new { success = true, hasFlashSale = false });
				}

				return Json(new { success = true, hasFlashSale = true, data = flashSaleProduct });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = ex.Message });
			}
		}
	}
}

