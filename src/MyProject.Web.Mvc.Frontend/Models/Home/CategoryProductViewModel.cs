using System.Collections.Generic;
using MyProject.Categories.Dto;
using MyProject.Product.Dtos;

namespace MyProject.Web.Models.Home
{
	public class CategoryProductViewModel
	{
		public int CategoryId { get; set; }
		public string CategoryName { get; set; }
		public List<ProductListDto> Products { get; set; }
	}
}
