using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyProject.Categories.Dto;
using MyProject.Product.Dtos;
using MyProject.Suppliers.Dto;

namespace MyProject.Web.Models.Products
{
	public class EditProductViewModel
	{
		public ProductListDto Product { get; set; }
		public List<CategoryListDto> Categories { get; set; } // Danh sách danh mục sản phẩm
		public List<SupplierDto> Suppliers { get; set; } // Danh sách nhà cung cấp
		public List<SelectListItem> ProductStateList { get; set; } // Danh sách trạng thái sản phẩm

	}
}
