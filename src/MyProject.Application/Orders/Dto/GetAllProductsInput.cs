using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using static MyProject.Products.Product;

namespace MyProject.Orders.Dto
{
	public class GetAllOrdersInput : PagedAndSortedResultRequestDto
	{
		public long? orderId { get; set; }
		public string? NameUser { get; set; }
		public int? OrderStatus { get; set; }
		public int? PaymentMethod { get; set; }
		public DateTime? FromDate { get; set; }
		public DateTime? ToDate { get; set; }
	}
}
