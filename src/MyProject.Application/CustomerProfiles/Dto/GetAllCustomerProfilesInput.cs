using Abp.Application.Services.Dto;

namespace MyProject.CustomerProfiles.Dto
{
	public class GetAllCustomerProfilesInput : PagedAndSortedResultRequestDto
	{
		public string Keyword { get; set; }
		public long? UserId { get; set; }
		public string FullName { get; set; }
		public string PhoneNumber { get; set; }
		public string City { get; set; }
	}
}

