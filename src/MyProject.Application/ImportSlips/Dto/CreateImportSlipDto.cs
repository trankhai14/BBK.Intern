using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MyProject.ImportSlips;

namespace MyProject.ImportSlips.Dto
{
	/// <summary>
	/// DTO cho việc tạo phiếu nhập kho mới
	/// </summary>
	public class CreateImportSlipDto
	{
		[Required(ErrorMessage = "Ngày nhập kho là bắt buộc")]
		public DateTime ImportDate { get; set; }

		/// <summary>
		/// ID nhà cung cấp (tùy chọn)
		/// </summary>
		public int? SupplierId { get; set; }

		[Required(ErrorMessage = "Loại nhập kho là bắt buộc")]
		public ImportType Type { get; set; }

		[StringLength(1000)]
		public string Notes { get; set; }

		[Required(ErrorMessage = "Chi tiết sản phẩm là bắt buộc")]
		[MinLength(1, ErrorMessage = "Phiếu nhập phải có ít nhất 1 sản phẩm")]
		public List<CreateImportDetailDto> Details { get; set; }
	}

	/// <summary>
	/// DTO cho chi tiết sản phẩm trong phiếu nhập
	/// </summary>
	public class CreateImportDetailDto
	{
		[Required(ErrorMessage = "Sản phẩm là bắt buộc")]
		public int ProductId { get; set; }

		[Required(ErrorMessage = "Số lượng là bắt buộc")]
		[Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
		public int Quantity { get; set; }

		[Required(ErrorMessage = "Giá nhập là bắt buộc")]
		[Range(0.01, double.MaxValue, ErrorMessage = "Giá nhập phải lớn hơn 0")]
		public decimal UnitPrice { get; set; }

		[StringLength(500)]
		public string Notes { get; set; }
	}
}


