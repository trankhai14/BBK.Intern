using System;

namespace MyProject.Inventories.Dto
{
	/// <summary>
	/// DTO cho việc xuất kho
	/// </summary>
	public class ExportInventoryDto
	{
		/// <summary>
		/// ID sản phẩm
		/// </summary>
		public int ProductId { get; set; }

		/// <summary>
		/// Số lượng xuất
		/// </summary>
		public int Quantity { get; set; }

		/// <summary>
		/// Lý do xuất kho
		/// </summary>
		public string Reason { get; set; }

		/// <summary>
		/// Ghi chú
		/// </summary>
		public string Notes { get; set; }
	}
}
