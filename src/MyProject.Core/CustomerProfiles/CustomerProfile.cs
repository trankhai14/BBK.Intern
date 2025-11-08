using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using MyProject.Authorization.Users;

namespace MyProject.CustomerProfiles
{
	[Table("AppCustomerProfiles")]
	public class CustomerProfile : FullAuditedEntity<int>
	{
		public const int MaxPhoneLength = 20;
		public const int MaxAddressLength = 500;
		public const int MaxCityLength = 100;
		public const int MaxDistrictLength = 100;
		public const int MaxWardLength = 100;
	public const int MaxFullNameLength = 256;
	public const int MaxAvatarLength = 500;

	[Required]
	public long UserId { get; set; }

		[ForeignKey("UserId")]
		public User User { get; set; }

		[StringLength(MaxFullNameLength)]
		public string FullName { get; set; }

		[StringLength(MaxPhoneLength)]
		public string PhoneNumber { get; set; }

		[StringLength(MaxAddressLength)]
		public string Address { get; set; }

		[StringLength(MaxWardLength)]
		public string Ward { get; set; } // Phường/Xã

		[StringLength(MaxDistrictLength)]
		public string District { get; set; } // Quận/Huyện

	[StringLength(MaxCityLength)]
	public string City { get; set; } // Tỉnh/Thành phố

	[StringLength(MaxAvatarLength)]
	public string Avatar { get; set; } // Đường dẫn ảnh đại diện

	public bool IsDefault { get; set; } // Địa chỉ mặc định

		public CustomerProfile()
		{
			IsDefault = false;
		}
	}
}

