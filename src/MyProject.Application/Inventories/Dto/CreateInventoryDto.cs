using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Domain.Entities.Auditing;

namespace MyProject.Inventories.Dto
{
	public class CreateInventoryDto : FullAuditedEntity
	{
		public int ProductId { get; set; }
		public string ProductName { get; set; }
		public int Quantity { get; set; }
		public DateTime CreateTime { get; set; }
		public DateTime LastUpdateTime { get; set; }
		public int ReservedQuantity { get; set; }
	}
}
