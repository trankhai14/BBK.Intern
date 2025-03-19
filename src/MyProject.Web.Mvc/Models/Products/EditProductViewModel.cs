using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyProject.Product.Dtos;

namespace MyProject.Web.Models.Products
{
	public class EditProductViewModel
	{
		public ProductListDto Product { get; set; }
		public List<SelectListItem> ProductStateList { get; set; } // Danh sách trạng thái sản phẩm

	}
}
