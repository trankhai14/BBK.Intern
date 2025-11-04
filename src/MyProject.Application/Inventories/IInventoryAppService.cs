using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using MyProject.Inventories.Dto;

namespace MyProject.Inventories
{
	public interface IInventoryAppService : IApplicationService
	{
		Task<PagedResultDto<InventoryListDto>> GetAllInventories(GetAllInventoriesDto input);
		Task CreateInventory(CreateInventoryDto input);
		Task<Inventory> UpdateInventory(int id, int quantity);
		Task<Inventory> UpdateInventoryByProductId(int productId, int quantity);
		Task DeleteInventory(int id);
		Task DeleteInventoryByProductId(int productId);
		Task<Inventory> GetInventoryById(int id);
		Task<Inventory> GetInventoryByProductId(int productId);
		Task<Inventory> IncreaseInventory(int productId, int quantity);
		Task<Inventory> DecreaseInventory(int productId, int quantity);
		Task<bool> CheckInventorySufficient(int productId, int requiredQuantity);
	}
}
