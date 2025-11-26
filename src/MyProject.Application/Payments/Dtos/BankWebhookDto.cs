using System;
using System.ComponentModel.DataAnnotations;

namespace MyProject.Payments.Dtos
{
	/// <summary>
	/// DTO nhận webhook từ ngân hàng
	/// </summary>
	public class BankWebhookDto
	{
		[Required]
		public string PaymentReference { get; set; }

		[Required]
		public decimal Amount { get; set; }

		public string BankCode { get; set; }

		public string BankAccount { get; set; }

		[Required]
		public string TransactionId { get; set; }

		[Required]
		public DateTime TransactionTime { get; set; }

		public string Content { get; set; }

		/// <summary>
		/// Chữ ký để validate (nếu ngân hàng hỗ trợ)
		/// </summary>
		public string Signature { get; set; }
	}
}

