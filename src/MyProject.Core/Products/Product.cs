using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Timing;
using MyProject.Categories;
using MyProject.Suppliers;

namespace MyProject.Products
{
	[Table("AppProducts")]
	public class Product : Entity, IHasCreationTime
	{
		public const int MaxNameLength = 256;
		public const int MaxDescriptionLength = 64 * 1024; // 64KB
		public const int MaxBrandLength = 128;

		[Required]
		[StringLength(MaxNameLength)]
		public string Name { get; set; }

		[StringLength(MaxDescriptionLength)]
		public string Description { get; set; }

		public DateTime CreationTime { get; set; }

		public ProductState State { get; set; }

		public decimal Price { get; set; }

		public string Image { get; set; }

		// Thương hiệu/Nhà sản xuất
		[StringLength(MaxBrandLength)]
		public string Brand { get; set; }

		// Thông tin khối lượng/kích thước (phục vụ vận chuyển)
		public int? WeightInGrams { get; set; }
		public decimal? WidthCm { get; set; }
		public decimal? HeightCm { get; set; }
		public decimal? LengthCm { get; set; }

		/// <summary>
		/// ID danh mục sản phẩm (bắt buộc)
		/// </summary>
		public int CategoryId { get; set; }
		[ForeignKey("CategoryId")]
		public Category Category { get; set; }

		/// <summary>
		/// ID nhà cung cấp (tùy chọn, nullable)
		/// Liên kết sản phẩm với nhà cung cấp để quản lý nguồn gốc hàng hóa
		/// </summary>
		public int? SupplierId { get; set; }
		[ForeignKey("SupplierId")]
		public Supplier Supplier { get; set; }

		/// <summary>
		/// Quan hệ 1:1 với ProductSpecification - Thông tin kỹ thuật chi tiết
		/// </summary>
		public ProductSpecification Specification { get; set; }

		public Product()
		{
			CreationTime = Clock.Now;
			State = ProductState.Available;
		}

		public Product(string name, string description = null, decimal price = 0, int categoryId = 0)
				: this()
		{
			Name = name;
			Description = description;
			Price = price;
			CategoryId = categoryId;
		}

		public enum ProductState : byte
		{
			Available = 0,
			OutOfStock = 1,
			Discontinued = 2
		}
	}
}
