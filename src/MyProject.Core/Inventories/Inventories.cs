using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using MyProject.Products;

namespace MyProject.Inventories
{
	[Table("AppInventories")]
	public class Inventory : FullAuditedEntity
	{
		// Liên kết với sản phẩm
		[Required]
		public int ProductId { get; set; }

		[ForeignKey("ProductId")]
		public Product Product { get; set; }

		// Số lượng hiện tại trong kho
		public int Quantity { get; set; }

		// Số lượng đã được giữ lại (Reserved) cho các đơn hàng chưa hoàn tất
		public int ReservedQuantity { get; set; }

		// Ngưỡng cảnh báo khi sắp hết hàng (khi còn <= ReorderLevel thì cảnh báo)
		public int ReorderLevel { get; set; }

		// Số lượng tối thiểu trong kho (ngưỡng sắp hết hàng)
		public int MinQuantity { get; set; }

		// Đơn vị tính (Ví dụ: cái, hộp, thùng, kg, lít...)
		[StringLength(50)]
		public string Unit { get; set; }

		// Trạng thái kho hàng
		public InventoryStatus Status { get; set; }

		// Thuộc tính tính toán: số lượng sẵn sàng để bán
		[NotMapped]
		public int AvailableQuantity => Quantity - ReservedQuantity;

		// Thuộc tính tính toán: có sắp hết hàng không
		[NotMapped]
		public bool IsLowStock => Quantity <= MinQuantity && MinQuantity > 0;

		// Thuộc tính tính toán: có cần đặt hàng lại không
		[NotMapped]
		public bool NeedReorder => Quantity <= ReorderLevel && ReorderLevel > 0;

		// Ngày cập nhật gần nhất
		public DateTime LastUpdated { get; set; }

		// Ghi chú
		[StringLength(500)]
		public string Notes { get; set; }

		public Inventory()
		{
			Quantity = 0;
			ReservedQuantity = 0;
			ReorderLevel = 0;
			MinQuantity = 0;
			Unit = "cái";
			Status = InventoryStatus.Active;
			LastUpdated = DateTime.Now;
		}
	}

	/// <summary>
	/// Enum định nghĩa trạng thái kho hàng
	/// </summary>
	public enum InventoryStatus : byte
	{
		Active = 1,      // Đang hoạt động
		Inactive = 2,    // Tạm ngưng
		Discontinued = 3 // Ngừng kinh doanh
	}
}
