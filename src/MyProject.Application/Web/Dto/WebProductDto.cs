using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject.Web.Dto
{
	public class WebProductDto
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public decimal Price { get; set; }
		public string Image { get; set; }
		public DateTime CreationTime { get; set; }
		public List<WebProductDto> Products { get; internal set; }

		public WebProductDto()
		{
			Products = new List<WebProductDto>();
		}
	}
}
