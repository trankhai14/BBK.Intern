using System;
using MyProject.Payments;

namespace MyProject.Payments.Dtos
{
	/// <summary>
	/// Kết quả kiểm tra giao dịch thanh toán
	/// </summary>
	public class PaymentVerificationResult
	{
		/// <summary>
		/// Đã xác nhận thành công hay chưa
		/// </summary>
		public bool IsVerified { get; set; }

		/// <summary>
		/// Thông tin giao dịch (nếu tìm thấy)
		/// </summary>
		public PaymentTransactionDto Transaction { get; set; }

		/// <summary>
		/// Thông báo
		/// </summary>
		public string Message { get; set; }

		/// <summary>
		/// Phương thức xác nhận
		/// </summary>
		public VerificationMethod Method { get; set; }
	}

	/// <summary>
	/// Enum định nghĩa phương thức xác nhận thanh toán
	/// </summary>
	public enum VerificationMethod
	{
		API = 1,      // Tích hợp API ngân hàng
		File = 2,     // Import file statement
		Manual = 3,   // Nhân viên xác nhận thủ công
		Webhook = 4   // Callback từ ngân hàng
	}
}

