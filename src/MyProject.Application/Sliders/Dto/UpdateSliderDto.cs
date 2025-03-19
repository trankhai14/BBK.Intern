using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Microsoft.AspNetCore.Http;

namespace MyProject.Sliders.Dto
{
	public class UpdateSliderDto: EntityDto
	{
		public string Title { get; set; }
		public string Description { get; set; }
		public string Image { get; set; }
		public bool IsActive { get; set; }
		public IFormFile ImageFile { get; set; }


	}
}
