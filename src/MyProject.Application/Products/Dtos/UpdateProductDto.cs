using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using static MyProject.Products.Product;
using MyProject.Products.Dtos;

namespace MyProject.Products.Dtos
{
	public class UpdateProductDto
	{
		public int Id { get; set; }
		public string Name { get; set; }

		public string Description { get; set; }

		public decimal Price { get; set; }

		public ProductState State { get; set; }


		public int CategoryId { get; set; }
		public int? SupplierId { get; set; }
		public string Image { get; set; }
		public string Brand { get; set; }
		public int? WeightInGrams { get; set; }
		public decimal? WidthCm { get; set; }
		public decimal? HeightCm { get; set; }
		public decimal? LengthCm { get; set; }
		public IFormFile ImageFile { get; set; }

		// Thông tin kỹ thuật (tách riêng)
		public UpdateProductSpecificationDto Specification { get; set; }
	}
}
