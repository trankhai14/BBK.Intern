using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyProject.ImportSlips.Dto;
using MyProject.Inventories;
using MyProject.InventoryTransactions;
using MyProject.Products;
using MyProject.Suppliers;

namespace MyProject.ImportSlips
{
	/// <summary>
	/// Service quản lý phiếu nhập kho
	/// </summary>
	public class ImportSlipAppService : ApplicationService, IImportSlipAppService
	{
		private readonly IRepository<ImportSlip> _importSlipRepository;
		private readonly IRepository<ImportDetail> _importDetailRepository;
		private readonly IRepository<Inventory> _inventoryRepository;
		private readonly IRepository<InventoryTransaction> _transactionRepository;
		private readonly IRepository<MyProject.Products.Product> _productRepository;
		private readonly IRepository<Supplier> _supplierRepository;

		public ImportSlipAppService(
			IRepository<ImportSlip> importSlipRepository,
			IRepository<ImportDetail> importDetailRepository,
			IRepository<Inventory> inventoryRepository,
			IRepository<InventoryTransaction> transactionRepository,
			IRepository<MyProject.Products.Product> productRepository,
			IRepository<Supplier> supplierRepository
		)
		{
			_importSlipRepository = importSlipRepository;
			_importDetailRepository = importDetailRepository;
			_inventoryRepository = inventoryRepository;
			_transactionRepository = transactionRepository;
			_productRepository = productRepository;
			_supplierRepository = supplierRepository;
		}

		/// <summary>
		/// Tạo phiếu nhập kho mới (trạng thái Draft)
		/// </summary>
		public async Task<ImportSlipDto> CreateImportSlip(CreateImportSlipDto input)
		{
			// Validate
			if (input.Details == null || !input.Details.Any())
				throw new UserFriendlyException("Phiếu nhập phải có ít nhất 1 sản phẩm");

			// Validate sản phẩm tồn tại
			var productIds = input.Details.Select(d => d.ProductId).Distinct().ToList();
			var products = await _productRepository.GetAll()
				.Where(p => productIds.Contains(p.Id))
				.ToListAsync();

			if (products.Count != productIds.Count)
				throw new UserFriendlyException("Có sản phẩm không tồn tại trong hệ thống");

			// Validate nhà cung cấp nếu có
			if (input.SupplierId.HasValue)
			{
				var supplier = await _supplierRepository.FirstOrDefaultAsync(x => x.Id == input.SupplierId.Value);
				if (supplier == null)
					throw new UserFriendlyException("Nhà cung cấp không tồn tại");
			}

			// Tạo mã phiếu nhập tự động
			var importCode = await GenerateImportCodeAsync();

			// Tính tổng tiền
			decimal totalAmount = input.Details.Sum(d => d.Quantity * d.UnitPrice);

			// Tạo ImportSlip
			var importSlip = new ImportSlip
			{
				ImportCode = importCode,
				ImportDate = input.ImportDate,
				SupplierId = input.SupplierId,
				Type = input.Type,
				Status = ImportStatus.Draft,
				TotalAmount = totalAmount,
				Notes = input.Notes
			};

			var importSlipId = await _importSlipRepository.InsertAndGetIdAsync(importSlip);

			// Tạo ImportDetail
			foreach (var detailDto in input.Details)
			{
				var detail = new ImportDetail
				{
					ImportSlipId = importSlipId,
					ProductId = detailDto.ProductId,
					Quantity = detailDto.Quantity,
					UnitPrice = detailDto.UnitPrice,
					TotalAmount = detailDto.Quantity * detailDto.UnitPrice,
					Notes = detailDto.Notes
				};
				await _importDetailRepository.InsertAsync(detail);
			}

			await CurrentUnitOfWork.SaveChangesAsync();

			return await GetImportSlipById(importSlipId);
		}

		/// <summary>
		/// Cập nhật phiếu nhập kho (chỉ khi Status = Draft)
		/// </summary>
		public async Task<ImportSlipDto> UpdateImportSlip(UpdateImportSlipDto input)
		{
			var importSlip = await _importSlipRepository.GetAll()
				.Include(x => x.Details)
				.FirstOrDefaultAsync(x => x.Id == input.Id);

			if (importSlip == null)
				throw new UserFriendlyException("Không tìm thấy phiếu nhập kho");

			if (importSlip.Status != ImportStatus.Draft)
				throw new UserFriendlyException($"Chỉ có thể sửa phiếu nhập ở trạng thái Nháp. Trạng thái hiện tại: {GetStatusName(importSlip.Status)}");

			// Validate sản phẩm
			if (input.Details != null && input.Details.Any())
			{
				var productIds = input.Details.Select(d => d.ProductId).Distinct().ToList();
				var products = await _productRepository.GetAll()
					.Where(p => productIds.Contains(p.Id))
					.ToListAsync();

				if (products.Count != productIds.Count)
					throw new UserFriendlyException("Có sản phẩm không tồn tại trong hệ thống");
			}

			// Validate nhà cung cấp nếu có
			if (input.SupplierId.HasValue)
			{
				var supplier = await _supplierRepository.FirstOrDefaultAsync(x => x.Id == input.SupplierId.Value);
				if (supplier == null)
					throw new UserFriendlyException("Nhà cung cấp không tồn tại");
			}

			// Cập nhật thông tin chung
			importSlip.ImportDate = input.ImportDate;
			importSlip.SupplierId = input.SupplierId;
			importSlip.Type = input.Type;
			importSlip.Notes = input.Notes;

			// Xóa các detail cũ không còn trong danh sách mới
			if (input.Details != null && input.Details.Any())
			{
				var existingDetailIds = input.Details.Where(d => d.Id.HasValue).Select(d => d.Id.Value).ToList();
				var detailsToDelete = importSlip.Details.Where(d => !existingDetailIds.Contains(d.Id)).ToList();
				foreach (var detail in detailsToDelete)
				{
					await _importDetailRepository.DeleteAsync(detail);
				}

				// Cập nhật hoặc tạo mới detail
				foreach (var detailDto in input.Details)
				{
					if (detailDto.Id.HasValue)
					{
						// Cập nhật
						var detail = await _importDetailRepository.GetAsync(detailDto.Id.Value);
						detail.ProductId = detailDto.ProductId;
						detail.Quantity = detailDto.Quantity;
						detail.UnitPrice = detailDto.UnitPrice;
						detail.TotalAmount = detailDto.Quantity * detailDto.UnitPrice;
						detail.Notes = detailDto.Notes;
						await _importDetailRepository.UpdateAsync(detail);
					}
					else
					{
						// Tạo mới
						var detail = new ImportDetail
						{
							ImportSlipId = importSlip.Id,
							ProductId = detailDto.ProductId,
							Quantity = detailDto.Quantity,
							UnitPrice = detailDto.UnitPrice,
							TotalAmount = detailDto.Quantity * detailDto.UnitPrice,
							Notes = detailDto.Notes
						};
						await _importDetailRepository.InsertAsync(detail);
					}
				}
			}

			// Tính lại tổng tiền
			var allDetails = await _importDetailRepository.GetAll()
				.Where(x => x.ImportSlipId == importSlip.Id)
				.ToListAsync();
			importSlip.TotalAmount = allDetails.Sum(d => d.TotalAmount);

			await _importSlipRepository.UpdateAsync(importSlip);
			await CurrentUnitOfWork.SaveChangesAsync();

			return await GetImportSlipById(importSlip.Id);
		}

		/// <summary>
		/// Xác nhận và hoàn thành phiếu nhập kho
		/// </summary>
		[HttpPost]
		public async Task CompleteImportSlip(int importSlipId)
		{
			// Lấy ImportSlip
			var importSlip = await _importSlipRepository.GetAll()
				.Include(x => x.Details)
				.FirstOrDefaultAsync(x => x.Id == importSlipId);

			if (importSlip == null)
				throw new UserFriendlyException("Không tìm thấy phiếu nhập kho");

			if (importSlip.Status != ImportStatus.Draft)
				throw new UserFriendlyException($"Không thể hoàn thành phiếu nhập ở trạng thái {GetStatusName(importSlip.Status)}. Chỉ có thể hoàn thành phiếu ở trạng thái Nháp.");

			// Lấy tất cả chi tiết
			var details = importSlip.Details.ToList();
			if (!details.Any())
				throw new UserFriendlyException("Phiếu nhập không có sản phẩm nào");

			// Xử lý từng sản phẩm
			foreach (var detail in details)
			{
				// Lấy hoặc tạo Inventory
				var inventory = await _inventoryRepository.FirstOrDefaultAsync(x => x.ProductId == detail.ProductId);

				int quantityBefore = 0;
				bool isNewInventory = false;
				
				if (inventory == null)
				{
					// Tạo mới Inventory
					quantityBefore = 0;
					inventory = new Inventory
					{
						ProductId = detail.ProductId,
						Quantity = detail.Quantity, // Set trực tiếp số lượng nhập
						ReservedQuantity = 0,
						ReorderLevel = 0,
						MinQuantity = 0,
						Unit = "cái",
						Status = InventoryStatus.Active,
						LastUpdated = DateTime.Now
					};
					await _inventoryRepository.InsertAsync(inventory);
					isNewInventory = true;
				}
				else
				{
					quantityBefore = inventory.Quantity;
					// Tăng số lượng cho inventory đã tồn tại
					inventory.Quantity += detail.Quantity;
					inventory.LastUpdated = DateTime.Now;
					await _inventoryRepository.UpdateAsync(inventory);
				}

				int quantityAfter = inventory.Quantity;

				// Tạo InventoryTransaction
				var transaction = new InventoryTransaction
				{
					Type = TransactionType.Import,
					ProductId = detail.ProductId,
					Quantity = detail.Quantity,
					QuantityBefore = quantityBefore,
					QuantityAfter = quantityAfter,
					Reason = GetImportReason(importSlip.Type),
					Notes = $"Nhập từ phiếu: {importSlip.ImportCode}",
					UserId = AbpSession.UserId,
					TransactionDate = importSlip.ImportDate,
					ReferenceId = importSlipId,
					ReferenceType = "ImportSlip"
				};
				await _transactionRepository.InsertAsync(transaction);
			}

			// Cập nhật trạng thái
			importSlip.Status = ImportStatus.Completed;
			await _importSlipRepository.UpdateAsync(importSlip);

			await CurrentUnitOfWork.SaveChangesAsync();
		}

		/// <summary>
		/// Hủy phiếu nhập kho (chỉ khi Status = Draft)
		/// </summary>
		[HttpPost]
		public async Task CancelImportSlip(int importSlipId)
		{
			var importSlip = await _importSlipRepository.GetAsync(importSlipId);

			if (importSlip.Status != ImportStatus.Draft)
				throw new UserFriendlyException($"Chỉ có thể hủy phiếu nhập ở trạng thái Nháp. Trạng thái hiện tại: {GetStatusName(importSlip.Status)}");

			importSlip.Status = ImportStatus.Cancelled;
			await _importSlipRepository.UpdateAsync(importSlip);
			await CurrentUnitOfWork.SaveChangesAsync();
		}

		/// <summary>
		/// Lấy danh sách phiếu nhập kho có phân trang và lọc
		/// </summary>
		public async Task<PagedResultDto<ImportSlipDto>> GetAllImportSlips(GetAllImportSlipsInput input)
		{
			var query = _importSlipRepository.GetAll()
				.Include(x => x.Supplier)
				.Include(x => x.CreatorUser)
				.Include(x => x.Details)
					.ThenInclude(d => d.Product)
				.AsQueryable();

			// Filter
			if (!string.IsNullOrWhiteSpace(input.ImportCode))
				query = query.Where(x => x.ImportCode.Contains(input.ImportCode));

			if (input.SupplierId.HasValue)
				query = query.Where(x => x.SupplierId == input.SupplierId.Value);

			if (input.Type.HasValue)
				query = query.Where(x => x.Type == input.Type.Value);

			if (input.Status.HasValue)
				query = query.Where(x => x.Status == input.Status.Value);

			if (input.FromDate.HasValue)
				query = query.Where(x => x.ImportDate >= input.FromDate.Value);

			if (input.ToDate.HasValue)
				query = query.Where(x => x.ImportDate <= input.ToDate.Value);

			if (!string.IsNullOrWhiteSpace(input.Keyword))
			{
				var keyword = input.Keyword.ToLower();
				query = query.Where(x =>
					x.ImportCode.ToLower().Contains(keyword) ||
					(x.Notes != null && x.Notes.ToLower().Contains(keyword)) ||
					(x.Supplier != null && x.Supplier.Name.ToLower().Contains(keyword))
				);
			}

			var totalCount = await query.CountAsync();

			// Apply sorting - Mặc định sắp xếp theo ngày nhập mới nhất
			query = query.OrderByDescending(x => x.ImportDate)
				.ThenByDescending(x => x.CreationTime);

			query = query.PageBy(input);

			var items = await query.ToListAsync();

			var dtos = items.Select(MapToDto).ToList();

			return new PagedResultDto<ImportSlipDto>(totalCount, dtos);
		}

		/// <summary>
		/// Lấy chi tiết phiếu nhập kho theo ID
		/// </summary>
		public async Task<ImportSlipDto> GetImportSlipById(int id)
		{
			var importSlip = await _importSlipRepository.GetAll()
				.Include(x => x.Supplier)
				.Include(x => x.CreatorUser)
				.Include(x => x.Details)
					.ThenInclude(d => d.Product)
				.FirstOrDefaultAsync(x => x.Id == id);

			if (importSlip == null)
				throw new UserFriendlyException("Không tìm thấy phiếu nhập kho");

			return MapToDto(importSlip);
		}

		/// <summary>
		/// Xóa phiếu nhập kho (chỉ khi Status = Draft)
		/// </summary>
		[HttpPost]
		public async Task DeleteImportSlip(int id)
		{
			var importSlip = await _importSlipRepository.GetAsync(id);

			if (importSlip.Status != ImportStatus.Draft)
				throw new UserFriendlyException($"Chỉ có thể xóa phiếu nhập ở trạng thái Nháp. Trạng thái hiện tại: {GetStatusName(importSlip.Status)}");

			await _importSlipRepository.DeleteAsync(id);
			await CurrentUnitOfWork.SaveChangesAsync();
		}

		#region Private Methods

		/// <summary>
		/// Tạo mã phiếu nhập tự động: PN{yyyyMMdd}{xxxx}
		/// </summary>
		private async Task<string> GenerateImportCodeAsync()
		{
			var today = DateTime.Now;
			var prefix = $"PN{today:yyyyMMdd}";

			var lastCode = await _importSlipRepository.GetAll()
				.Where(x => x.ImportCode.StartsWith(prefix))
				.OrderByDescending(x => x.ImportCode)
				.FirstOrDefaultAsync();

			int sequence = 1;
			if (lastCode != null)
			{
				var lastSequence = lastCode.ImportCode.Substring(prefix.Length);
				if (int.TryParse(lastSequence, out int lastNum))
					sequence = lastNum + 1;
			}

			return $"{prefix}{sequence:D4}"; // VD: PN202401010001
		}

		/// <summary>
		/// Lấy lý do nhập kho theo loại
		/// </summary>
		private string GetImportReason(ImportType type)
		{
			return type switch
			{
				ImportType.Supplier => "Nhập từ nhà cung cấp",
				ImportType.Return => "Nhập hàng trả lại",
				ImportType.Adjustment => "Nhập điều chỉnh",
				ImportType.Transfer => "Nhập chuyển kho",
				_ => "Nhập kho"
			};
		}

		/// <summary>
		/// Map ImportSlip entity sang DTO
		/// </summary>
		private ImportSlipDto MapToDto(ImportSlip importSlip)
		{
			return new ImportSlipDto
			{
				Id = importSlip.Id,
				ImportCode = importSlip.ImportCode,
				ImportDate = importSlip.ImportDate,
				SupplierId = importSlip.SupplierId,
				SupplierName = importSlip.Supplier?.Name,
				Type = importSlip.Type,
				TypeName = GetTypeName(importSlip.Type),
				Status = importSlip.Status,
				StatusName = GetStatusName(importSlip.Status),
				TotalAmount = importSlip.TotalAmount,
				Notes = importSlip.Notes,
				CreatorUserId = importSlip.CreatorUserId,
				CreatorUserName = importSlip.CreatorUser?.UserName ?? importSlip.CreatorUser?.Name,
				CreationTime = importSlip.CreationTime,
				LastModificationTime = importSlip.LastModificationTime,
				Details = importSlip.Details?.Select(d => new ImportDetailDto
				{
					Id = d.Id,
					ImportSlipId = d.ImportSlipId,
					ProductId = d.ProductId,
					ProductName = d.Product?.Name,
					Quantity = d.Quantity,
					UnitPrice = d.UnitPrice,
					TotalAmount = d.TotalAmount,
					Notes = d.Notes
				}).ToList() ?? new List<ImportDetailDto>()
			};
		}

		/// <summary>
		/// Lấy tên loại nhập kho
		/// </summary>
		private string GetTypeName(ImportType type)
		{
			return type switch
			{
				ImportType.Supplier => "Nhập từ nhà cung cấp",
				ImportType.Return => "Nhập hàng trả lại",
				ImportType.Adjustment => "Nhập điều chỉnh",
				ImportType.Transfer => "Nhập chuyển kho",
				_ => "Không xác định"
			};
		}

		/// <summary>
		/// Lấy tên trạng thái phiếu nhập
		/// </summary>
		private string GetStatusName(ImportStatus status)
		{
			return status switch
			{
				ImportStatus.Draft => "Nháp",
				ImportStatus.Completed => "Đã hoàn thành",
				ImportStatus.Cancelled => "Đã hủy",
				_ => "Không xác định"
			};
		}

		#endregion
	}
}

