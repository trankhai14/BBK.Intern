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
		
		// Thông tin phân trang
		public int CurrentPage { get; set; } = 1;
		public int PageSize { get; set; } = 10;
		public int TotalCount { get; set; }
		public int TotalPages => (int)System.Math.Ceiling((double)TotalCount / PageSize);
		public bool HasPreviousPage => CurrentPage > 1;
		public bool HasNextPage => CurrentPage < TotalPages;
	}
}
