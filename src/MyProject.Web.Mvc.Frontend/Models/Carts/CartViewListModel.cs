using System.Collections.Generic;

namespace MyProject.Web.Models.Carts
{
	public class CartViewListModel
	{
		public long UserId { get; set; }
		public string NameUser { get; set; }
		public List<CartViewModel> Carts { get; set; }

	}
}
