using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using MyProject.Carts.Dto;
using MyProject.Product.Dtos;
using static MyProject.Products.Product;

namespace MyProject.Web.Models.Carts
{
	public class CartViewModel
	{
		public long Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }

		public decimal Price;

		public decimal TotalPrice { get; set; }
		public DateTime CreationTime { get; set; }

		public ProductState State { get; set; }

		public IFormFile ImageFile { get; set; }  // Thêm thuộc tính này
		public string Image { get; set; }  // Lưu đường dẫn ảnh
		public long Quantity { get; set; }

	}
	
}
