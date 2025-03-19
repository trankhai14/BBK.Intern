using System.Collections.Generic;
using MyProject.Web.Models.Products;

namespace MyProject.Web.Models.Home
{
	public class HomePageViewModel
	{
		public ProductViewModel ProductData { get; set; }
		public List<CategoryProductViewModel> CategoryProducts { get; set; }

	}
}
