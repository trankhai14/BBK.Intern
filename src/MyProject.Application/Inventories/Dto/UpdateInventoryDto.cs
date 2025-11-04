using System.ComponentModel.DataAnnotations;

namespace MyProject.Inventories.Dto
{
	/// <summary>
	/// DTO cho việc cập nhật kho hàng
	/// </summary>
	public class UpdateInventoryDto
	{
		[Required]
		public int Id { get; set; }

		[Range(0, int.MaxValue, ErrorMessage = "Số lượng không được âm")]
		public int? Quantity { get; set; }

		[Range(0, int.MaxValue, ErrorMessage = "Số lượng giữ không được âm")]
		public int? ReservedQuantity { get; set; }

		[Range(0, int.MaxValue, ErrorMessage = "Ngưỡng đặt lại không được âm")]
		public int? ReorderLevel { get; set; }

		[Range(0, int.MaxValue, ErrorMessage = "Số lượng tối thiểu không được âm")]
		public int? MinQuantity { get; set; }

		[StringLength(50)]
		public string Unit { get; set; }

		public InventoryStatus? Status { get; set; }

		[StringLength(500)]
		public string Notes { get; set; }
	}
}
