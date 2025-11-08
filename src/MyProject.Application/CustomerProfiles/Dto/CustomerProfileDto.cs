using System;

namespace MyProject.CustomerProfiles.Dto
{
	public class CustomerProfileDto
	{
		public int Id { get; set; }
		public long UserId { get; set; }
		public string FullName { get; set; }
		public string PhoneNumber { get; set; }
		public string Address { get; set; }
		public string Ward { get; set; }
		public string District { get; set; }
	public string City { get; set; }
	public string Avatar { get; set; }
	public bool IsDefault { get; set; }
	public DateTime CreationTime { get; set; }
		public DateTime? LastModificationTime { get; set; }
	}
}

