using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using MyProject.Controllers;
using MyProject.Inventories;
using MyProject.OrderDetails;
using MyProject.Orders;
using MyProject.Products;
using MyProject.Products.Dtos;
using MyProject.Inventories.Dto;

namespace MyProject.Web.Controllers
{
	public class InventoriesController: MyProjectControllerBase
	{
		private readonly IInventoryAppService inventoryAppService;

		public InventoriesController(IInventoryAppService _inventoryAppService)
		{
			_inventoryAppService = inventoryAppService;
		}

		public async Task<ActionResult> Index()
		{
			//var output = await _inventoryAppService.GetAll(input);
			return View();
		}

		//public async Task<IActionResult> CreateInventory()
		//{
		//	return PartialView();
		//}
	}
}
