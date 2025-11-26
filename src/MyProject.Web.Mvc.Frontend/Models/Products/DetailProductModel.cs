using System;
using System.Collections.Generic;
using MyProject.Product.Dtos;
using MyProject.FlashSales.Dto;
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
		
		// Thông tin kỹ thuật điện thoại
		public string Sku { get; set; }
		public string ModelNumber { get; set; }
		public string Chipset { get; set; }
		public string Ram { get; set; }
		public string Storage { get; set; }
		public string Screen { get; set; }
		public string OperatingSystem { get; set; }
		public string Battery { get; set; }
		public string Camera { get; set; }
		public string FrontCamera { get; set; }
		public string Sim { get; set; }
		public string Connectivity { get; set; }
		public string Security { get; set; }
		public string Charging { get; set; }
		public string ChargingPort { get; set; }
		public string Color { get; set; }
		public string Warranty { get; set; }
		public string TechnicalSpecifications { get; set; }
		
		// Sản phẩm tương tự
		public List<ProductListDto> RelatedProducts { get; set; }

		// FlashSale information
		public FlashSaleProductDto FlashSaleProduct { get; set; }
	}
}
