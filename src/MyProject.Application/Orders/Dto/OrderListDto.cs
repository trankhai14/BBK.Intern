using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject.Orders.Dto
{
	public class OrderListDto
	{
		public int Id { get; set; }
		public long UserId { get; set; }
		public string NameUser { get; set; }	
		public decimal TotalAmount { get; set; }
		public decimal DiscountAmount { get; set; }
		public int PaymentMethod { get; set; }
		public int OrderStatus { get; set; }
		public DateTime CreationTime {get; set; }
		public string PhoneNumber { get; set; }
		public string ShippingAddress { get; set; }
		public string PaymentReference { get; set; }
		public bool IsPaid { get; set; }
		public DateTime? PaidTime { get; set; }
	}
}
