using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using static MyProject.Categories.Category;

namespace MyProject.Web.Models.Products
{
	public class CreateProductViewModel
	{
		public List<SelectListItem> Categories { get; set; }
		public string ProductName { get; set; }
		public string ProductDescription { get; set; }
		public int CategoryId { get; set; }

		public CreateProductViewModel(List<SelectListItem> categories) 
		{
			Categories = categories;
		}
	}
}
