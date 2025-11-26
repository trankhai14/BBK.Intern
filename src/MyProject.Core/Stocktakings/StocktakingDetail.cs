using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using MyProject.Products;

namespace MyProject.Stocktakings
{
	/// <summary>
	/// Entity đại diện cho Chi tiết kiểm kê kho
	/// </summary>
	[Table("AppStocktakingDetails")]
	public class StocktakingDetail : Entity<int>
	{
		/// <summary>
		/// ID phiếu kiểm kê
		/// </summary>
		[Required]
		public int StocktakingId { get; set; }

		[ForeignKey("StocktakingId")]
		public virtual Stocktaking Stocktaking { get; set; }

		/// <summary>
		/// ID sản phẩm
		/// </summary>
		[Required]
		public int ProductId { get; set; }

		[ForeignKey("ProductId")]
		public virtual Product Product { get; set; }

		/// <summary>
		/// Số lượng theo hệ thống (từ Inventory)
		/// </summary>
		[Required]
		public int SystemQuantity { get; set; }

		/// <summary>
		/// Số lượng thực tế (đếm được)
		/// </summary>
		[Required]
		public int ActualQuantity { get; set; }

		/// <summary>
		/// Chênh lệch (ActualQuantity - SystemQuantity)
		/// </summary>
		[Required]
		public int Difference { get; set; }

		/// <summary>
		/// Lý do chênh lệch
		/// </summary>
		[StringLength(500)]
		public string Reason { get; set; }

		/// <summary>
		/// Đã điều chỉnh chưa
		/// </summary>
		public bool IsAdjusted { get; set; }

		/// <summary>
		/// Ngày điều chỉnh
		/// </summary>
		public DateTime? AdjustedDate { get; set; }

		public StocktakingDetail()
		{
			SystemQuantity = 0;
			ActualQuantity = 0;
			Difference = 0;
			IsAdjusted = false;
		}
	}
}

