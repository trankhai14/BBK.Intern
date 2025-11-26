using System;
using System.Collections.Generic;
using MyProject.ExportSlips;

namespace MyProject.ExportSlips.Dto
{
	/// <summary>
	/// DTO hiển thị thông tin phiếu xuất kho
	/// </summary>
	public class ExportSlipDto
	{
		public int Id { get; set; }
		public string ExportCode { get; set; }
		public DateTime ExportDate { get; set; }
		public int? OrderId { get; set; }
		public string OrderCode { get; set; }
		public int? SupplierId { get; set; }
		public string SupplierName { get; set; }
		public ExportType Type { get; set; }
		public string TypeName { get; set; }
		public ExportStatus Status { get; set; }
		public string StatusName { get; set; }
		public string Reason { get; set; }
		public string Notes { get; set; }
		public long? CreatorUserId { get; set; }
		public string CreatorUserName { get; set; }
		public DateTime CreationTime { get; set; }
		public DateTime? LastModificationTime { get; set; }
		public List<ExportDetailDto> Details { get; set; }
	}

	/// <summary>
	/// DTO hiển thị chi tiết sản phẩm trong phiếu xuất
	/// </summary>
	public class ExportDetailDto
	{
		public int Id { get; set; }
		public int ExportSlipId { get; set; }
		public int ProductId { get; set; }
		public string ProductName { get; set; }
		public int Quantity { get; set; }
		public string Notes { get; set; }
	}
}

