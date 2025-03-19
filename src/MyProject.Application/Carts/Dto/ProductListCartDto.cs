using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject.Carts.Dto
{
	public class ProductListCartDto
	{

		public List<ProductCartItemDto> Items { get; set; } = new List<ProductCartItemDto>();
		public decimal TotalPrice => Items.Sum(x => x.Price * x.Quantity);

		public int CartItem { get; set; }

	}
}
