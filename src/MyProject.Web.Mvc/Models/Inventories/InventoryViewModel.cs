using System.Collections.Generic;
using System.Linq;
using MyProject.Inventories.Dto;

namespace MyProject.Web.Models.Inventories
{
	public class InventoryViewModel
	{
		public List<InventoryListDto> InventoryLists { get; set; }

		public InventoryViewModel()
		{
			InventoryLists = new List<InventoryListDto>();
		}

		public InventoryViewModel(IReadOnlyList<InventoryListDto> items)
		{
			InventoryLists = items?.ToList() ?? new List<InventoryListDto>();
		}
	}

	public class EditInventoryViewModel
	{
		public InventoryDetailDto Inventory { get; set; }
	}
}
