using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using MyProject.Controllers;
using MyProject.Products.Dtos;
using MyProject.Web.Models.Products;
using MyProject.Products;
using MyProject.Categories;
using MyProject.Categories.Dto;
using MyProject.Web.Models.Categories;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using MyProject.Authorization;
using Abp.AspNetCore.Mvc.Authorization;

namespace MyProject.Web.Controllers
{
	[AbpMvcAuthorize(PermissionNames.Pages_Categories)]
	public class CategoriesController : MyProjectControllerBase
	{

		private readonly ICategoryAppService _categoryAppService;
		private readonly IProductAppService _productAppService;


		public CategoriesController(ICategoryAppService categoryAppService, IProductAppService productAppService)
		{
			_categoryAppService = categoryAppService;
			_productAppService = productAppService;
		}
		public async Task<ActionResult> Index(GetAllCategoriesInput input)
		{

			// Kiểm tra xem input có phải null không
			if (input == null)
			{
				// Khởi tạo giá trị mặc định cho input nếu nó là null
				input = new GetAllCategoriesInput();
			}
			var output = await _categoryAppService.GetCategory(input);
			var model = new CategoryViewModel(output.Items);
			return View(model);
		}

		public async Task<ActionResult> EditModalCategory(int categoryId)
		{
			var category = await _categoryAppService.GetEdit(new EntityDto<int>(categoryId));

			var model = new EditCategoryViewModel
			{
				Category = category
			};

			return PartialView("_EditModal", model);
		}

		public async Task<ActionResult> Detail(int categoryId)
		{
			var category = await _categoryAppService.GetEdit(new EntityDto<int> ( categoryId ));

			var model = new EditCategoryViewModel
			{
				Category = category
			};

			return View(model);
		}

		//public async Task<IActionResult> DeleteCategoryWithCheck(int categoryId)
		//{
		//	bool hasProducts = await _productAppService.CheckProductsByCategoryId(categoryId);

		//	if (hasProducts)
		//	{
		//		return BadRequest(new { message = "Không thể xóa danh mục vì vẫn còn sản phẩm thuộc danh mục này." });
		//	}

		//	await _categoryAppService.DeleteCategory(new EntityDto<int> { Id = categoryId });

		//	return Ok(new { message = "Xóa danh mục thành công!" });
		//}

	}
}
