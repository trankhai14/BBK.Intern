using System;
using System.ComponentModel.DataAnnotations;

namespace MyProject.FlashSales.Dto
{
	public class UpdateFlashSaleDto
	{
		[Required]
		public int Id { get; set; }

		[Required(ErrorMessage = "Tên chương trình FlashSale là bắt buộc")]
		[StringLength(256, ErrorMessage = "Tên không được vượt quá 256 ký tự")]
		public string Name { get; set; }

		[StringLength(2000, ErrorMessage = "Mô tả không được vượt quá 2000 ký tự")]
		public string Description { get; set; }

		[Required(ErrorMessage = "Thời gian bắt đầu là bắt buộc")]
		public DateTime StartTime { get; set; }

		[Required(ErrorMessage = "Thời gian kết thúc là bắt buộc")]
		public DateTime EndTime { get; set; }

		public bool IsActive { get; set; }

		public bool IsHidden { get; set; }
	}
}

