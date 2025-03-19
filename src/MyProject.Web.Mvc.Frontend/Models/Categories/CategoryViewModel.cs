using System.Collections.Generic;
using MyProject.Categories.Dto;
using static MyProject.Categories.Category;

namespace MyProject.Web.Models.Categories
{
	public class CategoryViewModel
	{
		public IReadOnlyList<CategoryListDto> Categories;

		public CategoryViewModel(IReadOnlyList<CategoryListDto> categories)
		{
			this.Categories = categories;
		}
	}
}
