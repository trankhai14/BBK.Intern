using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MyProject.ExportSlips;

namespace MyProject.ExportSlips.Dto
{
	/// <summary>
	/// DTO cho việc cập nhật phiếu xuất kho
	/// </summary>
	public class UpdateExportSlipDto
	{
		[Required]
		public int Id { get; set; }

		[Required(ErrorMessage = "Ngày xuất kho là bắt buộc")]
		public DateTime ExportDate { get; set; }

		public int? OrderId { get; set; }

		public int? SupplierId { get; set; }

		[Required(ErrorMessage = "Loại xuất kho là bắt buộc")]
		public ExportType Type { get; set; }

		[StringLength(500)]
		public string Reason { get; set; }

		[StringLength(1000)]
		public string Notes { get; set; }

		[Required(ErrorMessage = "Chi tiết sản phẩm là bắt buộc")]
		[MinLength(1, ErrorMessage = "Phiếu xuất phải có ít nhất 1 sản phẩm")]
		public List<UpdateExportDetailDto> Details { get; set; }
	}

	/// <summary>
	/// DTO cho chi tiết sản phẩm trong phiếu xuất (cập nhật)
	/// </summary>
	public class UpdateExportDetailDto
	{
		public int? Id { get; set; } // Null nếu là mới

		[Required(ErrorMessage = "Sản phẩm là bắt buộc")]
		public int ProductId { get; set; }

		[Required(ErrorMessage = "Số lượng là bắt buộc")]
		[Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
		public int Quantity { get; set; }

		[StringLength(500)]
		public string Notes { get; set; }
	}
}

