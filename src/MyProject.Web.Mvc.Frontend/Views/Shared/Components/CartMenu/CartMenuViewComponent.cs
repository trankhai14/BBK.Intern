using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyProject.Carts;

namespace MyProject.Web.Views.Shared.Components.CartMenu
{
	public class CartMenuViewComponent : MyProjectViewComponent
	{
		private readonly ICartFrontendAppService _cartFrontendAppService;

		public CartMenuViewComponent(ICartFrontendAppService cartFrontendAppService)
		{
			_cartFrontendAppService = cartFrontendAppService;
		}

		public async Task<IViewComponentResult> InvokeAsync()
		{
			var model = new CartMenuViewModel
			{
				CartItem = await _cartFrontendAppService.CountCart()
			};
			return View(model);
		}
	}
}
