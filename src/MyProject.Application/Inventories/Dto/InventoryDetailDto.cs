using System;
using MyProject.Inventories;

namespace MyProject.Inventories.Dto
{
	/// <summary>
	/// DTO hiển thị chi tiết kho hàng
	/// </summary>
	public class InventoryDetailDto
	{
		public int Id { get; set; }
		public int ProductId { get; set; }
		public string ProductName { get; set; }
		public string ProductDescription { get; set; }
		public decimal ProductPrice { get; set; }
		public int Quantity { get; set; }
		public int ReservedQuantity { get; set; }
		public int AvailableQuantity { get; set; }
		public int ReorderLevel { get; set; }
		public int MinQuantity { get; set; }
		public string Unit { get; set; }
		public InventoryStatus Status { get; set; }
		public string StatusName { get; set; }
		public bool IsLowStock { get; set; }
		public bool NeedReorder { get; set; }
		public DateTime CreateTime { get; set; }
		public DateTime LastUpdateTime { get; set; }
		public string Notes { get; set; }
		public string CreatorUserName { get; set; }
		public string LastModifierUserName { get; set; }
	}
}
