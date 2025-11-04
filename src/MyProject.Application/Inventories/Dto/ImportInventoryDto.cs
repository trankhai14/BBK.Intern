using System;

namespace MyProject.Inventories.Dto
{
	/// <summary>
	/// DTO cho việc nhập kho
	/// </summary>
	public class ImportInventoryDto
	{
		/// <summary>
		/// ID sản phẩm
		/// </summary>
		public int ProductId { get; set; }

		/// <summary>
		/// Số lượng nhập
		/// </summary>
		public int Quantity { get; set; }

		/// <summary>
		/// Lý do nhập kho
		/// </summary>
		public string Reason { get; set; }

		/// <summary>
		/// Ghi chú
		/// </summary>
		public string Notes { get; set; }
	}
}
