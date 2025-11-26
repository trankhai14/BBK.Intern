using System;
using MyProject.Payments;

namespace MyProject.Payments.Dtos
{
	/// <summary>
	/// DTO cho PaymentTransaction
	/// </summary>
	public class PaymentTransactionDto
	{
		public int Id { get; set; }
		public int OrderId { get; set; }
		public string PaymentReference { get; set; }
		public decimal Amount { get; set; }
		public string BankCode { get; set; }
		public string BankAccount { get; set; }
		public string TransactionId { get; set; }
		public DateTime TransactionTime { get; set; }
		public string Content { get; set; }
		public PaymentTransactionStatus Status { get; set; }
		public string StatusName { get; set; }
		public string VerifiedBy { get; set; }
		public DateTime? VerifiedAt { get; set; }
		public string Notes { get; set; }
		public DateTime CreationTime { get; set; }
	}
}

