using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using MyProject.Categories.Dto;


namespace MyProject.Categories
{
	public interface ICategoryAppService : IApplicationService
	{
		Task<PagedResultDto<CategoryListDto>> GetCategory(GetAllCategoriesInput input);
		Task<List<CategoryListDto>> GetAllCategory();
		Task<CategoryListDto> CreateCategory(CreateCategoryDto input);
		Task<CategoryListDto> GetEdit(EntityDto<int> input);
		Task DeleteCategory(EntityDto<int> input);
		Task<CategoryListDto> DetailCategory(EntityDto<int> input);
		Task<PagedResultDto<CategoryListDto>> SearchCategory(GetAllCategoriesInput input);
		Task<CategoryListDto> GetCategoryById(int? categoryId);
	}
}
