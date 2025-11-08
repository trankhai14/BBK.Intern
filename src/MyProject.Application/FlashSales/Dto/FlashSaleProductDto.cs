using System;

namespace MyProject.FlashSales.Dto
{
	public class FlashSaleProductDto
	{
		public int Id { get; set; }
		public int FlashSaleId { get; set; }
		public int ProductId { get; set; }
		public string ProductName { get; set; }
		public string ProductImage { get; set; }
		public decimal OriginalPrice { get; set; }
		public decimal FlashSalePrice { get; set; }
		public int FlashSaleQuantity { get; set; }
		public int SoldQuantity { get; set; }
		public int RemainingQuantity { get; set; }
		public int? MaxQuantityPerUser { get; set; }
		public int ReservedQuantity { get; set; }
		public bool IsReturnedToInventory { get; set; }
	}
}

