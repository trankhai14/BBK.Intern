using System.Collections.Generic;
using System.Linq;
using MyProject.OrderDetails.Dto;
using MyProject.Product.Dtos;

namespace MyProject.Web.Models.Orders
{
	public class DetailOrderViewModel
	{
		public List<OrderDetailDto> OrderList { get; set; }
		public List<ProductListDto> ProductList { get; set; }

		public ProductListDto GetProductById(int productId)
		{
			return ProductList?.FirstOrDefault(p => p.Id == productId);
		}
	}
}
