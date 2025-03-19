using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using static MyProject.Products.Product;

namespace MyProject.Products.Dtos
{
    public class GetAllProductsInput : PagedAndSortedResultRequestDto
    {
		public int? CategoryId { get; set; }
    public string CategoryName { get; set; } // Lọc theo tên danh mục
		public string Name { get; set; } // Tìm kiếm theo tên sản phẩm
    public ProductState? State { get; set; } // Lọc theo trạng thái sản phẩm

    public DateTime CreateTime { get; set; }
    public string StateInput { get; set; }
    public string CategoryInput { get; set; }

    public string Keyword { get; set; }
	}
}
