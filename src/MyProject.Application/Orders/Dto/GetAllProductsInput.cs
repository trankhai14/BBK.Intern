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
	}
}
