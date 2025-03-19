using Abp.Application.Services.Dto;
using MyProject.Carts.Dto;

namespace MyProject.Web.Views.Shared.Components.CartMenu
{
	public class CartMenuViewModel
	{

		public ListResultDto<CartsDto> Carts { get; set; }
		public int CartItem { get; set; }

		public int UserId { get; set; }
	}
}
