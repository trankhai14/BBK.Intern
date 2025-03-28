using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using MyProject.Authorization.Users;
using MyProject.Categories;
using MyProject.Products;

namespace MyProject.Carts
{
	public class Cart : Entity<int>
	{

		//public long UserId { get; set; }
		//[ForeignKey("UserId")]
		//public User User { get; set; }
		//public int ProductId { get; set; }
		//[ForeignKey("ProductId")]
		//public Product Product { get; set; }
		//public int Quantity { get; set; }
	}

	public class CartItem : FullAuditedEntity<int>
	{
		public int ProductId { get; set; }
		[ForeignKey("ProductId")]
		public Product Product { get; set; }
		public long UserId { get; set; }
		[ForeignKey("UserId")]
		public User User { get; set; }
		public int Quantity { get; set; }

	}
}
