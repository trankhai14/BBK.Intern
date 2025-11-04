using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using MyProject.Inventories.Dto;
using MyProject.Products;

namespace MyProject.Inventories
{
	public class InventoryAppService : ApplicationService, IInventoryAppService
	{
		private readonly IRepository<Inventory> _inventoryRepository;
		private readonly IRepository<Product> _productRepository;

		public InventoryAppService(
			IRepository<Inventory> inventoryRepository,
			IRepository<Product> productRepository
		)
		{
			_inventoryRepository = inventoryRepository;
			_productRepository = productRepository;
		}

		public async Task<bool> CheckInventorySufficient(int productId, int requiredQuantity)
		{
			var inventory = await _inventoryRepository.FirstOrDefaultAsync(x => x.ProductId == productId);
			if (inventory == null)
				return false;

			return inventory.AvailableQuantity >= requiredQuantity;
		}

		public async Task CreateInventory(CreateInventoryDto input)
		{
			// Kiểm tra xem đã có inventory cho product này chưa
			var existingInventory = await _inventoryRepository.FirstOrDefaultAsync(x => x.ProductId == input.ProductId);
			if (existingInventory != null)
			{
				throw new UserFriendlyException($"Đã tồn tại kho hàng cho sản phẩm ID: {input.ProductId}");
			}

			var inventory = new Inventory
			{
				ProductId = input.ProductId,
				Quantity = input.Quantity,
				ReservedQuantity = input.ReservedQuantity,
				ReorderLevel = 0,
				LastUpdated = DateTime.Now
			};

			await _inventoryRepository.InsertAsync(inventory);
		}

		public async Task<Inventory> DecreaseInventory(int productId, int quantity)
		{
			if (quantity <= 0)
				throw new UserFriendlyException("Số lượng phải lớn hơn 0");

			var inventory = await _inventoryRepository.FirstOrDefaultAsync(x => x.ProductId == productId);
			if (inventory == null)
				throw new UserFriendlyException("Không tìm thấy kho hàng cho sản phẩm này");

			if (inventory.AvailableQuantity < quantity)
				throw new UserFriendlyException($"Không đủ hàng trong kho. Hiện tại còn {inventory.AvailableQuantity} sản phẩm");

			inventory.Quantity -= quantity;
			inventory.LastUpdated = DateTime.Now;

			await _inventoryRepository.UpdateAsync(inventory);
			await CurrentUnitOfWork.SaveChangesAsync();

			return inventory;
		}

		public async Task DeleteInventory(int id)
		{
			var inventory = await _inventoryRepository.GetAsync(id);
			await _inventoryRepository.DeleteAsync(inventory);
		}

		public async Task DeleteInventoryByProductId(int productId)
		{
			var inventory = await _inventoryRepository.FirstOrDefaultAsync(x => x.ProductId == productId);
			if (inventory != null)
			{
				await _inventoryRepository.DeleteAsync(inventory);
			}
		}

		public async Task<PagedResultDto<InventoryListDto>> GetAllInventories(GetAllInventoriesDto input)
		{
			var query = _inventoryRepository.GetAll()
				.Include(x => x.Product);

			// Lọc theo ProductId nếu có
			if (input.ProductId.HasValue)
			{
				query = query.Where(x => x.ProductId == input.ProductId.Value);
			}

			// Lọc theo tên sản phẩm nếu có
			if (!string.IsNullOrWhiteSpace(input.ProductName))
			{
				query = query.Where(x => x.Product.Name.Contains(input.ProductName));
			}

			// Lọc theo số lượng
			if (input.MinQuantity.HasValue)
			{
				query = query.Where(x => x.Quantity >= input.MinQuantity.Value);
			}

			if (input.MaxQuantity.HasValue)
			{
				query = query.Where(x => x.Quantity <= input.MaxQuantity.Value);
			}

			var totalCount = await query.CountAsync();

			var items = await query
				.OrderByDescending(x => x.LastUpdated)
				.PageBy(input)
				.ToListAsync();

			var dtos = items.Select(x => new InventoryListDto
			{
				Id = x.Id,
				ProductId = x.ProductId,
				ProductName = x.Product?.Name ?? "",
				Quantity = x.Quantity,
				ReservedQuantity = x.ReservedQuantity,
				AvailableQuantity = x.AvailableQuantity,
				CreateTime = x.CreationTime,
				LastUpdateTime = x.LastUpdated
			}).ToList();

			return new PagedResultDto<InventoryListDto>(totalCount, dtos);
		}

		public async Task<Inventory> GetInventoryById(int id)
		{
			return await _inventoryRepository.GetAsync(id);
		}

		public async Task<Inventory> GetInventoryByProductId(int productId)
		{
			return await _inventoryRepository.FirstOrDefaultAsync(x => x.ProductId == productId);
		}

		public async Task<Inventory> IncreaseInventory(int productId, int quantity)
		{
			if (quantity <= 0)
				throw new UserFriendlyException("Số lượng phải lớn hơn 0");

			var inventory = await _inventoryRepository.FirstOrDefaultAsync(x => x.ProductId == productId);
			if (inventory == null)
			{
				// Nếu chưa có inventory, tạo mới
				inventory = new Inventory
				{
					ProductId = productId,
					Quantity = quantity,
					ReservedQuantity = 0,
					ReorderLevel = 0,
					LastUpdated = DateTime.Now
				};
				await _inventoryRepository.InsertAsync(inventory);
			}
			else
			{
				inventory.Quantity += quantity;
				inventory.LastUpdated = DateTime.Now;
				await _inventoryRepository.UpdateAsync(inventory);
			}

			await CurrentUnitOfWork.SaveChangesAsync();
			return inventory;
		}

		public async Task<Inventory> UpdateInventory(int id, int quantity)
		{
			if (quantity < 0)
				throw new UserFriendlyException("Số lượng không được âm");

			var inventory = await _inventoryRepository.GetAsync(id);
			inventory.Quantity = quantity;
			inventory.LastUpdated = DateTime.Now;

			await _inventoryRepository.UpdateAsync(inventory);
			await CurrentUnitOfWork.SaveChangesAsync();

			return inventory;
		}

		public async Task<Inventory> UpdateInventoryByProductId(int productId, int quantity)
		{
			if (quantity < 0)
				throw new UserFriendlyException("Số lượng không được âm");

			var inventory = await _inventoryRepository.FirstOrDefaultAsync(x => x.ProductId == productId);
			if (inventory == null)
				throw new UserFriendlyException("Không tìm thấy kho hàng cho sản phẩm này");

			inventory.Quantity = quantity;
			inventory.LastUpdated = DateTime.Now;

			await _inventoryRepository.UpdateAsync(inventory);
			await CurrentUnitOfWork.SaveChangesAsync();

			return inventory;
		}
	}
}
