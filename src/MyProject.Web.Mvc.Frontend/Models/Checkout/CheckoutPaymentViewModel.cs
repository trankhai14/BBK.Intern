using System;
using System.Collections.Generic;

namespace MyProject.Web.Models.Checkout
{
	public class CheckoutPaymentViewModel
	{
		public int OrderId { get; set; }
		public decimal OrderAmount { get; set; }
		public string PaymentReference { get; set; }

		// Bank info for VietQR
		public string BankCode { get; set; }
		public string BankAccount { get; set; }
		public string BankAccountName { get; set; }
		public string QrImageUrl { get; set; }
		public string PaymentContent { get; set; }

		// For displaying
		public string FullName { get; set; }
		public string PhoneNumber { get; set; }
		public string Address { get; set; }
		public string Note { get; set; }
		public DateTime? PaymentExpiredAt { get; set; }
		public IReadOnlyList<CheckoutPaymentItemViewModel> Items { get; set; } = Array.Empty<CheckoutPaymentItemViewModel>();

		public class CheckoutPaymentItemViewModel
		{
			public int ProductId { get; set; }
			public string ProductName { get; set; }
			public string Image { get; set; }
			public int Quantity { get; set; }
			public decimal UnitPrice { get; set; }
			public decimal LineTotal => UnitPrice * Quantity;
		}
	}
}

