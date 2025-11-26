using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using MyProject.Authorization.Users;

namespace MyProject.Stocktakings
{
	/// <summary>
	/// Entity đại diện cho Phiếu kiểm kê kho
	/// </summary>
	[Table("AppStocktakings")]
	public class Stocktaking : FullAuditedEntity<int>
	{
		public const int MaxStocktakingCodeLength = 50;
		public const int MaxNotesLength = 1000;

		/// <summary>
		/// Mã phiếu kiểm kê (duy nhất)
		/// </summary>
		[Required]
		[StringLength(MaxStocktakingCodeLength)]
		public string StocktakingCode { get; set; }

		/// <summary>
		/// Ngày dự kiến thực hiện
		/// </summary>
		[Required]
		public DateTime PlannedDate { get; set; }

		/// <summary>
		/// Ngày hoàn thành (null nếu chưa hoàn thành)
		/// </summary>
		public DateTime? CompletedDate { get; set; }

		/// <summary>
		/// Trạng thái kiểm kê
		/// </summary>
		[Required]
		public StocktakingStatus Status { get; set; }

		/// <summary>
		/// ID kho (nếu có nhiều kho, null nếu chỉ có 1 kho)
		/// </summary>
		public int? WarehouseId { get; set; }

		/// <summary>
		/// ID người được phân công thực hiện
		/// </summary>
		public long? AssignedTo { get; set; }

		[ForeignKey("AssignedTo")]
		public virtual User AssignedUser { get; set; }

		/// <summary>
		/// Ghi chú
		/// </summary>
		[StringLength(MaxNotesLength)]
		public string Notes { get; set; }

		/// <summary>
		/// Chi tiết kiểm kê
		/// </summary>
		public virtual ICollection<StocktakingDetail> Details { get; set; }

		public Stocktaking()
		{
			PlannedDate = DateTime.Now;
			Status = StocktakingStatus.Planned;
			Details = new List<StocktakingDetail>();
		}
	}

	/// <summary>
	/// Enum định nghĩa trạng thái kiểm kê
	/// </summary>
	public enum StocktakingStatus : byte
	{
		Planned = 0,      // Đã lập kế hoạch
		InProgress = 1,   // Đang thực hiện
		Completed = 2,    // Đã hoàn thành
		Cancelled = 3     // Đã hủy
	}
}

