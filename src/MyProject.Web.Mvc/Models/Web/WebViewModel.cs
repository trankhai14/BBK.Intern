

using System.Collections.Generic;
using MyProject.Product.Dtos;

namespace MyProject.Web.Models.Web
{
	public class WebViewModel
	{
		public List<ProductListDto> Products { get; set; } = new List<ProductListDto>();

		public ProductListDto ProductList { get; set; }

		public string Keyword { get; set; }
	}
}
