using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using MyProject.Authorization.Users;
using MyProject.Suppliers;

namespace MyProject.ImportSlips
{
	/// <summary>
	/// Entity đại diện cho Phiếu nhập kho
	/// </summary>
	[Table("AppImportSlips")]
	public class ImportSlip : FullAuditedEntity<int>
	{
		public const int MaxImportCodeLength = 50;
		public const int MaxNotesLength = 1000;

		/// <summary>
		/// Mã phiếu nhập (duy nhất)
		/// </summary>
		[Required]
		[StringLength(MaxImportCodeLength)]
		public string ImportCode { get; set; }

		/// <summary>
		/// Ngày nhập kho
		/// </summary>
		[Required]
		public DateTime ImportDate { get; set; }

		/// <summary>
		/// ID nhà cung cấp (nếu nhập từ nhà cung cấp)
		/// </summary>
		public int? SupplierId { get; set; }

		[ForeignKey("SupplierId")]
		public Supplier Supplier { get; set; }

		/// <summary>
		/// Loại nhập kho
		/// </summary>
		[Required]
		public ImportType Type { get; set; }

		/// <summary>
		/// Trạng thái phiếu nhập
		/// </summary>
		[Required]
		public ImportStatus Status { get; set; }

		/// <summary>
		/// Tổng giá trị phiếu nhập
		/// </summary>
		public decimal TotalAmount { get; set; }

		/// <summary>
		/// Ghi chú
		/// </summary>
		[StringLength(MaxNotesLength)]
		public string Notes { get; set; }

		/// <summary>
		/// Chi tiết phiếu nhập
		/// </summary>
		public virtual ICollection<ImportDetail> Details { get; set; }

		/// <summary>
		/// Navigation property đến User (người tạo)
		/// </summary>
		[ForeignKey("CreatorUserId")]
		public virtual User CreatorUser { get; set; }

		public ImportSlip()
		{
			ImportDate = DateTime.Now;
			Status = ImportStatus.Draft;
			TotalAmount = 0;
			Details = new List<ImportDetail>();
		}
	}

	/// <summary>
	/// Enum định nghĩa loại nhập kho
	/// </summary>
	public enum ImportType : byte
	{
		Supplier = 1,      // Nhập từ nhà cung cấp
		Return = 2,       // Nhập hàng trả lại từ khách hàng
		Adjustment = 3,   // Nhập điều chỉnh (sau kiểm kê)
		Transfer = 4      // Nhập chuyển kho từ kho khác
	}

	/// <summary>
	/// Enum định nghĩa trạng thái phiếu nhập
	/// </summary>
	public enum ImportStatus : byte
	{
		Draft = 0,        // Nháp
		Completed = 1,    // Đã hoàn thành
		Cancelled = 2     // Đã hủy
	}
}

