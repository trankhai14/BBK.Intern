using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using MyProject.Authorization.Users;

namespace MyProject.FlashSales
{
	[Table("AppFlashSales")]
	public class FlashSale : FullAuditedEntity<int>
	{
		public const int MaxNameLength = 256;
		public const int MaxDescriptionLength = 2000;

		[Required]
		[StringLength(MaxNameLength)]
		public string Name { get; set; }

		[StringLength(MaxDescriptionLength)]
		public string Description { get; set; }

		[Required]
		public DateTime StartTime { get; set; }

		[Required]
		public DateTime EndTime { get; set; }

		public FlashSaleStatus Status { get; set; }

		public bool IsActive { get; set; }

		public bool IsHidden { get; set; }

		// Navigation property
		public virtual ICollection<FlashSaleProduct> FlashSaleProducts { get; set; }

		public FlashSale()
		{
			IsActive = true;
			IsHidden = false;
			Status = FlashSaleStatus.NotStarted;
			FlashSaleProducts = new List<FlashSaleProduct>();
		}

		// Tính toán trạng thái dựa trên thời gian
		[NotMapped]
		public FlashSaleStatus CalculatedStatus
		{
			get
			{
				var now = DateTime.Now;
				if (now < StartTime)
					return FlashSaleStatus.NotStarted;
				if (now >= StartTime && now <= EndTime)
					return FlashSaleStatus.Ongoing;
				if (now > EndTime)
					return FlashSaleStatus.Ended;
				return FlashSaleStatus.Cancelled;
			}
		}
	}

	/// <summary>
	/// Enum định nghĩa trạng thái FlashSale
	/// </summary>
	public enum FlashSaleStatus : byte
	{
		NotStarted = 0,  // Chưa bắt đầu
		Ongoing = 1,     // Đang diễn ra
		Ended = 2,       // Đã kết thúc
		Cancelled = 3    // Đã hủy
	}
}

