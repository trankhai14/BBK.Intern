using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace MyProject.Suppliers
{
	/// <summary>
	/// Entity đại diện cho Nhà cung cấp trong hệ thống
	/// Kế thừa từ FullAuditedEntity để tự động theo dõi thông tin tạo/sửa/xóa
	/// </summary>
	[Table("AppSuppliers")]
	public class Supplier : FullAuditedEntity<int>
	{
		// Các hằng số định nghĩa độ dài tối đa cho các trường
		public const int MaxNameLength = 256;
		public const int MaxCodeLength = 50;
		public const int MaxPhoneLength = 20;
		public const int MaxEmailLength = 256;
		public const int MaxAddressLength = 500;
		public const int MaxContactPersonLength = 256;
		public const int MaxNotesLength = 1000;

		/// <summary>
		/// Tên nhà cung cấp (bắt buộc)
		/// </summary>
		[Required]
		[StringLength(MaxNameLength)]
		public string Name { get; set; }

		/// <summary>
		/// Mã nhà cung cấp (tùy chọn, dùng để tra cứu nhanh)
		/// </summary>
		[StringLength(MaxCodeLength)]
		public string Code { get; set; }

		/// <summary>
		/// Số điện thoại liên hệ
		/// </summary>
		[StringLength(MaxPhoneLength)]
		public string Phone { get; set; }

		/// <summary>
		/// Email liên hệ
		/// </summary>
		[StringLength(MaxEmailLength)]
		public string Email { get; set; }

		/// <summary>
		/// Địa chỉ nhà cung cấp
		/// </summary>
		[StringLength(MaxAddressLength)]
		public string Address { get; set; }

		/// <summary>
		/// Tên người liên hệ trực tiếp tại nhà cung cấp
		/// </summary>
		[StringLength(MaxContactPersonLength)]
		public string ContactPerson { get; set; }

		/// <summary>
		/// Ghi chú bổ sung về nhà cung cấp
		/// </summary>
		[StringLength(MaxNotesLength)]
		public string Notes { get; set; }

		/// <summary>
		/// Trạng thái hoạt động của nhà cung cấp
		/// true: Đang hoạt động, false: Tạm ngưng
		/// </summary>
		public bool IsActive { get; set; }

		/// <summary>
		/// Constructor mặc định
		/// Khởi tạo nhà cung cấp với trạng thái hoạt động mặc định là true
		/// </summary>
		public Supplier()
		{
			IsActive = true;
		}
	}
}

