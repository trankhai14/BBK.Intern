using System.Collections.Generic;
using MyProject.Categories.Dto;
using MyProject.Product.Dtos;


namespace MyProject.Web.Models.Products
{
	public class FilterProductsModel
	{
		public List<CategoryListDto> Categories { get; set; } = new List<CategoryListDto>(); // Danh sách danh mục
		public List<ProductListDto> StatusList { get; set; } = new List<ProductListDto>(); // Danh sách trạng thái
	}
}

