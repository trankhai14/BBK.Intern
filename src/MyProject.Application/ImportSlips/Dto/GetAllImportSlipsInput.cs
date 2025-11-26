using System;
using Abp.Application.Services.Dto;
using MyProject.ImportSlips;

namespace MyProject.ImportSlips.Dto
{
	/// <summary>
	/// DTO cho việc lọc và phân trang danh sách phiếu nhập kho
	/// </summary>
	public class GetAllImportSlipsInput : PagedAndSortedResultRequestDto
	{
		/// <summary>
		/// Tìm kiếm theo mã phiếu nhập
		/// </summary>
		public string ImportCode { get; set; }

		/// <summary>
		/// Lọc theo nhà cung cấp
		/// </summary>
		public int? SupplierId { get; set; }

		/// <summary>
		/// Lọc theo loại nhập kho
		/// </summary>
		public ImportType? Type { get; set; }

		/// <summary>
		/// Lọc theo trạng thái
		/// </summary>
		public ImportStatus? Status { get; set; }

		/// <summary>
		/// Lọc từ ngày
		/// </summary>
		public DateTime? FromDate { get; set; }

		/// <summary>
		/// Lọc đến ngày
		/// </summary>
		public DateTime? ToDate { get; set; }

		/// <summary>
		/// Tìm kiếm theo từ khóa (mã phiếu, ghi chú, tên nhà cung cấp)
		/// </summary>
		public string Keyword { get; set; }
	}
}


