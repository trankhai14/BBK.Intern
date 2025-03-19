using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;

namespace MyProject.Categories.Dto;

  public class GetAllCategoriesInput : PagedAndSortedResultRequestDto
  {
  //public int Id { get; set; }
  public string CategoryName { get; set; } // Tìm kiếm theo tên sản phẩm
  public string CategoryDescription { get; set; }
	}
