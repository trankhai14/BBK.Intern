using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;

namespace MyProject.Categories.Dto
{
	public class CreateCategoryDto : EntityDto
	{
		public string CategoryName { get; set; }
		public string CategoryDescription { get; set; }
	}
}
