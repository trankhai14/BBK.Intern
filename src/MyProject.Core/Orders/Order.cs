using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Domain.Entities.Auditing;
using MyProject.Authorization.Users;
using MyProject.Products;

namespace MyProject.Orders
{
	[Table("AppOrders")]
	public class Order : FullAuditedEntity<int>
	{
		[Required]
		public long UserId { get; set; }
		[Required]
		public string NameUser { get; set; }
		public decimal totalAmount { get; set; } // tổng số tiền
		public decimal DiscountAmount { get; set; }// tổng tiền giảm giá 
		[Required]
		public int PaymentMethod { get; set; }
		[Required]
		[StringLength(50)]
		public int OrderStatus { get; set; }// trang thái đơn hàng mặc định

		//quan hệ với bảng user
		[ForeignKey("UserId")]
		public User User { get; set; }


	}

	[Table("AppOrderDetails")]
	public class OrderDetail : FullAuditedEntity<int>
	{
		[Required]
		public int OrderId { get; set; }
		[Required]
		public int ProductId { get; set; }
		[Required]
		public int Quantity { get; set; }
		[Required]
		public decimal UnitPrice { get; set; } // giá của sản phẩm tại thời điểm đặt hàng
		public decimal DiscountPrice { get; set; } = 0; // giảm giá cho sản phẩm nếu có 

		public decimal TotalPrice => (UnitPrice * Quantity) - DiscountPrice; // tổng giá tiền cho sản phẩm 

		[ForeignKey("OrderId")]
		public Order Order { get; set; }

		[ForeignKey("ProductId")]
		public Product Product { get; set; }
	}

	//public enum OrderStatus : byte
	//{
	//	All = 0,          // Tất cả đơn hàng
	//	Pending = 1,      // Chờ xử lý
	//	Confirmed = 2,    // Đã xác nhận
	//	Shipping = 3,     // Đang giao hàng
	//	Canceled = 4,     // Đã hủy
	//	Success = 5       // Thành công
	//}

}
