using Abp.Application.Services.Dto;

namespace MyProject.Suppliers.Dto
{
	public class GetAllSuppliersInput : PagedAndSortedResultRequestDto
	{
		public string Name { get; set; }
		public string Code { get; set; }
		public string Phone { get; set; }
		public string Email { get; set; }
		public bool? IsActive { get; set; }
	}
}

