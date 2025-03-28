using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Domain.Entities.Auditing;

namespace MyProject.OrderDetails.Dto
{
	public class OrderDetailDto : FullAuditedEntity<int>
	{
		public int OrderId {  get; set; } 
		public int ProductId { get; set; }
		public int Quantity { get; set; }
		public decimal UnitPrice { get; set; }
		public decimal DiscountPrice { get; set; }
		public int OrderStatus { get; set; }

	}
}
