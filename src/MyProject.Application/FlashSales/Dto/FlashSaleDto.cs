using System;
using System.Collections.Generic;
using MyProject.FlashSales;

namespace MyProject.FlashSales.Dto
{
	public class FlashSaleDto
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public FlashSaleStatus Status { get; set; }
		public string StatusText { get; set; }
		public bool IsActive { get; set; }
		public bool IsHidden { get; set; }
		public DateTime CreationTime { get; set; }
		public DateTime? LastModificationTime { get; set; }
		public int TotalProducts { get; set; }
		public int TotalSold { get; set; }
		public List<FlashSaleProductDto> Products { get; set; }

		public FlashSaleDto()
		{
			Products = new List<FlashSaleProductDto>();
		}
	}
}

