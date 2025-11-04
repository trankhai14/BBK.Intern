using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using MyProject.Inventories.Dto;

namespace MyProject.Inventories
{
	/// <summary>
	/// Service quản lý nhập xuất kho
	/// </summary>
	public class InventoryTransactionAppService : ApplicationService, IInventoryTransactionAppService
	{
		private readonly IRepository<InventoryTransaction> _transactionRepository;
		private readonly IRepository<Inventory> _inventoryRepository;
		private readonly IInventoryAppService _inventoryAppService;

		public InventoryTransactionAppService(
			IRepository<InventoryTransaction> transactionRepository,
			IRepository<Inventory> inventoryRepository,
			IInventoryAppService inventoryAppService
		)
		{
			_transactionRepository = transactionRepository;
			_inventoryRepository = inventoryRepository;
			_inventoryAppService = inventoryAppService;
		}

		public async Task<InventoryTransactionDto> ImportInventory(ImportInventoryDto input)
		{
			if (input.Quantity <= 0)
				throw new UserFriendlyException("Số lượng nhập phải lớn hơn 0");

			// Lấy thông tin inventory hiện tại
			var inventory = await _inventoryRepository.FirstOrDefaultAsync(x => x.ProductId == input.ProductId);
			var quantityBefore = inventory?.Quantity ?? 0;

			// Tăng số lượng trong kho
			await _inventoryAppService.IncreaseInventory(input.ProductId, input.Quantity);

			// Lấy lại inventory sau khi cập nhật
			inventory = await _inventoryRepository.FirstOrDefaultAsync(x => x.ProductId == input.ProductId);
			var quantityAfter = inventory.Quantity;

			// Tạo bản ghi giao dịch
			var transaction = new InventoryTransaction
			{
				Type = TransactionType.Import,
				ProductId = input.ProductId,
				Quantity = input.Quantity,
				QuantityBefore = quantityBefore,
				QuantityAfter = quantityAfter,
				Reason = input.Reason ?? "Nhập kho",
				Notes = input.Notes,
				UserId = AbpSession.UserId,
				TransactionDate = DateTime.Now
			};

			await _transactionRepository.InsertAsync(transaction);
			await CurrentUnitOfWork.SaveChangesAsync();

			// Load lại transaction với Product và User
			var loadedTransaction = await _transactionRepository.GetAll()
				.Include(x => x.Product)
				.Include(x => x.User)
				.FirstOrDefaultAsync(x => x.Id == transaction.Id);

			return MapToDto(loadedTransaction);
		}

		public async Task<InventoryTransactionDto> ExportInventory(ExportInventoryDto input)
		{
			if (input.Quantity <= 0)
				throw new UserFriendlyException("Số lượng xuất phải lớn hơn 0");

			// Lấy thông tin inventory hiện tại
			var inventory = await _inventoryRepository.FirstOrDefaultAsync(x => x.ProductId == input.ProductId);
			if (inventory == null)
				throw new UserFriendlyException("Không tìm thấy kho hàng cho sản phẩm này");

			var quantityBefore = inventory.Quantity;

			// Kiểm tra số lượng có đủ không
			if (inventory.AvailableQuantity < input.Quantity)
				throw new UserFriendlyException($"Không đủ hàng trong kho. Hiện tại còn {inventory.AvailableQuantity} sản phẩm");

			// Giảm số lượng trong kho
			await _inventoryAppService.DecreaseInventory(input.ProductId, input.Quantity);

			// Lấy lại inventory sau khi cập nhật
			inventory = await _inventoryRepository.FirstOrDefaultAsync(x => x.ProductId == input.ProductId);
			var quantityAfter = inventory.Quantity;

			// Tạo bản ghi giao dịch
			var transaction = new InventoryTransaction
			{
				Type = TransactionType.Export,
				ProductId = input.ProductId,
				Quantity = input.Quantity,
				QuantityBefore = quantityBefore,
				QuantityAfter = quantityAfter,
				Reason = input.Reason ?? "Xuất kho",
				Notes = input.Notes,
				UserId = AbpSession.UserId,
				TransactionDate = DateTime.Now
			};

			await _transactionRepository.InsertAsync(transaction);
			await CurrentUnitOfWork.SaveChangesAsync();

			// Load lại transaction với Product và User
			var loadedTransaction = await _transactionRepository.GetAll()
				.Include(x => x.Product)
				.Include(x => x.User)
				.FirstOrDefaultAsync(x => x.Id == transaction.Id);

			return MapToDto(loadedTransaction);
		}

		public async Task<PagedResultDto<InventoryTransactionDto>> GetAllTransactions(GetAllInventoryTransactionsDto input)
		{
			var query = _transactionRepository.GetAll()
				.Include(x => x.Product)
				.Include(x => x.User);

			// Lọc theo ProductId
			if (input.ProductId.HasValue)
			{
				query = query.Where(x => x.ProductId == input.ProductId.Value);
			}

			// Lọc theo loại giao dịch
			if (input.Type.HasValue)
			{
				query = query.Where(x => x.Type == input.Type.Value);
			}

			// Lọc theo khoảng thời gian
			if (input.FromDate.HasValue)
			{
				query = query.Where(x => x.TransactionDate >= input.FromDate.Value);
			}

			if (input.ToDate.HasValue)
			{
				query = query.Where(x => x.TransactionDate <= input.ToDate.Value);
			}

			// Tìm kiếm theo keyword
			if (!string.IsNullOrWhiteSpace(input.Keyword))
			{
				var keyword = input.Keyword.ToLower();
				query = query.Where(x =>
					x.Product.Name.ToLower().Contains(keyword) ||
					(x.Reason != null && x.Reason.ToLower().Contains(keyword)) ||
					(x.Notes != null && x.Notes.ToLower().Contains(keyword))
				);
			}

			var totalCount = await query.CountAsync();

			var items = await query
				.OrderByDescending(x => x.TransactionDate)
				.ThenByDescending(x => x.CreationTime)
				.PageBy(input)
				.ToListAsync();

			var dtos = items.Select(MapToDto).ToList();

			return new PagedResultDto<InventoryTransactionDto>(totalCount, dtos);
		}

		public async Task<InventoryTransactionDto> GetTransactionById(int id)
		{
			var transaction = await _transactionRepository.GetAll()
				.Include(x => x.Product)
				.Include(x => x.User)
				.FirstOrDefaultAsync(x => x.Id == id);

			if (transaction == null)
				throw new UserFriendlyException("Không tìm thấy giao dịch này");

			return MapToDto(transaction);
		}

		private InventoryTransactionDto MapToDto(InventoryTransaction transaction)
		{
			return new InventoryTransactionDto
			{
				Id = transaction.Id,
				Type = transaction.Type,
				TypeName = transaction.Type == TransactionType.Import ? "Nhập kho" : "Xuất kho",
				ProductId = transaction.ProductId,
				ProductName = transaction.Product?.Name ?? "",
				Quantity = transaction.Quantity,
				QuantityBefore = transaction.QuantityBefore,
				QuantityAfter = transaction.QuantityAfter,
				Reason = transaction.Reason,
				Notes = transaction.Notes,
				UserId = transaction.UserId,
				UserName = transaction.User?.UserName ?? transaction.User?.Name ?? "",
				TransactionDate = transaction.TransactionDate,
				CreationTime = transaction.CreationTime
			};
		}
	}
}
