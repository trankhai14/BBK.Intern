using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using MyProject.Products;

namespace MyProject.ImportSlips
{
	/// <summary>
	/// Entity đại diện cho Chi tiết phiếu nhập kho
	/// </summary>
	[Table("AppImportDetails")]
	public class ImportDetail : Entity<int>
	{
		/// <summary>
		/// ID phiếu nhập
		/// </summary>
		[Required]
		public int ImportSlipId { get; set; }

		[ForeignKey("ImportSlipId")]
		public virtual ImportSlip ImportSlip { get; set; }

		/// <summary>
		/// ID sản phẩm
		/// </summary>
		[Required]
		public int ProductId { get; set; }

		[ForeignKey("ProductId")]
		public virtual Product Product { get; set; }

		/// <summary>
		/// Số lượng nhập
		/// </summary>
		[Required]
		public int Quantity { get; set; }

		/// <summary>
		/// Giá nhập đơn vị
		/// </summary>
		[Required]
		[Column(TypeName = "decimal(18,2)")]
		public decimal UnitPrice { get; set; }

		/// <summary>
		/// Thành tiền (Quantity * UnitPrice)
		/// </summary>
		[Required]
		[Column(TypeName = "decimal(18,2)")]
		public decimal TotalAmount { get; set; }

		/// <summary>
		/// Ghi chú
		/// </summary>
		[StringLength(500)]
		public string Notes { get; set; }

		public ImportDetail()
		{
			Quantity = 0;
			UnitPrice = 0;
			TotalAmount = 0;
		}
	}
}

