using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using MyProject.Orders;

namespace MyProject.Payments
{
	/// <summary>
	/// Entity lưu trữ lịch sử giao dịch thanh toán từ ngân hàng
	/// </summary>
	[Table("AppPaymentTransactions")]
	public class PaymentTransaction : FullAuditedEntity<int>
	{
		/// <summary>
		/// Liên kết với đơn hàng
		/// </summary>
		[Required]
		public int OrderId { get; set; }

		[ForeignKey("OrderId")]
		public Order Order { get; set; }

		/// <summary>
		/// Mã tham chiếu thanh toán (PaymentReference từ Order)
		/// </summary>
		[StringLength(100)]
		public string PaymentReference { get; set; }

		/// <summary>
		/// Số tiền giao dịch
		/// </summary>
		[Required]
		public decimal Amount { get; set; }

		/// <summary>
		/// Mã ngân hàng (VD: VCB, TCB, etc.)
		/// </summary>
		[StringLength(50)]
		public string BankCode { get; set; }

		/// <summary>
		/// Số tài khoản ngân hàng
		/// </summary>
		[StringLength(50)]
		public string BankAccount { get; set; }

		/// <summary>
		/// Mã giao dịch từ ngân hàng
		/// </summary>
		[StringLength(100)]
		public string TransactionId { get; set; }

		/// <summary>
		/// Thời gian giao dịch từ ngân hàng
		/// </summary>
		[Required]
		public DateTime TransactionTime { get; set; }

		/// <summary>
		/// Nội dung chuyển khoản
		/// </summary>
		[StringLength(500)]
		public string Content { get; set; }

		/// <summary>
		/// Trạng thái giao dịch
		/// </summary>
		[Required]
		public PaymentTransactionStatus Status { get; set; }

		/// <summary>
		/// Phương thức xác nhận (System, Manual, Webhook, API)
		/// </summary>
		[StringLength(50)]
		public string VerifiedBy { get; set; }

		/// <summary>
		/// Thời gian xác nhận
		/// </summary>
		public DateTime? VerifiedAt { get; set; }

		/// <summary>
		/// Ghi chú
		/// </summary>
		[StringLength(1000)]
		public string Notes { get; set; }

		public PaymentTransaction()
		{
			Status = PaymentTransactionStatus.Pending;
			TransactionTime = DateTime.UtcNow;
		}
	}

	/// <summary>
	/// Enum định nghĩa trạng thái giao dịch thanh toán
	/// </summary>
	public enum PaymentTransactionStatus : int
	{
		Pending = 0,    // Chờ xác nhận
		Verified = 1,  // Đã xác nhận
		Failed = 2,   // Không khớp/Thất bại
		Expired = 3   // Hết hạn
	}
}

