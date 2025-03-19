using MyProject.Products.Dtos;

namespace MyProject.Web.Models.Web
{
	public class WebDetailModel
	{
		public ProductDetailDto Product { get; set; }
		public int Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public decimal Price { get; set; }
		public string State { get; set; }
		public string Image { get; set; }
		public int CategoryId
		{
			get; set;
		}

	}
}
