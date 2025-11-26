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

		[StringLength(50)]
		public string PhoneNumber { get; set; }

		[StringLength(500)]
		public string ShippingAddress { get; set; }

		[StringLength(100)]
		public string PaymentReference { get; set; }

		public bool IsPaid { get; set; }

		public DateTime? PaidTime { get; set; }

		public DateTime? PaymentExpiredAt { get; set; }

		[StringLength(500)]
		public string CustomerNote { get; set; }

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

	public enum OrderStatus : int
	{
		Pending = 0,      // Chờ xử lý - Đơn hàng mới tạo, chưa thanh toán
		Confirmed = 1,    // Đã xác nhận - Đã thanh toán, đang chờ xử lý
		Shipping = 2,     // Đang giao hàng - Đã xác nhận và đang vận chuyển
		Canceled = 3,     // Đã hủy - Đơn hàng bị hủy
		Success = 4       // Thành công - Đã giao hàng thành công
	}

}
