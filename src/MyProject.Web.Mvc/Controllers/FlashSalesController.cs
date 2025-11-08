using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyProject.Authorization;
using MyProject.Controllers;
using MyProject.FlashSales;
using MyProject.FlashSales.Dto;

namespace MyProject.Web.Controllers
{
	[AbpMvcAuthorize(PermissionNames.Pages_Products)]
	public class FlashSalesController : MyProjectControllerBase
	{
		private readonly IFlashSaleAppService _flashSaleAppService;

		public FlashSalesController(IFlashSaleAppService flashSaleAppService)
		{
			_flashSaleAppService = flashSaleAppService;
		}

		public async Task<ActionResult> Index(GetAllFlashSalesInput input)
		{
			if (input == null)
			{
				input = new GetAllFlashSalesInput();
			}

			var output = await _flashSaleAppService.GetAll(input);
			return View(output);
		}

		/// <summary>
		/// Hiển thị modal tạo mới FlashSale
		/// Được sử dụng khi người dùng click nút "Tạo mới" trong danh sách FlashSale
		/// </summary>
		/// <returns>PartialView _CreateModal với model CreateFlashSaleDto rỗng</returns>
		public async Task<ActionResult> CreateModal()
		{
			var createDto = new CreateFlashSaleDto
			{
				IsActive = true, // Mặc định là active
				IsHidden = false // Mặc định là không ẩn
			};
			return PartialView("_CreateModal", createDto);
		}

		public async Task<ActionResult> EditModal(int flashSaleId)
		{
			var flashSale = await _flashSaleAppService.GetById(flashSaleId);
			var updateDto = new UpdateFlashSaleDto
			{
				Id = flashSale.Id,
				Name = flashSale.Name,
				Description = flashSale.Description,
				StartTime = flashSale.StartTime,
				EndTime = flashSale.EndTime,
				IsActive = flashSale.IsActive,
				IsHidden = flashSale.IsHidden
			};

			return PartialView("_EditModal", updateDto);
		}

		public async Task<ActionResult> Detail(int flashSaleId)
		{
			var flashSale = await _flashSaleAppService.GetById(flashSaleId);
			return View(flashSale);
		}

		public async Task<ActionResult> EditProductModal(int flashSaleProductId)
		{
			var flashSaleProduct = await _flashSaleAppService.GetFlashSaleProductById(flashSaleProductId);
			var updateDto = new AddProductToFlashSaleDto
			{
				FlashSaleId = flashSaleProduct.FlashSaleId,
				ProductId = flashSaleProduct.ProductId,
				FlashSalePrice = flashSaleProduct.FlashSalePrice,
				FlashSaleQuantity = flashSaleProduct.FlashSaleQuantity,
				MaxQuantityPerUser = flashSaleProduct.MaxQuantityPerUser
			};

			ViewBag.FlashSaleProductId = flashSaleProductId;
			return PartialView("_EditProductModal", updateDto);
		}
	}
}

