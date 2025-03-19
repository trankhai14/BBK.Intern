using System;
using System.Collections.Generic;
using Abp.Application.Services.Dto;
using Abp.Domain.Entities.Auditing;
using Microsoft.AspNetCore.Http;
using static MyProject.Products.Product;

namespace MyProject.Product.Dtos
{
	public class ProductListDto : EntityDto, IHasCreationTime
	{

		public string Name { get; set; }

		public string Description { get; set; }

		public decimal Price;
		public DateTime CreationTime { get; set; }

		public ProductState State { get; set; }

		public IFormFile ImageFile { get; set; }  // Thêm thuộc tính này
		public string Image { get; set; }  // Lưu đường dẫn ảnh

		public int CategoryId { get; set; }
		public string CategoryName { get; set; }
		public List<ProductListDto> Products { get; internal set; }
	}
}
