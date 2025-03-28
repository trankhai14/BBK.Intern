using System.Collections.Generic;
using MyProject.OrderDetails.Dto;
using MyProject.Orders.Dto;
using MyProject.Product.Dtos;

namespace MyProject.Web.Models.Orders
{
	public class FilterStatusOrderViewModel
	{
		public List<OrderDetailDto> ListOrder { get; set; }
		public int? OrderStatus { get; set; }
		public List<ProductListDto> Products { get; set; }
	}
}
