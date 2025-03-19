using Abp.Configuration.Startup;
using Microsoft.AspNetCore.Mvc;
using MyProject.Categories;
using MyProject.Categories.Dto;
using MyProject.Web.Views.Shared.Components.HeaderMenu;
using System.Threading.Tasks;

namespace MyProject.Web.Views.Shared.Components.CategoryMenu
{
	public class CategoryMenuViewComponent : MyProjectViewComponent
	{
		private readonly ICategoryFontendAppService _categoryFontendAppService;

		public CategoryMenuViewComponent(ICategoryFontendAppService categoryFontendAppService)
		{
			_categoryFontendAppService = categoryFontendAppService;
		}

		public async Task<IViewComponentResult> InvokeAsync()
		{
			var model = new CategoryMenuViewModel
			{
				Categories = await _categoryFontendAppService.GetCategory(new GetAllCategoriesInput())
			};

			return View(model);
		}
	}
}
