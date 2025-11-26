using System;
using Abp.Application.Services.Dto;
using MyProject.ExportSlips;

namespace MyProject.ExportSlips.Dto
{
	/// <summary>
	/// DTO cho việc lấy danh sách phiếu xuất kho có phân trang và lọc
	/// </summary>
	public class GetAllExportSlipsInput : PagedAndSortedResultRequestDto
	{
		public string ExportCode { get; set; }
		public int? SupplierId { get; set; }
		public int? OrderId { get; set; }
		public ExportType? Type { get; set; }
		public ExportStatus? Status { get; set; }
		public DateTime? FromDate { get; set; }
		public DateTime? ToDate { get; set; }
		public string Keyword { get; set; }
	}
}

