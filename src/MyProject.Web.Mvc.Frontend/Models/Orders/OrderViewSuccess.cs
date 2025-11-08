using System.Collections.Generic;
using MyProject.OrderDetails.Dto;
using MyProject.Orders.Dto;
using MyProject.Product.Dtos;
using MyProject.CustomerProfiles.Dto;

namespace MyProject.Web.Models.Orders
{
	public class OrderViewSuccess
	{
		public OrderListDto Order {  get; set; }
		public List<OrderDetailDto> OrderListDetail { get; set; }
		public List<ProductListDto> ProductList { get; set; }
		public CustomerProfileDto CustomerProfile { get; set; } // Thông tin khách hàng từ CustomerProfile
	}
}
