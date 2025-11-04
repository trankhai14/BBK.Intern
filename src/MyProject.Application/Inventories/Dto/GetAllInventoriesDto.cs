using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;

namespace MyProject.Inventories.Dto
{
	/// <summary>
	/// DTO cho tìm kiếm và lọc danh sách kho hàng
	/// </summary>
	public class GetAllInventoriesDto: PagedAndSortedResultRequestDto
	{
		public int? ProductId { get; set; }
		public string ProductName { get; set; }
		public int? MinQuantity { get; set; }
		public int? MaxQuantity { get; set; }
		public bool? IsLowStock { get; set; }
		public bool? NeedReorder { get; set; }
		public InventoryStatus? Status { get; set; }
		public string Keyword { get; set; }
	}
}
