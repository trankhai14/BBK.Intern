using System.Threading.Tasks;
using Abp.Application.Navigation;
using Abp.Configuration.Startup;
using Abp.Runtime.Session;
using Microsoft.AspNetCore.Mvc;
using MyProject.Categories;
using MyProject.Categories.Dto;
using MyProject.Sessions;
using MyProject.Web.Views.Shared.Components.SideBarMenu;
using MyProject.Web.Views.Shared.Components.SideBarUserArea;

namespace MyProject.Web.Views.Shared.Components.HeaderMenu
{
	public class HeaderMenuViewComponent : MyProjectViewComponent
	{
		private readonly ISessionAppService _sessionAppService;
		private readonly IMultiTenancyConfig _multiTenancyConfig;
		private readonly ICategoryFontendAppService _categoryFontendAppService;

		public HeaderMenuViewComponent(
				ISessionAppService sessionAppService,
				IMultiTenancyConfig multiTenancyConfig, 
				ICategoryFontendAppService categoryFontendAppService)
		{
			_sessionAppService = sessionAppService;
			_multiTenancyConfig = multiTenancyConfig;
			_categoryFontendAppService = categoryFontendAppService;
		}

		public async Task<IViewComponentResult> InvokeAsync()
		{
			var model = new HeaderMenuViewModel
			{
				LoginInformations = await _sessionAppService.GetCurrentLoginInformations(),
				IsMultiTenancyEnabled = _multiTenancyConfig.IsEnabled,
				//Categories = await _categoryFontendAppService.GetCategory(new GetAllCategoriesInput();
			};

			return View(model);
		}


	}
}
