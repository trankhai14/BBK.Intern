using System.Collections.Generic;
using System.Linq;
using MyProject.OrderDetails.Dto;
using MyProject.Orders.Dto;
using MyProject.Product.Dtos;
using MyProject.CustomerProfiles.Dto;

namespace MyProject.Web.Models.Orders
{
	public class DetailOrderViewModel
	{
		public OrderListDto Order { get; set; }
		public List<OrderDetailDto> OrderList { get; set; }
		public List<ProductListDto> ProductList { get; set; }
		public CustomerProfileDto CustomerProfile { get; set; }

		public ProductListDto GetProductById(int productId)
		{
			return ProductList?.FirstOrDefault(p => p.Id == productId);
		}
	}
}
