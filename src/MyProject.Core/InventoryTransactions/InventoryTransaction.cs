using System;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using MyProject.Authorization.Users;
using MyProject.Products;


namespace MyProject.InventoryTransactions
{
	/// <summary>
	/// Entity quản lý lịch sử nhập xuất kho
	/// </summary>
	[Table("AppInventoryTransactions")]
	public class InventoryTransaction : FullAuditedEntity
	{
		// Loại giao dịch: Nhập kho (Import) hoặc Xuất kho (Export)
		public TransactionType Type { get; set; }

		// Liên kết với sản phẩm
		public int ProductId { get; set; }

		[ForeignKey("ProductId")]
		public Product Product { get; set; }

		// Số lượng nhập/xuất
		public int Quantity { get; set; }

		// Số lượng tồn kho trước khi giao dịch
		public int QuantityBefore { get; set; }

		// Số lượng tồn kho sau khi giao dịch
		public int QuantityAfter { get; set; }

		// Lý do nhập/xuất
		public string Reason { get; set; }

		// Ghi chú
		public string Notes { get; set; }

		// Người thực hiện (nếu cần)
		public long? UserId { get; set; }

		[ForeignKey("UserId")]
		public User User { get; set; }

		// Ngày thực hiện giao dịch
		public DateTime TransactionDate { get; set; }

		public InventoryTransaction()
		{
			TransactionDate = DateTime.Now;
		}
	}

	/// <summary>
	/// Enum định nghĩa loại giao dịch kho
	/// </summary>
	public enum TransactionType : byte
	{
		Import = 1,  // Nhập kho
		Export = 2   // Xuất kho
	}
}
