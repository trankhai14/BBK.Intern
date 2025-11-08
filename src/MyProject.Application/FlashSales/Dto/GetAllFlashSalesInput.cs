using Abp.Application.Services.Dto;

namespace MyProject.FlashSales.Dto
{
	public class GetAllFlashSalesInput : PagedAndSortedResultRequestDto
	{
		public string Keyword { get; set; }
		public int? Status { get; set; }
		public bool? IsActive { get; set; }
		public bool? IsHidden { get; set; }
	}
}

