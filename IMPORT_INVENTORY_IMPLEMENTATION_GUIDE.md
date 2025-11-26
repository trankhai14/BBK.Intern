# Hướng Dẫn Triển Khai Chức Năng Nhập Kho

## 📋 Tổng Quan

Chức năng nhập kho sẽ sử dụng **ImportSlip** và **ImportDetail** để quản lý phiếu nhập kho, thay vì nhập trực tiếp như hiện tại.

## 🎯 Mục Tiêu

1. Tạo phiếu nhập kho (ImportSlip) với nhiều sản phẩm
2. Quản lý trạng thái: Draft → Completed → Cancelled
3. Tự động tạo InventoryTransaction khi hoàn thành
4. Cập nhật Inventory.Quantity cho từng sản phẩm
5. Hỗ trợ nhiều loại nhập: Supplier, Return, Adjustment, Transfer

---

## 📝 Các Bước Triển Khai

### Bước 1: Tạo DTOs cho ImportSlip

#### 1.1. CreateImportSlipDto.cs
```csharp
// File: MyProject.Application/ImportSlips/Dto/CreateImportSlipDto.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MyProject.ImportSlips;

namespace MyProject.ImportSlips.Dto
{
    public class CreateImportSlipDto
    {
        [Required]
        public DateTime ImportDate { get; set; }

        public int? SupplierId { get; set; }

        [Required]
        public ImportType Type { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Phải có ít nhất 1 sản phẩm")]
        public List<CreateImportDetailDto> Details { get; set; }
    }

    public class CreateImportDetailDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá nhập phải lớn hơn 0")]
        public decimal UnitPrice { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }
    }
}
```

#### 1.2. UpdateImportSlipDto.cs
```csharp
// File: MyProject.Application/ImportSlips/Dto/UpdateImportSlipDto.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MyProject.ImportSlips;

namespace MyProject.ImportSlips.Dto
{
    public class UpdateImportSlipDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public DateTime ImportDate { get; set; }

        public int? SupplierId { get; set; }

        [Required]
        public ImportType Type { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; }

        public List<UpdateImportDetailDto> Details { get; set; }
    }

    public class UpdateImportDetailDto
    {
        public int? Id { get; set; } // null nếu là mới

        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }
    }
}
```

#### 1.3. ImportSlipDto.cs
```csharp
// File: MyProject.Application/ImportSlips/Dto/ImportSlipDto.cs
using System;
using System.Collections.Generic;
using MyProject.ImportSlips;

namespace MyProject.ImportSlips.Dto
{
    public class ImportSlipDto
    {
        public int Id { get; set; }
        public string ImportCode { get; set; }
        public DateTime ImportDate { get; set; }
        public int? SupplierId { get; set; }
        public string SupplierName { get; set; }
        public ImportType Type { get; set; }
        public string TypeName { get; set; }
        public ImportStatus Status { get; set; }
        public string StatusName { get; set; }
        public decimal TotalAmount { get; set; }
        public string Notes { get; set; }
        public long? CreatorUserId { get; set; }
        public string CreatorUserName { get; set; }
        public DateTime CreationTime { get; set; }
        public List<ImportDetailDto> Details { get; set; }
    }

    public class ImportDetailDto
    {
        public int Id { get; set; }
        public int ImportSlipId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }
        public string Notes { get; set; }
    }
}
```

#### 1.4. GetAllImportSlipsInput.cs
```csharp
// File: MyProject.Application/ImportSlips/Dto/GetAllImportSlipsInput.cs
using System;
using Abp.Application.Services.Dto;
using MyProject.ImportSlips;

namespace MyProject.ImportSlips.Dto
{
    public class GetAllImportSlipsInput : PagedAndSortedResultRequestDto
    {
        public string ImportCode { get; set; }
        public int? SupplierId { get; set; }
        public ImportType? Type { get; set; }
        public ImportStatus? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string Keyword { get; set; }
    }
}
```

---

### Bước 2: Tạo Interface và Service

#### 2.1. IImportSlipAppService.cs
```csharp
// File: MyProject.Application/ImportSlips/IImportSlipAppService.cs
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using MyProject.ImportSlips.Dto;

namespace MyProject.ImportSlips
{
    public interface IImportSlipAppService : IApplicationService
    {
        /// <summary>
        /// Tạo phiếu nhập kho mới (trạng thái Draft)
        /// </summary>
        Task<ImportSlipDto> CreateImportSlip(CreateImportSlipDto input);

        /// <summary>
        /// Cập nhật phiếu nhập kho (chỉ khi Status = Draft)
        /// </summary>
        Task<ImportSlipDto> UpdateImportSlip(UpdateImportSlipDto input);

        /// <summary>
        /// Xác nhận và hoàn thành phiếu nhập kho
        /// - Cập nhật Inventory.Quantity
        /// - Tạo InventoryTransaction cho từng sản phẩm
        /// - Chuyển Status = Completed
        /// </summary>
        Task CompleteImportSlip(int importSlipId);

        /// <summary>
        /// Hủy phiếu nhập kho (chỉ khi Status = Draft)
        /// </summary>
        Task CancelImportSlip(int importSlipId);

        /// <summary>
        /// Lấy danh sách phiếu nhập kho có phân trang
        /// </summary>
        Task<PagedResultDto<ImportSlipDto>> GetAllImportSlips(GetAllImportSlipsInput input);

        /// <summary>
        /// Lấy chi tiết phiếu nhập kho theo ID
        /// </summary>
        Task<ImportSlipDto> GetImportSlipById(int id);

        /// <summary>
        /// Xóa phiếu nhập kho (chỉ khi Status = Draft)
        /// </summary>
        Task DeleteImportSlip(int id);
    }
}
```

---

### Bước 3: Implement Service

#### 3.1. ImportSlipAppService.cs - Cấu trúc chính

```csharp
// File: MyProject.Application/ImportSlips/ImportSlipAppService.cs
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
using MyProject.ImportSlips.Dto;
using MyProject.Inventories;
using MyProject.InventoryTransactions;
using MyProject.Products;
using MyProject.Suppliers;

namespace MyProject.ImportSlips
{
    public class ImportSlipAppService : ApplicationService, IImportSlipAppService
    {
        private readonly IRepository<ImportSlip> _importSlipRepository;
        private readonly IRepository<ImportDetail> _importDetailRepository;
        private readonly IRepository<Inventory> _inventoryRepository;
        private readonly IRepository<InventoryTransaction> _transactionRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<Supplier> _supplierRepository;
        private readonly IInventoryAppService _inventoryAppService;

        public ImportSlipAppService(
            IRepository<ImportSlip> importSlipRepository,
            IRepository<ImportDetail> importDetailRepository,
            IRepository<Inventory> inventoryRepository,
            IRepository<InventoryTransaction> transactionRepository,
            IRepository<Product> productRepository,
            IRepository<Supplier> supplierRepository,
            IInventoryAppService inventoryAppService
        )
        {
            _importSlipRepository = importSlipRepository;
            _importDetailRepository = importDetailRepository;
            _inventoryRepository = inventoryRepository;
            _transactionRepository = transactionRepository;
            _productRepository = productRepository;
            _supplierRepository = supplierRepository;
            _inventoryAppService = inventoryAppService;
        }

        // TODO: Implement các methods
    }
}
```

---

### Bước 4: Implement từng Method

#### 4.1. CreateImportSlip - Tạo phiếu nhập

**Logic:**
1. Validate input
2. Tạo mã phiếu nhập tự động (ImportCode)
3. Tính TotalAmount từ Details
4. Tạo ImportSlip với Status = Draft
5. Tạo các ImportDetail
6. Lưu vào database
7. Return DTO

**Code:**
```csharp
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
```

#### 4.2. CompleteImportSlip - Hoàn thành phiếu nhập

**Logic:**
1. Kiểm tra ImportSlip tồn tại và Status = Draft
2. Lấy tất cả ImportDetail
3. Với mỗi sản phẩm:
   - Lấy Inventory hiện tại (hoặc tạo mới nếu chưa có)
   - Lưu QuantityBefore
   - Tăng Inventory.Quantity
   - Lưu QuantityAfter
   - Tạo InventoryTransaction với ReferenceId và ReferenceType
4. Cập nhật ImportSlip.Status = Completed
5. Save changes

**Code:**
```csharp
public async Task CompleteImportSlip(int importSlipId)
{
    // Lấy ImportSlip
    var importSlip = await _importSlipRepository.GetAll()
        .Include(x => x.Details)
        .FirstOrDefaultAsync(x => x.Id == importSlipId);

    if (importSlip == null)
        throw new UserFriendlyException("Không tìm thấy phiếu nhập kho");

    if (importSlip.Status != ImportStatus.Draft)
        throw new UserFriendlyException($"Không thể hoàn thành phiếu nhập ở trạng thái {importSlip.Status}");

    // Lấy tất cả chi tiết
    var details = importSlip.Details.ToList();

    // Xử lý từng sản phẩm
    foreach (var detail in details)
    {
        // Lấy hoặc tạo Inventory
        var inventory = await _inventoryRepository.FirstOrDefaultAsync(x => x.ProductId == detail.ProductId);
        
        int quantityBefore = 0;
        if (inventory == null)
        {
            // Tạo mới Inventory
            inventory = new Inventory
            {
                ProductId = detail.ProductId,
                Quantity = 0,
                ReservedQuantity = 0
            };
            await _inventoryRepository.InsertAsync(inventory);
        }
        else
        {
            quantityBefore = inventory.Quantity;
        }

        // Tăng số lượng
        inventory.Quantity += detail.Quantity;
        inventory.LastUpdated = DateTime.Now;
        await _inventoryRepository.UpdateAsync(inventory);

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
```

#### 4.3. Các methods còn lại

```csharp
public async Task<ImportSlipDto> UpdateImportSlip(UpdateImportSlipDto input)
{
    var importSlip = await _importSlipRepository.GetAll()
        .Include(x => x.Details)
        .FirstOrDefaultAsync(x => x.Id == input.Id);

    if (importSlip == null)
        throw new UserFriendlyException("Không tìm thấy phiếu nhập kho");

    if (importSlip.Status != ImportStatus.Draft)
        throw new UserFriendlyException("Chỉ có thể sửa phiếu nhập ở trạng thái Nháp");

    // Cập nhật thông tin chung
    importSlip.ImportDate = input.ImportDate;
    importSlip.SupplierId = input.SupplierId;
    importSlip.Type = input.Type;
    importSlip.Notes = input.Notes;

    // Xóa các detail cũ
    var existingDetailIds = input.Details?.Where(d => d.Id.HasValue).Select(d => d.Id.Value).ToList() ?? new List<int>();
    var detailsToDelete = importSlip.Details.Where(d => !existingDetailIds.Contains(d.Id)).ToList();
    foreach (var detail in detailsToDelete)
    {
        await _importDetailRepository.DeleteAsync(detail);
    }

    // Cập nhật hoặc tạo mới detail
    if (input.Details != null)
    {
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

public async Task CancelImportSlip(int importSlipId)
{
    var importSlip = await _importSlipRepository.GetAsync(importSlipId);

    if (importSlip.Status != ImportStatus.Draft)
        throw new UserFriendlyException("Chỉ có thể hủy phiếu nhập ở trạng thái Nháp");

    importSlip.Status = ImportStatus.Cancelled;
    await _importSlipRepository.UpdateAsync(importSlip);
    await CurrentUnitOfWork.SaveChangesAsync();
}

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

    query = ApplySorting(query, input);
    query = query.PageBy(input);

    var items = await query.ToListAsync();

    var dtos = items.Select(MapToDto).ToList();

    return new PagedResultDto<ImportSlipDto>(totalCount, dtos);
}

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

public async Task DeleteImportSlip(int id)
{
    var importSlip = await _importSlipRepository.GetAsync(id);

    if (importSlip.Status != ImportStatus.Draft)
        throw new UserFriendlyException("Chỉ có thể xóa phiếu nhập ở trạng thái Nháp");

    await _importSlipRepository.DeleteAsync(id);
    await CurrentUnitOfWork.SaveChangesAsync();
}

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
```

---

### Bước 5: Tạo Controller (nếu cần)

```csharp
// File: MyProject.Web.Mvc/Controllers/ImportSlipController.cs
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyProject.Authorization;
using MyProject.ImportSlips;
using MyProject.ImportSlips.Dto;

namespace MyProject.Web.Mvc.Controllers
{
    [AbpMvcAuthorize(PermissionNames.Pages_Inventories)]
    public class ImportSlipController : MyProjectControllerBase
    {
        private readonly IImportSlipAppService _importSlipAppService;

        public ImportSlipController(IImportSlipAppService importSlipAppService)
        {
            _importSlipAppService = importSlipAppService;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> Create([FromBody] CreateImportSlipDto input)
        {
            var result = await _importSlipAppService.CreateImportSlip(input);
            return Json(result);
        }

        [HttpPost]
        public async Task<JsonResult> Complete(int id)
        {
            await _importSlipAppService.CompleteImportSlip(id);
            return Json(new { success = true });
        }
    }
}
```

---

## 🔄 Luồng Xử Lý

```
1. User tạo phiếu nhập
   ↓
2. CreateImportSlip() → Status = Draft
   ↓
3. User có thể sửa/xóa (chỉ khi Draft)
   ↓
4. User xác nhận → CompleteImportSlip()
   ↓
5. Với mỗi sản phẩm:
   - Tăng Inventory.Quantity
   - Tạo InventoryTransaction
   ↓
6. Status = Completed
   ↓
7. Không thể sửa/xóa nữa
```

---

## ✅ Checklist Triển Khai

- [ ] Tạo thư mục `MyProject.Application/ImportSlips/Dto/`
- [ ] Tạo các DTOs (Create, Update, Get, List)
- [ ] Tạo `IImportSlipAppService` interface
- [ ] Implement `ImportSlipAppService`
- [ ] Implement `CreateImportSlip`
- [ ] Implement `UpdateImportSlip`
- [ ] Implement `CompleteImportSlip` (quan trọng nhất)
- [ ] Implement `CancelImportSlip`
- [ ] Implement `GetAllImportSlips`
- [ ] Implement `GetImportSlipById`
- [ ] Implement `DeleteImportSlip`
- [ ] Tạo Controller (nếu cần)
- [ ] Test các trường hợp:
  - [ ] Tạo phiếu nhập thành công
  - [ ] Sửa phiếu nhập (Draft)
  - [ ] Hoàn thành phiếu nhập → Cập nhật Inventory
  - [ ] Hoàn thành phiếu nhập → Tạo InventoryTransaction
  - [ ] Không thể sửa khi đã Completed
  - [ ] Hủy phiếu nhập

---

## 🎯 Lưu Ý Quan Trọng

1. **Mã phiếu nhập tự động**: Format `PN{yyyyMMdd}{xxxx}` (VD: PN202401010001)
2. **Validation**: Luôn validate input trước khi xử lý
3. **Transaction**: Sử dụng UnitOfWork để đảm bảo tính nhất quán
4. **Inventory**: Tự động tạo Inventory nếu chưa có
5. **Reference**: InventoryTransaction phải có ReferenceId và ReferenceType
6. **Status**: Chỉ cho phép sửa/xóa khi Status = Draft

---

## 📚 Tài Liệu Tham Khảo

- Xem `WAREHOUSE_MANAGEMENT_SOLUTION.md` để hiểu rõ hơn về luồng xử lý
- Xem `InventoryTransactionAppService.cs` để tham khảo cách xử lý cũ


