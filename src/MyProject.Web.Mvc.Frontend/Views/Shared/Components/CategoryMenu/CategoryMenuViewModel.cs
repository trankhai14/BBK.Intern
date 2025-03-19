using Abp.Application.Services.Dto;
using MyProject.Categories.Dto;

namespace MyProject.Web.Views.Shared.Components.CategoryMenu
{
	public class CategoryMenuViewModel
	{
		public ListResultDto<CategoryListDto> Categories { get; set; }
	}
}
