using MyProject.Categories.Dto;
using MyProject.Product.Dtos;
using System.Collections.Generic;
using static MyProject.Products.Product;

namespace MyProject.Web.Models.Products
{
	public class ProductViewModel
	{
		public IReadOnlyList<ProductListDto> Products;
		public List<CategoryListDto> CategoryLists { get; set; }

		//public List<ProductListDto> Products { get; set; } = new List<ProductListDto>();
		public ProductViewModel(IReadOnlyList<ProductListDto> products)
		{
			Products = products;
		}

		public int CurrentPage { get; set; }
		public int TotalPages { get; set; }

		public string GetProductLabel(ProductListDto product)
		{
			switch (product.State)
			{
				case ProductState.Available:
					return "label-success"; // Xanh lá cho sản phẩm còn hàng
				case ProductState.OutOfStock:
					return "label-warning"; // Vàng cho sản phẩm hết hàng
				case ProductState.Discontinued:
					return "label-danger"; // Đỏ cho sản phẩm ngừng kinh doanh
				default:
					return "label-default"; // Xám mặc định
			}
		}

	}
}
