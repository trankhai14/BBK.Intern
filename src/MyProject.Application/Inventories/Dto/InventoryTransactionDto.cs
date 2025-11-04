using System;
using MyProject.Inventories;

namespace MyProject.Inventories.Dto
{
	/// <summary>
	/// DTO hiển thị lịch sử giao dịch kho
	/// </summary>
	public class InventoryTransactionDto
	{
		public int Id { get; set; }
		public TransactionType Type { get; set; }
		public string TypeName { get; set; }
		public int ProductId { get; set; }
		public string ProductName { get; set; }
		public int Quantity { get; set; }
		public int QuantityBefore { get; set; }
		public int QuantityAfter { get; set; }
		public string Reason { get; set; }
		public string Notes { get; set; }
		public long? UserId { get; set; }
		public string UserName { get; set; }
		public DateTime TransactionDate { get; set; }
		public DateTime CreationTime { get; set; }
	}

	/// <summary>
	/// DTO cho lọc và tìm kiếm giao dịch
	/// </summary>
	public class GetAllInventoryTransactionsDto : PagedAndSortedResultRequestDto
	{
		public int? ProductId { get; set; }
		public TransactionType? Type { get; set; }
		public DateTime? FromDate { get; set; }
		public DateTime? ToDate { get; set; }
		public string Keyword { get; set; }
	}
}
