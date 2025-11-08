using System.ComponentModel.DataAnnotations;

namespace MyProject.FlashSales.Dto
{
	public class AddProductToFlashSaleDto
	{
		[Required]
		public int FlashSaleId { get; set; }

		[Required]
		public int ProductId { get; set; }

		[Required(ErrorMessage = "Giá FlashSale là bắt buộc")]
		[Range(0.01, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
		public decimal FlashSalePrice { get; set; }

		[Required(ErrorMessage = "Số lượng FlashSale là bắt buộc")]
		[Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
		public int FlashSaleQuantity { get; set; }

		[Range(1, int.MaxValue, ErrorMessage = "Giới hạn mua mỗi người phải lớn hơn 0")]
		public int? MaxQuantityPerUser { get; set; }
	}
}

