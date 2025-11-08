using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;

namespace MyProject.Inventories.Dto
{
	public class GetAllInventoriesDto: PagedAndSortedResultRequestDto
	{
		public int? ProductId { get; set; }
		public string ProductName { get; set; } // Lọc theo tên sản phẩm
		public int? MinQuantity { get; set; } // Lọc theo số lượng tối thiểu
		public int? MaxQuantity { get; set; } // Lọc theo số lượng tối đa
		public DateTime CreateTime { get; set; }
		public string Keyword { get; set; }
		public int ReorderLevel { get; set; }
		//public string 
	}
}
