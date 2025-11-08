using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using MyProject.Products;

namespace MyProject.FlashSales
{
	[Table("AppFlashSaleProducts")]
	public class FlashSaleProduct : FullAuditedEntity<int>
	{
		[Required]
		public int FlashSaleId { get; set; }

		[ForeignKey("FlashSaleId")]
		public virtual FlashSale FlashSale { get; set; }

		[Required]
		public int ProductId { get; set; }

		[ForeignKey("ProductId")]
		public virtual Product Product { get; set; }

		/// <summary>
		/// Giá FlashSale (giá đã giảm)
		/// </summary>
		[Required]
		public decimal FlashSalePrice { get; set; }

		/// <summary>
		/// Số lượng FlashSale (số lượng được phân bổ cho FlashSale)
		/// </summary>
		[Required]
		public int FlashSaleQuantity { get; set; }

		/// <summary>
		/// Số lượng đã bán trong FlashSale
		/// </summary>
		public int SoldQuantity { get; set; }

		/// <summary>
		/// Số lượng còn lại trong FlashSale
		/// </summary>
		[NotMapped]
		public int RemainingQuantity => FlashSaleQuantity - SoldQuantity;

		/// <summary>
		/// Giới hạn số lượng mua mỗi tài khoản (null = không giới hạn)
		/// </summary>
		public int? MaxQuantityPerUser { get; set; }

		/// <summary>
		/// Số lượng đã được khóa trong Inventory (ReservedQuantity)
		/// </summary>
		public int ReservedQuantity { get; set; }

		/// <summary>
		/// Đã hoàn trả về Inventory chưa (khi FlashSale kết thúc)
		/// </summary>
		public bool IsReturnedToInventory { get; set; }

		public FlashSaleProduct()
		{
			SoldQuantity = 0;
			ReservedQuantity = 0;
			IsReturnedToInventory = false;
		}
	}
}

