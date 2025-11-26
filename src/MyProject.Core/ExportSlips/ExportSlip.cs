using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using MyProject.Authorization.Users;
using MyProject.Orders;
using MyProject.Suppliers;

namespace MyProject.ExportSlips
{
	/// <summary>
	/// Entity đại diện cho Phiếu xuất kho
	/// </summary>
	[Table("AppExportSlips")]
	public class ExportSlip : FullAuditedEntity<int>
	{
		public const int MaxExportCodeLength = 50;
		public const int MaxReasonLength = 500;
		public const int MaxNotesLength = 1000;

		/// <summary>
		/// Mã phiếu xuất (duy nhất)
		/// </summary>
		[Required]
		[StringLength(MaxExportCodeLength)]
		public string ExportCode { get; set; }

		/// <summary>
		/// Ngày xuất kho
		/// </summary>
		[Required]
		public DateTime ExportDate { get; set; }

		/// <summary>
		/// Loại xuất kho
		/// </summary>
		[Required]
		public ExportType Type { get; set; }

		/// <summary>
		/// ID đơn hàng (nếu xuất cho đơn hàng)
		/// </summary>
		public int? OrderId { get; set; }

		[ForeignKey("OrderId")]
		public virtual Order Order { get; set; }

		/// <summary>
		/// ID nhà cung cấp (nếu trả nhà cung cấp)
		/// </summary>
		public int? SupplierId { get; set; }

		[ForeignKey("SupplierId")]
		public virtual Supplier Supplier { get; set; }

		/// <summary>
		/// Trạng thái phiếu xuất
		/// </summary>
		[Required]
		public ExportStatus Status { get; set; }

		/// <summary>
		/// Lý do xuất kho
		/// </summary>
		[StringLength(MaxReasonLength)]
		public string Reason { get; set; }

		/// <summary>
		/// Ghi chú
		/// </summary>
		[StringLength(MaxNotesLength)]
		public string Notes { get; set; }

		/// <summary>
		/// Chi tiết phiếu xuất
		/// </summary>
		public virtual ICollection<ExportDetail> Details { get; set; }

		/// <summary>
		/// Navigation property đến User (người tạo)
		/// </summary>
		[ForeignKey("CreatorUserId")]
		public virtual User CreatorUser { get; set; }

		public ExportSlip()
		{
			ExportDate = DateTime.Now;
			Status = ExportStatus.Draft;
			Details = new List<ExportDetail>();
		}
	}

	/// <summary>
	/// Enum định nghĩa loại xuất kho
	/// </summary>
	public enum ExportType : byte
	{
		Order = 1,              // Xuất cho đơn hàng
		SupplierReturn = 2,     // Xuất trả nhà cung cấp
		Damage = 3,             // Xuất hỏng hóc
		Adjustment = 4,         // Xuất điều chỉnh (sau kiểm kê)
		Transfer = 5            // Xuất chuyển kho
	}

	/// <summary>
	/// Enum định nghĩa trạng thái phiếu xuất
	/// </summary>
	public enum ExportStatus : byte
	{
		Draft = 0,        // Nháp
		Completed = 1,    // Đã hoàn thành
		Cancelled = 2     // Đã hủy
	}
}

