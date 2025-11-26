using System;
using System.Collections.Generic;

namespace MyProject.Web.Models.Checkout
{
	public class CheckoutConfirmViewModel
	{
		public string FullName { get; set; }
		public string PhoneNumber { get; set; }
		public string Address { get; set; }
		public string PaymentMethod { get; set; } // QR (default), COD (future)
		public string Note { get; set; }
		public int? SelectedProfileId { get; set; }
		public IReadOnlyList<CustomerProfileOption> Profiles { get; set; } = Array.Empty<CustomerProfileOption>();

		// Tổng tiền hiển thị (client submit lên), backend sẽ tính lại khi tích hợp
		public decimal OrderAmount { get; set; }

		public class CustomerProfileOption
		{
			public int Id { get; set; }
			public string DisplayName { get; set; }
			public string FullName { get; set; }
			public string PhoneNumber { get; set; }
			public string Address { get; set; }
		}
	}
}

