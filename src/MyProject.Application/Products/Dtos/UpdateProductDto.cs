using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using static MyProject.Products.Product;

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
		public string Image { get; set; }
		public IFormFile ImageFile { get; set; }
	}
}
