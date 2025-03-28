using System.Collections.Generic;
using MyProject.OrderDetails.Dto;
using MyProject.Orders.Dto;
using MyProject.Product.Dtos;

namespace MyProject.Web.Models.Orders
{
	public class OrderViewSuccess
	{
		public OrderListDto Order {  get; set; }
		public List<OrderDetailDto> OrderListDetail { get; set; }
		public List<ProductListDto> ProductList { get; set; }
	}
}
