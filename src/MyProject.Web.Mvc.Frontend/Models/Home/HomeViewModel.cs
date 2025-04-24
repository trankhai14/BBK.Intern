using System.Collections.Generic;
using MyProject.Users.Dto;
using MyProject.Web.Models.Products;

namespace MyProject.Web.Models.Home
{
	public class HomePageViewModel
	{
		public ProductViewModel ProductData { get; set; }
		public List<CategoryProductViewModel> CategoryProducts { get; set; }

	}

	public class ProfileUser
	{
	  public UserDto User { get; set; }
	}
}
