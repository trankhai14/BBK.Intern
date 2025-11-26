using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using MyProject.Authorization;
using MyProject.Inventories.Dto;
using MyProject.InventoryTransactions;


namespace MyProject.Inventories
{
	/// <summary>
	/// Service quản lý kho hàng
	/// </summary>
	//[AbpAuthorize(PermissionNames.Pages_Inventories)]
	public class InventoryAppService : ApplicationService, IInventoryAppService
	{
		private readonly IRepository<Inventory> _inventoryRepository;
		private readonly IRepository<MyProject.Products.Product> _productRepository;
        private readonly IRepository<InventoryTransaction> _transactionRepository;

		public InventoryAppService(
			IRepository<Inventory> inventoryRepository,
			IRepository<MyProject.Products.Product> productRepository,
			IRepository<InventoryTransaction> transactionRepository
		)
		{
			_inventoryRepository = inventoryRepository;
			_productRepository = productRepository;
            _transactionRepository = transactionRepository;
		}

		#region READ - Đọc dữ liệu

		public async Task<PagedResultDto<InventoryListDto>> GetAllInventories(GetAllInventoriesDto input)
		{
			var query = _inventoryRepository.GetAll()
					.Include(x => x.Product)
					 .AsQueryable();

			// Lọc theo ProductId
			if (input.ProductId.HasValue)
			{
				query =  query.Where(x => x.ProductId == input.ProductId.Value);
			}

			// Lọc theo tên sản phẩm
			if (!string.IsNullOrWhiteSpace(input.ProductName))
			{
				query =  query.Where(x => x.Product.Name.Contains(input.ProductName));
			}

			if (input.MinQuantity.HasValue)
			{
				query =  query.Where(x => x.Quantity >= input.MinQuantity.Value);
			}

			if (input.MaxQuantity.HasValue)
			{
				query =  query.Where(x => x.Quantity <= input.MaxQuantity.Value);
			}

			//if (input.IsLowStock.HasValue && input.IsLowStock.Value)
			//{
			//	query = query.Where(x => x.Quantity <= x.MinQuantity && x.MinQuantity > 0);
			//}

			//if (input.NeedReorder.HasValue && input.NeedReorder.Value)
			//{
			//	query =  query.Where(x => x.Quantity <= x.ReorderLevel && x.ReorderLevel > 0);
			//}

			//if (input.Status.HasValue)
			//{
			//	query =  query.Where(x => x.Status == input.Status.Value);
			//}

			if (!string.IsNullOrWhiteSpace(input.Keyword))
			{
				var keyword = input.Keyword.ToLower();
				query = query.Where(x =>
					x.Product.Name.ToLower().Contains(keyword) ||
					(!string.IsNullOrEmpty(x.Unit) && x.Unit.ToLower().Contains(keyword)) ||
					(!string.IsNullOrEmpty(x.Notes) && x.Notes.ToLower().Contains(keyword))
				);
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
				ReorderLevel = x.ReorderLevel,
				MinQuantity = x.MinQuantity,
				Unit = x.Unit ?? "cái",
				Status = x.Status,
				StatusName = GetStatusName(x.Status),
				IsLowStock = x.IsLowStock,
				NeedReorder = x.NeedReorder,
				CreateTime = x.CreationTime,
				LastUpdateTime = x.LastUpdated,
				Notes = x.Notes
			}).ToList();

			return new PagedResultDto<InventoryListDto>(totalCount, dtos);
		}


		public async Task<InventoryDetailDto> GetInventoryById(int id)
		{
			var inventory = await _inventoryRepository.GetAll()
				.Include(x => x.Product)
				.FirstOrDefaultAsync(x => x.Id == id);

			if (inventory == null)
				throw new UserFriendlyException("Không tìm thấy kho hàng này");

			return MapToDetailDto(inventory);
		}

		public async Task<InventoryDetailDto> GetInventoryByProductId(int productId)
		{
			var inventory = await _inventoryRepository
				.GetAll()
				.Where(x => x.ProductId == productId)
				.Include(x => x.Product)
				.FirstOrDefaultAsync();

			// Debug: Nếu inventory null, log lại các giá trị liên quan để kiểm tra
			//if (inventory == null)
			//	throw new UserFriendlyException();

			return MapToDetailDto(inventory);
		}

		#endregion

		#region CREATE - Tạo mới

		public async Task<InventoryDetailDto> CreateInventory(CreateInventoryDto input)
		{
			// Kiểm tra sản phẩm có tồn tại không
			var product = await _productRepository.GetAsync(input.ProductId);

			// Kiểm tra xem đã có inventory cho product này chưa
			var existingInventory = await _inventoryRepository.FirstOrDefaultAsync(x => x.ProductId == input.ProductId);
			if (existingInventory != null)
			{
				throw new UserFriendlyException($"Đã tồn tại kho hàng cho sản phẩm: {product.Name}");
			}

			// Kiểm tra ReservedQuantity không được lớn hơn Quantity
			if (input.ReservedQuantity > input.Quantity)
			{
				throw new UserFriendlyException("Số lượng giữ không được lớn hơn số lượng trong kho");
			}

			var inventory = new Inventory
			{
				ProductId = input.ProductId,
				Quantity = input.Quantity,
				ReservedQuantity = input.ReservedQuantity,
				ReorderLevel = input.ReorderLevel,
				MinQuantity = input.MinQuantity,
				Unit = string.IsNullOrWhiteSpace(input.Unit) ? "cái" : input.Unit,
				Status = input.Status,
				Notes = input.Notes,
				LastUpdated = DateTime.Now
			};

			await _inventoryRepository.InsertAsync(inventory);
			await CurrentUnitOfWork.SaveChangesAsync();

			// Load lại với Product
			var loadedInventory = await _inventoryRepository.GetAll()
				.Include(x => x.Product)
				.FirstOrDefaultAsync(x => x.Id == inventory.Id);

			return MapToDetailDto(loadedInventory);
		}

		#endregion

		#region UPDATE - Cập nhật

		public async Task<InventoryDetailDto> UpdateInventory(UpdateInventoryDto input)
		{
			var inventory = await _inventoryRepository.GetAll()
				.Include(x => x.Product)
				.FirstOrDefaultAsync(x => x.Id == input.Id);

			if (inventory == null)
				throw new UserFriendlyException("Không tìm thấy kho hàng này");

			// Cập nhật các trường nếu có giá trị
			if (input.Quantity.HasValue)
			{
				if (input.Quantity.Value < 0)
					throw new UserFriendlyException("Số lượng không được âm");

				inventory.Quantity = input.Quantity.Value;
			}

			if (input.ReservedQuantity.HasValue)
			{
				if (input.ReservedQuantity.Value < 0)
					throw new UserFriendlyException("Số lượng giữ không được âm");

				// Kiểm tra ReservedQuantity không được lớn hơn Quantity
				var currentQuantity = input.Quantity ?? inventory.Quantity;
				if (input.ReservedQuantity.Value > currentQuantity)
				{
					throw new UserFriendlyException("Số lượng giữ không được lớn hơn số lượng trong kho");
				}

				inventory.ReservedQuantity = input.ReservedQuantity.Value;
			}

			if (input.ReorderLevel.HasValue)
			{
				if (input.ReorderLevel.Value < 0)
					throw new UserFriendlyException("Ngưỡng đặt lại không được âm");

				inventory.ReorderLevel = input.ReorderLevel.Value;
			}

			if (input.MinQuantity.HasValue)
			{
				if (input.MinQuantity.Value < 0)
					throw new UserFriendlyException("Số lượng tối thiểu không được âm");

				inventory.MinQuantity = input.MinQuantity.Value;
			}

			if (!string.IsNullOrWhiteSpace(input.Unit))
			{
				inventory.Unit = input.Unit;
			}

			if (input.Status.HasValue)
			{
				inventory.Status = input.Status.Value;
			}

			if (input.Notes != null)
			{
				inventory.Notes = input.Notes;
			}

			inventory.LastUpdated = DateTime.Now;

			await _inventoryRepository.UpdateAsync(inventory);
			await CurrentUnitOfWork.SaveChangesAsync();

			// Load lại với Product
			var updatedInventory = await _inventoryRepository.GetAll()
				.Include(x => x.Product)
				.FirstOrDefaultAsync(x => x.Id == inventory.Id);

			return MapToDetailDto(updatedInventory);
		}

		public async Task<Inventory> UpdateInventoryQuantity(int id, int quantity)
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

		#endregion

		#region DELETE - Xóa

		public async Task DeleteInventory(int id)
		{
			var inventory = await _inventoryRepository.GetAsync(id);

			// Kiểm tra nếu có hàng đang giữ thì không cho xóa
			if (inventory.ReservedQuantity > 0)
			{
				throw new UserFriendlyException($"Không thể xóa kho hàng. Đang có {inventory.ReservedQuantity} sản phẩm được giữ cho đơn hàng");
			}

			await _inventoryRepository.DeleteAsync(inventory);
		}

		public async Task DeleteInventoryByProductId(int productId)
		{
			var inventory = await _inventoryRepository.FirstOrDefaultAsync(x => x.ProductId == productId);
			if (inventory != null)
			{
				// Kiểm tra nếu có hàng đang giữ thì không cho xóa
				if (inventory.ReservedQuantity > 0)
				{
					throw new UserFriendlyException($"Không thể xóa kho hàng. Đang có {inventory.ReservedQuantity} sản phẩm được giữ cho đơn hàng");
				}

				await _inventoryRepository.DeleteAsync(inventory);
			}
		}

		#endregion

		#region UTILITY - Tiện ích

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
					MinQuantity = 0,
					Unit = "cái",
					Status = InventoryStatus.Active,
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

		public async Task<Inventory> DecreaseInventory(int productId, int quantity)
		{
			if (quantity <= 0)
				throw new UserFriendlyException("Số lượng phải lớn hơn 0");

			var inventory = await _inventoryRepository.FirstOrDefaultAsync(x => x.ProductId == productId);
			if (inventory == null)
				throw new UserFriendlyException("Không tìm thấy kho hàng cho sản phẩm này");

			if (inventory.AvailableQuantity < quantity)
				throw new UserFriendlyException($"Không đủ hàng trong kho. Hiện tại còn {inventory.AvailableQuantity} sản phẩm có thể xuất");

			inventory.Quantity -= quantity;
			inventory.LastUpdated = DateTime.Now;

			await _inventoryRepository.UpdateAsync(inventory);
			await CurrentUnitOfWork.SaveChangesAsync();

			return inventory;
		}

		public async Task<bool> CheckInventorySufficient(int productId, int requiredQuantity)
		{
			var inventory = await _inventoryRepository.FirstOrDefaultAsync(x => x.ProductId == productId);
			if (inventory == null)
				return false;

			return inventory.AvailableQuantity >= requiredQuantity;
		}

		public async Task ReserveInventory(int productId, int quantity)
		{
			if (quantity <= 0)
				throw new UserFriendlyException("Số lượng giữ phải lớn hơn 0");

			var inventory = await _inventoryRepository.FirstOrDefaultAsync(x => x.ProductId == productId);
			if (inventory == null)
				throw new UserFriendlyException("Không tìm thấy kho hàng cho sản phẩm này");

			if (inventory.AvailableQuantity < quantity)
				throw new UserFriendlyException($"Không đủ hàng trong kho. Hiện tại còn {inventory.AvailableQuantity} sản phẩm có thể giữ");

			inventory.ReservedQuantity += quantity;
			inventory.LastUpdated = DateTime.Now;

			await _inventoryRepository.UpdateAsync(inventory);
			await CurrentUnitOfWork.SaveChangesAsync();
		}

		public async Task CommitReservedInventory(int productId, int quantity)
		{
			if (quantity <= 0)
				throw new UserFriendlyException("Số lượng phải lớn hơn 0");

			var inventory = await _inventoryRepository.FirstOrDefaultAsync(x => x.ProductId == productId);
			if (inventory == null)
				throw new UserFriendlyException("Không tìm thấy kho hàng cho sản phẩm này");

			if (inventory.ReservedQuantity < quantity)
				throw new UserFriendlyException("Số lượng giữ không hợp lệ");

			if (inventory.Quantity < quantity)
				throw new UserFriendlyException("Kho hàng không đủ số lượng để trừ");

			inventory.ReservedQuantity -= quantity;
			inventory.Quantity -= quantity;
			inventory.LastUpdated = DateTime.Now;

			await _inventoryRepository.UpdateAsync(inventory);
			await CurrentUnitOfWork.SaveChangesAsync();
		}

		public async Task ReleaseReservedInventory(int productId, int quantity)
		{
			if (quantity <= 0)
				throw new UserFriendlyException("Số lượng phải lớn hơn 0");

			var inventory = await _inventoryRepository.FirstOrDefaultAsync(x => x.ProductId == productId);
			if (inventory == null)
				throw new UserFriendlyException("Không tìm thấy kho hàng cho sản phẩm này");

			if (inventory.ReservedQuantity < quantity)
				throw new UserFriendlyException("Số lượng giữ không hợp lệ");

			inventory.ReservedQuantity -= quantity;
			inventory.LastUpdated = DateTime.Now;

			await _inventoryRepository.UpdateAsync(inventory);
			await CurrentUnitOfWork.SaveChangesAsync();
		}

		#endregion

		#region Private Methods

		private InventoryDetailDto MapToDetailDto(Inventory inventory)
		{
			if (inventory == null) return null;

			return new InventoryDetailDto
			{
				Id = inventory.Id,
				ProductId = inventory.ProductId,
				ProductName = inventory.Product?.Name ?? "",
				ProductDescription = inventory.Product?.Description ?? "",
				ProductPrice = inventory.Product?.Price ?? 0,
				Quantity = inventory.Quantity,
				ReservedQuantity = inventory.ReservedQuantity,
				AvailableQuantity = inventory.AvailableQuantity,
				ReorderLevel = inventory.ReorderLevel,
				MinQuantity = inventory.MinQuantity,
				Unit = inventory.Unit ?? "cái",
				Status = inventory.Status,
				StatusName = GetStatusName(inventory.Status),
				IsLowStock = inventory.IsLowStock,
				NeedReorder = inventory.NeedReorder,
				CreateTime = inventory.CreationTime,
				LastUpdateTime = inventory.LastUpdated,
				Notes = inventory.Notes,
				CreatorUserName = null, // Có thể load thêm nếu cần
				LastModifierUserName = null // Có thể load thêm nếu cần
			};
		}

		private string GetStatusName(InventoryStatus status)
		{
			return status switch
			{
				InventoryStatus.Active => "Đang hoạt động",
				InventoryStatus.Inactive => "Tạm ngưng",
				InventoryStatus.Discontinued => "Ngừng kinh doanh",
				_ => "Không xác định"
			};
		}

		#endregion
	}
}
