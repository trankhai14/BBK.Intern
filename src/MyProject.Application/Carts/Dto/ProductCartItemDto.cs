using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject.Carts.Dto
{
	public class ProductCartItemDto
	{
		public int Id { get; set; }
		public int Quantity { get; set; }
		public string Name { get; set; }
		public string Image { get; set; }
		public decimal Price { get; set; }
		public long UserId { get; set; }

		public int ProductId { get; set; }
	}

	public class CartsDto
	{
		public int Id { get; set; }
		public int Quantity { get; set; }
		public long UserId { get; set; }
		public int ProductId { get; set; }
	}
}
