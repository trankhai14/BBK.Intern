using System.Collections.Generic;
using MyProject.FlashSales.Dto;

namespace MyProject.Web.Models.FlashSales
{
	/// <summary>
	/// ViewModel cho trang danh sách FlashSale
	/// </summary>
	public class FlashSaleListViewModel
	{
		public List<FlashSaleDto> FlashSales { get; set; }

		public FlashSaleListViewModel()
		{
			FlashSales = new List<FlashSaleDto>();
		}
	}
}

