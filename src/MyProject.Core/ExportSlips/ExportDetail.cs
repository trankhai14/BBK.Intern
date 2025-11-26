using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using MyProject.Products;

namespace MyProject.ExportSlips
{
	/// <summary>
	/// Entity đại diện cho Chi tiết phiếu xuất kho
	/// </summary>
	[Table("AppExportDetails")]
	public class ExportDetail : Entity<int>
	{
		/// <summary>
		/// ID phiếu xuất
		/// </summary>
		[Required]
		public int ExportSlipId { get; set; }

		[ForeignKey("ExportSlipId")]
		public virtual ExportSlip ExportSlip { get; set; }

		/// <summary>
		/// ID sản phẩm
		/// </summary>
		[Required]
		public int ProductId { get; set; }

		[ForeignKey("ProductId")]
		public virtual Product Product { get; set; }

		/// <summary>
		/// Số lượng xuất
		/// </summary>
		[Required]
		public int Quantity { get; set; }

		/// <summary>
		/// Ghi chú
		/// </summary>
		[StringLength(500)]
		public string Notes { get; set; }

		public ExportDetail()
		{
			Quantity = 0;
		}
	}
}

