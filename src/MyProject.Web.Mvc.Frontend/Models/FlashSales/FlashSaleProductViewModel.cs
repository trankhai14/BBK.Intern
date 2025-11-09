using MyProject.FlashSales.Dto;

namespace MyProject.Web.Models.FlashSales
{
	/// <summary>
	/// ViewModel cho sản phẩm FlashSale (có thể mở rộng thêm properties nếu cần)
	/// </summary>
	public class FlashSaleProductViewModel : FlashSaleProductDto
	{
		/// <summary>
		/// Phần trăm giảm giá
		/// </summary>
		public decimal DiscountPercentage
		{
			get
			{
				if (OriginalPrice == 0) return 0;
				return ((OriginalPrice - FlashSalePrice) / OriginalPrice) * 100;
			}
		}

		/// <summary>
		/// Phần trăm đã bán
		/// </summary>
		public decimal SoldPercentage
		{
			get
			{
				if (FlashSaleQuantity == 0) return 0;
				return ((decimal)SoldQuantity / FlashSaleQuantity) * 100;
			}
		}
	}
}

