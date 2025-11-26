using System;
using System.Collections.Generic;
using Abp.Application.Services.Dto;
using Abp.Domain.Entities.Auditing;
using Microsoft.AspNetCore.Http;
using static MyProject.Products.Product;
using MyProject.Products.Dtos;

namespace MyProject.Product.Dtos
{
	public class ProductListDto : EntityDto, IHasCreationTime
	{

		public string Name { get; set; }

		public string Description { get; set; }

		public decimal Price { get; set; }
		public DateTime CreationTime { get; set; }

		public ProductState State { get; set; }

		public IFormFile ImageFile { get; set; }  // Thêm thuộc tính này
		public string Image { get; set; }  // Lưu đường dẫn ảnh
		public string Brand { get; set; }
		public int? WeightInGrams { get; set; }
		public decimal? WidthCm { get; set; }
		public decimal? HeightCm { get; set; }
		public decimal? LengthCm { get; set; }

		public int CategoryId { get; set; }
		public string CategoryName { get; set; }
		public int? SupplierId { get; set; }
		public string SupplierName { get; set; }

		// Thông tin kỹ thuật (tách riêng - chỉ load khi cần)
		public ProductSpecificationDto Specification { get; set; }

		public List<ProductListDto> Products { get; internal set; }
	}
}
