using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using MyProject.Controllers;
using MyProject.Inventories;
using MyProject.Inventories.Dto;
using MyProject.Web.Models.Inventories;

namespace MyProject.Web.Controllers
{
	public class InventoriesController: MyProjectControllerBase
	{
		private readonly IInventoryAppService _inventoryAppService;

		public InventoriesController(IInventoryAppService inventoryAppService)
		{
			_inventoryAppService = inventoryAppService;
		}

		public async Task<ActionResult> Index(GetAllInventoriesDto input)
		{
			if (input == null)
			{
				input = new GetAllInventoriesDto();
			}
			
			var output = await _inventoryAppService.GetAllInventories(input);
			var model = new InventoryViewModel
			{
				InventoryLists = output.Items.ToList()
			};
			
			return View(model);
		}

		public async Task<ActionResult> EditModal(int inventoryId)
		{
			var inventory = await _inventoryAppService.GetInventoryById(inventoryId);
			var model = new EditInventoryViewModel
			{
				Inventory = inventory
			};
			return PartialView("_EditModal", model);
		}

		public async Task<ActionResult> Detail(int inventoryId)
		{
			var inventory = await _inventoryAppService.GetInventoryById(inventoryId);
			var model = new EditInventoryViewModel
			{
				Inventory = inventory
			};
			return View(model);
		}
	}
}
