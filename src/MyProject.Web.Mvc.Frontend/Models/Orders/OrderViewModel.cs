using System.Collections.Generic;
using MyProject.Carts.Dto;
using MyProject.Products.Dtos;
using MyProject.Web.Models.Carts;

namespace MyProject.Web.Models.Orders
{
	public class OrderViewModel
	{
		 public long UserId { get; set; }
		//public List<CartViewModel> Carts { get; set; }
		public List<ProductDetailDto> Products { get; set; }
		//public string PaymentMethod { get; set; }

	}
}
