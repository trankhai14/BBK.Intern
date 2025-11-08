using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MyProject.CustomerProfiles.Dto
{
	public class CreateCustomerProfileDto
	{
		[Required(ErrorMessage = "Họ và tên là bắt buộc")]
		[StringLength(256, ErrorMessage = "Họ và tên không được vượt quá 256 ký tự")]
		public string FullName { get; set; }

		[Required(ErrorMessage = "Số điện thoại là bắt buộc")]
		[StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
		[Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
		public string PhoneNumber { get; set; }

		[Required(ErrorMessage = "Địa chỉ là bắt buộc")]
		[StringLength(500, ErrorMessage = "Địa chỉ không được vượt quá 500 ký tự")]
		public string Address { get; set; }

		[StringLength(100, ErrorMessage = "Phường/Xã không được vượt quá 100 ký tự")]
		public string Ward { get; set; }

		[StringLength(100, ErrorMessage = "Quận/Huyện không được vượt quá 100 ký tự")]
		public string District { get; set; }

	[Required(ErrorMessage = "Tỉnh/Thành phố là bắt buộc")]
	[StringLength(100, ErrorMessage = "Tỉnh/Thành phố không được vượt quá 100 ký tự")]
	public string City { get; set; }

	public IFormFile AvatarFile { get; set; }
	public string Avatar { get; set; }

	public bool IsDefault { get; set; }
}
}

