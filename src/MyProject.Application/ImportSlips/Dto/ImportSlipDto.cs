using System;
using System.Collections.Generic;
using MyProject.ImportSlips;

namespace MyProject.ImportSlips.Dto
{
	/// <summary>
	/// DTO hiển thị thông tin phiếu nhập kho
	/// </summary>
	public class ImportSlipDto
	{
		public int Id { get; set; }
		public string ImportCode { get; set; }
		public DateTime ImportDate { get; set; }
		public int? SupplierId { get; set; }
		public string SupplierName { get; set; }
		public ImportType Type { get; set; }
		public string TypeName { get; set; }
		public ImportStatus Status { get; set; }
		public string StatusName { get; set; }
		public decimal TotalAmount { get; set; }
		public string Notes { get; set; }
		public long? CreatorUserId { get; set; }
		public string CreatorUserName { get; set; }
		public DateTime CreationTime { get; set; }
		public DateTime? LastModificationTime { get; set; }
		public List<ImportDetailDto> Details { get; set; }
	}

	/// <summary>
	/// DTO hiển thị chi tiết sản phẩm trong phiếu nhập
	/// </summary>
	public class ImportDetailDto
	{
		public int Id { get; set; }
		public int ImportSlipId { get; set; }
		public int ProductId { get; set; }
		public string ProductName { get; set; }
		public int Quantity { get; set; }
		public decimal UnitPrice { get; set; }
		public decimal TotalAmount { get; set; }
		public string Notes { get; set; }
	}
}


