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
using MyProject.ExportSlips.Dto;
using MyProject.Inventories;
using MyProject.InventoryTransactions;
using MyProject.Orders;
using MyProject.Products;
using MyProject.Suppliers;

namespace MyProject.ExportSlips
{
	/// <summary>
	/// Service quản lý phiếu xuất kho
	/// </summary>
	public class ExportSlipAppService : ApplicationService, IExportSlipAppService
	{
		private readonly IRepository<ExportSlip> _exportSlipRepository;
		private readonly IRepository<ExportDetail> _exportDetailRepository;
		private readonly IRepository<Inventory> _inventoryRepository;
		private readonly IRepository<InventoryTransaction> _transactionRepository;
		private readonly IRepository<Product> _productRepository;
		private readonly IRepository<Supplier> _supplierRepository;
		private readonly IRepository<Order> _orderRepository;

		public ExportSlipAppService(
			IRepository<ExportSlip> exportSlipRepository,
			IRepository<ExportDetail> exportDetailRepository,
			IRepository<Inventory> inventoryRepository,
			IRepository<InventoryTransaction> transactionRepository,
			IRepository<Product> productRepository,
			IRepository<Supplier> supplierRepository,
			IRepository<Order> orderRepository
		)
		{
			_exportSlipRepository = exportSlipRepository;
			_exportDetailRepository = exportDetailRepository;
			_inventoryRepository = inventoryRepository;
			_transactionRepository = transactionRepository;
			_productRepository = productRepository;
			_orderRepository = orderRepository;
			_supplierRepository = supplierRepository;
		}

		/// <summary>
		/// Tạo phiếu xuất kho mới (trạng thái Draft)
		/// </summary>
		public async Task<ExportSlipDto> CreateExportSlip(CreateExportSlipDto input)
		{
			// Validate
			if (input.Details == null || !input.Details.Any())
				throw new UserFriendlyException("Phiếu xuất phải có ít nhất 1 sản phẩm");

			// Validate sản phẩm tồn tại
			var productIds = input.Details.Select(d => d.ProductId).Distinct().ToList();
			var products = await _productRepository.GetAll()
				.Where(p => productIds.Contains(p.Id))
				.ToListAsync();

			if (products.Count != productIds.Count)
				throw new UserFriendlyException("Có sản phẩm không tồn tại trong hệ thống");

			// Validate đơn hàng nếu có
			if (input.OrderId.HasValue)
			{
				var order = await _orderRepository.FirstOrDefaultAsync(x => x.Id == input.OrderId.Value);
				if (order == null)
					throw new UserFriendlyException("Đơn hàng không tồn tại");
			}

			// Validate nhà cung cấp nếu có
			if (input.SupplierId.HasValue)
			{
				var supplier = await _supplierRepository.FirstOrDefaultAsync(x => x.Id == input.SupplierId.Value);
				if (supplier == null)
					throw new UserFriendlyException("Nhà cung cấp không tồn tại");
			}

			// Tạo mã phiếu xuất tự động
			var exportCode = await GenerateExportCodeAsync();

			// Tạo ExportSlip
			var exportSlip = new ExportSlip
			{
				ExportCode = exportCode,
				ExportDate = input.ExportDate,
				OrderId = input.OrderId,
				SupplierId = input.SupplierId,
				Type = input.Type,
				Status = ExportStatus.Draft,
				Reason = input.Reason,
				Notes = input.Notes
			};

			var exportSlipId = await _exportSlipRepository.InsertAndGetIdAsync(exportSlip);

			// Tạo ExportDetail
			foreach (var detailDto in input.Details)
			{
				var detail = new ExportDetail
				{
					ExportSlipId = exportSlipId,
					ProductId = detailDto.ProductId,
					Quantity = detailDto.Quantity,
					Notes = detailDto.Notes
				};
				await _exportDetailRepository.InsertAsync(detail);
			}

			await CurrentUnitOfWork.SaveChangesAsync();

			return await GetExportSlipById(exportSlipId);
		}

		/// <summary>
		/// Cập nhật phiếu xuất kho (chỉ khi Status = Draft)
		/// </summary>
		public async Task<ExportSlipDto> UpdateExportSlip(UpdateExportSlipDto input)
		{
			var exportSlip = await _exportSlipRepository.GetAll()
				.Include(x => x.Details)
				.FirstOrDefaultAsync(x => x.Id == input.Id);

			if (exportSlip == null)
				throw new UserFriendlyException("Không tìm thấy phiếu xuất kho");

			if (exportSlip.Status != ExportStatus.Draft)
				throw new UserFriendlyException($"Không thể sửa phiếu xuất ở trạng thái {GetStatusName(exportSlip.Status)}. Chỉ có thể sửa phiếu ở trạng thái Nháp.");

			// Validate
			if (input.Details == null || !input.Details.Any())
				throw new UserFriendlyException("Phiếu xuất phải có ít nhất 1 sản phẩm");

			// Validate sản phẩm
			var productIds = input.Details.Select(d => d.ProductId).Distinct().ToList();
			var products = await _productRepository.GetAll()
				.Where(p => productIds.Contains(p.Id))
				.ToListAsync();

			if (products.Count != productIds.Count)
				throw new UserFriendlyException("Có sản phẩm không tồn tại trong hệ thống");

			// Cập nhật thông tin chung
			exportSlip.ExportDate = input.ExportDate;
			exportSlip.OrderId = input.OrderId;
			exportSlip.SupplierId = input.SupplierId;
			exportSlip.Type = input.Type;
			exportSlip.Reason = input.Reason;
			exportSlip.Notes = input.Notes;

			// Xóa các detail cũ không còn trong danh sách mới
			if (input.Details != null && input.Details.Any())
			{
				var existingDetailIds = input.Details.Where(d => d.Id.HasValue).Select(d => d.Id.Value).ToList();
				var detailsToDelete = exportSlip.Details.Where(d => !existingDetailIds.Contains(d.Id)).ToList();
				foreach (var detail in detailsToDelete)
				{
					await _exportDetailRepository.DeleteAsync(detail);
				}

				// Cập nhật hoặc tạo mới detail
				foreach (var detailDto in input.Details)
				{
					if (detailDto.Id.HasValue)
					{
						// Cập nhật
						var detail = await _exportDetailRepository.GetAsync(detailDto.Id.Value);
						detail.ProductId = detailDto.ProductId;
						detail.Quantity = detailDto.Quantity;
						detail.Notes = detailDto.Notes;
						await _exportDetailRepository.UpdateAsync(detail);
					}
					else
					{
						// Tạo mới
						var detail = new ExportDetail
						{
							ExportSlipId = exportSlip.Id,
							ProductId = detailDto.ProductId,
							Quantity = detailDto.Quantity,
							Notes = detailDto.Notes
						};
						await _exportDetailRepository.InsertAsync(detail);
					}
				}
			}

			await _exportSlipRepository.UpdateAsync(exportSlip);
			await CurrentUnitOfWork.SaveChangesAsync();

			return await GetExportSlipById(exportSlip.Id);
		}

		/// <summary>
		/// Xác nhận và hoàn thành phiếu xuất kho
		/// </summary>
		[HttpPost]
		public async Task CompleteExportSlip(int exportSlipId)
		{
			// Lấy ExportSlip
			var exportSlip = await _exportSlipRepository.GetAll()
				.Include(x => x.Details)
				.FirstOrDefaultAsync(x => x.Id == exportSlipId);

			if (exportSlip == null)
				throw new UserFriendlyException("Không tìm thấy phiếu xuất kho");

			if (exportSlip.Status != ExportStatus.Draft)
				throw new UserFriendlyException($"Không thể hoàn thành phiếu xuất ở trạng thái {GetStatusName(exportSlip.Status)}. Chỉ có thể hoàn thành phiếu ở trạng thái Nháp.");

			// Lấy tất cả chi tiết
			var details = exportSlip.Details.ToList();
			if (!details.Any())
				throw new UserFriendlyException("Phiếu xuất không có sản phẩm nào");

			// Xử lý từng sản phẩm
			foreach (var detail in details)
			{
				// Lấy Inventory
				var inventory = await _inventoryRepository.FirstOrDefaultAsync(x => x.ProductId == detail.ProductId);

				if (inventory == null)
					throw new UserFriendlyException($"Sản phẩm {detail.Product?.Name ?? detail.ProductId.ToString()} không có trong kho");

				// Kiểm tra số lượng tồn kho
				if (inventory.Quantity < detail.Quantity)
					throw new UserFriendlyException($"Sản phẩm {detail.Product?.Name ?? detail.ProductId.ToString()} không đủ số lượng. Tồn kho: {inventory.Quantity}, Yêu cầu: {detail.Quantity}");

				int quantityBefore = inventory.Quantity;

				// Giảm số lượng
				inventory.Quantity -= detail.Quantity;
				inventory.LastUpdated = DateTime.Now;
				await _inventoryRepository.UpdateAsync(inventory);

				int quantityAfter = inventory.Quantity;

				// Tạo InventoryTransaction
				var transaction = new InventoryTransaction
				{
					Type = TransactionType.Export,
					ProductId = detail.ProductId,
					Quantity = detail.Quantity,
					QuantityBefore = quantityBefore,
					QuantityAfter = quantityAfter,
					Reason = GetExportReason(exportSlip.Type),
					Notes = $"Xuất từ phiếu: {exportSlip.ExportCode}",
					UserId = AbpSession.UserId,
					TransactionDate = exportSlip.ExportDate,
					ReferenceId = exportSlipId,
					ReferenceType = "ExportSlip"
				};
				await _transactionRepository.InsertAsync(transaction);
			}

			// Cập nhật trạng thái
			exportSlip.Status = ExportStatus.Completed;
			await _exportSlipRepository.UpdateAsync(exportSlip);

			await CurrentUnitOfWork.SaveChangesAsync();
		}

		/// <summary>
		/// Hủy phiếu xuất kho (chỉ khi Status = Draft)
		/// </summary>
		[HttpPost]
		public async Task CancelExportSlip(int exportSlipId)
		{
			var exportSlip = await _exportSlipRepository.GetAsync(exportSlipId);

			if (exportSlip.Status != ExportStatus.Draft)
				throw new UserFriendlyException($"Chỉ có thể hủy phiếu xuất ở trạng thái Nháp. Trạng thái hiện tại: {GetStatusName(exportSlip.Status)}");

			exportSlip.Status = ExportStatus.Cancelled;
			await _exportSlipRepository.UpdateAsync(exportSlip);
			await CurrentUnitOfWork.SaveChangesAsync();
		}

		/// <summary>
		/// Lấy danh sách phiếu xuất kho có phân trang và lọc
		/// </summary>
		public async Task<PagedResultDto<ExportSlipDto>> GetAllExportSlips(GetAllExportSlipsInput input)
		{
			var query = _exportSlipRepository.GetAll()
				.Include(x => x.Supplier)
				.Include(x => x.Order)
				.Include(x => x.CreatorUser)
				.Include(x => x.Details)
					.ThenInclude(d => d.Product)
				.AsQueryable();

			// Filter
			if (!string.IsNullOrWhiteSpace(input.ExportCode))
				query = query.Where(x => x.ExportCode.Contains(input.ExportCode));

			if (input.SupplierId.HasValue)
				query = query.Where(x => x.SupplierId == input.SupplierId.Value);

			if (input.OrderId.HasValue)
				query = query.Where(x => x.OrderId == input.OrderId.Value);

			if (input.Type.HasValue)
				query = query.Where(x => x.Type == input.Type.Value);

			if (input.Status.HasValue)
				query = query.Where(x => x.Status == input.Status.Value);

			if (input.FromDate.HasValue)
				query = query.Where(x => x.ExportDate >= input.FromDate.Value);

			if (input.ToDate.HasValue)
				query = query.Where(x => x.ExportDate <= input.ToDate.Value);

			if (!string.IsNullOrWhiteSpace(input.Keyword))
			{
				var keyword = input.Keyword.ToLower();
				query = query.Where(x =>
					x.ExportCode.ToLower().Contains(keyword) ||
					(x.Notes != null && x.Notes.ToLower().Contains(keyword)) ||
					(x.Reason != null && x.Reason.ToLower().Contains(keyword)) ||
					(x.Supplier != null && x.Supplier.Name.ToLower().Contains(keyword)) ||
					(x.Order != null && x.Order.OrderCode.ToLower().Contains(keyword))
				);
			}

			var totalCount = await query.CountAsync();

			// Apply sorting - Mặc định sắp xếp theo ngày xuất mới nhất
			if (string.IsNullOrWhiteSpace(input.Sorting))
				input.Sorting = "ExportDate DESC";

			var exportSlips = await query.OrderBy(input.Sorting).PageBy(input).ToListAsync();

			var items = exportSlips.Select(x => MapToDto(x)).ToList();

			return new PagedResultDto<ExportSlipDto>(totalCount, items);
		}

		/// <summary>
		/// Lấy chi tiết phiếu xuất kho theo ID
		/// </summary>
		public async Task<ExportSlipDto> GetExportSlipById(int id)
		{
			var exportSlip = await _exportSlipRepository.GetAll()
				.Include(x => x.Supplier)
				.Include(x => x.Order)
				.Include(x => x.CreatorUser)
				.Include(x => x.Details)
					.ThenInclude(d => d.Product)
				.FirstOrDefaultAsync(x => x.Id == id);

			if (exportSlip == null)
				throw new UserFriendlyException("Không tìm thấy phiếu xuất kho");

			return MapToDto(exportSlip);
		}

		/// <summary>
		/// Xóa phiếu xuất kho (chỉ khi Status = Draft)
		/// </summary>
		[HttpPost]
		public async Task DeleteExportSlip(int id)
		{
			var exportSlip = await _exportSlipRepository.GetAsync(id);

			if (exportSlip.Status != ExportStatus.Draft)
				throw new UserFriendlyException($"Chỉ có thể xóa phiếu xuất ở trạng thái Nháp. Trạng thái hiện tại: {GetStatusName(exportSlip.Status)}");

			// Xóa các detail trước
			var details = await _exportDetailRepository.GetAll()
				.Where(x => x.ExportSlipId == id)
				.ToListAsync();

			foreach (var detail in details)
			{
				await _exportDetailRepository.DeleteAsync(detail);
			}

			await _exportSlipRepository.DeleteAsync(exportSlip);
			await CurrentUnitOfWork.SaveChangesAsync();
		}

		#region Private Methods

		/// <summary>
		/// Tạo mã phiếu xuất tự động: PX{yyyyMMdd}{xxxx}
		/// </summary>
		private async Task<string> GenerateExportCodeAsync()
		{
			var today = DateTime.Now;
			var prefix = $"PX{today:yyyyMMdd}";

			var lastCode = await _exportSlipRepository.GetAll()
				.Where(x => x.ExportCode.StartsWith(prefix))
				.OrderByDescending(x => x.ExportCode)
				.FirstOrDefaultAsync();

			int sequence = 1;
			if (lastCode != null)
			{
				var lastSequence = lastCode.ExportCode.Substring(prefix.Length);
				if (int.TryParse(lastSequence, out int lastNum))
					sequence = lastNum + 1;
			}

			return $"{prefix}{sequence:D4}"; // VD: PX202401010001
		}

		/// <summary>
		/// Lấy lý do xuất kho theo loại
		/// </summary>
		private string GetExportReason(ExportType type)
		{
			return type switch
			{
				ExportType.Order => "Xuất cho đơn hàng",
				ExportType.SupplierReturn => "Xuất trả nhà cung cấp",
				ExportType.Damage => "Xuất hỏng hóc",
				ExportType.Adjustment => "Xuất điều chỉnh",
				ExportType.Transfer => "Xuất chuyển kho",
				_ => "Xuất kho"
			};
		}

		/// <summary>
		/// Map ExportSlip entity sang DTO
		/// </summary>
		private ExportSlipDto MapToDto(ExportSlip exportSlip)
		{
			return new ExportSlipDto
			{
				Id = exportSlip.Id,
				ExportCode = exportSlip.ExportCode,
				ExportDate = exportSlip.ExportDate,
				OrderId = exportSlip.OrderId,
				OrderCode = exportSlip.Order?.OrderCode,
				SupplierId = exportSlip.SupplierId,
				SupplierName = exportSlip.Supplier?.Name,
				Type = exportSlip.Type,
				TypeName = GetTypeName(exportSlip.Type),
				Status = exportSlip.Status,
				StatusName = GetStatusName(exportSlip.Status),
				Reason = exportSlip.Reason,
				Notes = exportSlip.Notes,
				CreatorUserId = exportSlip.CreatorUserId,
				CreatorUserName = exportSlip.CreatorUser?.UserName ?? exportSlip.CreatorUser?.Name,
				CreationTime = exportSlip.CreationTime,
				LastModificationTime = exportSlip.LastModificationTime,
				Details = exportSlip.Details?.Select(d => new ExportDetailDto
				{
					Id = d.Id,
					ExportSlipId = d.ExportSlipId,
					ProductId = d.ProductId,
					ProductName = d.Product?.Name,
					Quantity = d.Quantity,
					Notes = d.Notes
				}).ToList() ?? new List<ExportDetailDto>()
			};
		}

		/// <summary>
		/// Lấy tên loại xuất kho
		/// </summary>
		private string GetTypeName(ExportType type)
		{
			return type switch
			{
				ExportType.Order => "Xuất cho đơn hàng",
				ExportType.SupplierReturn => "Xuất trả nhà cung cấp",
				ExportType.Damage => "Xuất hỏng hóc",
				ExportType.Adjustment => "Xuất điều chỉnh",
				ExportType.Transfer => "Xuất chuyển kho",
				_ => "Không xác định"
			};
		}

		/// <summary>
		/// Lấy tên trạng thái phiếu xuất
		/// </summary>
		private string GetStatusName(ExportStatus status)
		{
			return status switch
			{
				ExportStatus.Draft => "Nháp",
				ExportStatus.Completed => "Đã hoàn thành",
				ExportStatus.Cancelled => "Đã hủy",
				_ => "Không xác định"
			};
		}

		#endregion
	}
}

