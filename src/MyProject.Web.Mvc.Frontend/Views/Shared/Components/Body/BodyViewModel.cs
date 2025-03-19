using MyProject.Categories.Dto;
using MyProject.Product.Dtos;
using static MyProject.Products.Product;
using System.Collections.Generic;

namespace MyProject.Web.Views.Shared.Components.Body
{
	public class BodyViewModel
	{
		public IReadOnlyList<ProductListDto> Products;
		public List<CategoryListDto> CategoryLists { get; set; }

		public BodyViewModel(IReadOnlyList<ProductListDto> products)
		{
			Products = products;
		}

		public int CurrentPage { get; set; }
		public int TotalPages { get; set; }

	
	}
}
