using System;
using System.Collections.Generic;
using MyProject.Product.Dtos;
using static MyProject.Products.Product;

namespace MyProject.Web.Models.Products
{
	public class DetailProductModel
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public decimal Price { get; set; }
		public string Image { get; set; }
		public string CategoryName { get; set; }
		public int CategoryId { get; set; }
		
		// Thông tin kỹ thuật
		public string Brand { get; set; }
		public ProductState State { get; set; }
		public DateTime CreationTime { get; set; }
		public int? WeightInGrams { get; set; }
		public decimal? WidthCm { get; set; }
		public decimal? HeightCm { get; set; }
		public decimal? LengthCm { get; set; }
		
		// Sản phẩm tương tự
		public List<ProductListDto> RelatedProducts { get; set; }
	}
}
