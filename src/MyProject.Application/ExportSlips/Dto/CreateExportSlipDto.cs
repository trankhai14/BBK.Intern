using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MyProject.ExportSlips;

namespace MyProject.ExportSlips.Dto
{
	/// <summary>
	/// DTO cho việc tạo phiếu xuất kho mới
	/// </summary>
	public class CreateExportSlipDto
	{
		[Required(ErrorMessage = "Ngày xuất kho là bắt buộc")]
		public DateTime ExportDate { get; set; }

		/// <summary>
		/// ID đơn hàng (nếu xuất cho đơn hàng)
		/// </summary>
		public int? OrderId { get; set; }

		/// <summary>
		/// ID nhà cung cấp (nếu trả nhà cung cấp)
		/// </summary>
		public int? SupplierId { get; set; }

		[Required(ErrorMessage = "Loại xuất kho là bắt buộc")]
		public ExportType Type { get; set; }

		[StringLength(500)]
		public string Reason { get; set; }

		[StringLength(1000)]
		public string Notes { get; set; }

		[Required(ErrorMessage = "Chi tiết sản phẩm là bắt buộc")]
		[MinLength(1, ErrorMessage = "Phiếu xuất phải có ít nhất 1 sản phẩm")]
		public List<CreateExportDetailDto> Details { get; set; }
	}

	/// <summary>
	/// DTO cho chi tiết sản phẩm trong phiếu xuất
	/// </summary>
	public class CreateExportDetailDto
	{
		[Required(ErrorMessage = "Sản phẩm là bắt buộc")]
		public int ProductId { get; set; }

		[Required(ErrorMessage = "Số lượng là bắt buộc")]
		[Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
		public int Quantity { get; set; }

		[StringLength(500)]
		public string Notes { get; set; }
	}
}

