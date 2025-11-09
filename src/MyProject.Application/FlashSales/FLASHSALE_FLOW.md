# 📋 TÀI LIỆU CHI TIẾT LUỒNG CHỨC NĂNG FLASHSALE

## 🎯 TỔNG QUAN

Hệ thống FlashSale cho phép admin quản lý các chương trình khuyến mãi giới hạn thời gian, với khả năng:
- Tạo/sửa/xóa FlashSale events
- Quản lý sản phẩm trong FlashSale (thêm/sửa/xóa)
- Tự động khóa số lượng trong Inventory
- Tự động hoàn trả số lượng khi FlashSale kết thúc
- Giới hạn số lượng mua mỗi tài khoản

---

## 🏗️ KIẾN TRÚC HỆ THỐNG

### **1. Layers**

```
┌─────────────────────────────────────────┐
│   Presentation Layer (Web.Mvc)          │
│   - Controllers                         │
│   - Views (Razor)                       │
│   - JavaScript (jQuery)                 │
└─────────────────────────────────────────┘
              ↓
┌─────────────────────────────────────────┐
│   Application Layer                     │
│   - AppServices (Business Logic)        │
│   - DTOs (Data Transfer Objects)        │
└─────────────────────────────────────────┘
              ↓
┌─────────────────────────────────────────┐
│   Domain Layer (Core)                   │
│   - Entities (FlashSale, FlashSaleProduct)│
│   - Enums (FlashSaleStatus)            │
└─────────────────────────────────────────┘
              ↓
┌─────────────────────────────────────────┐
│   Data Access Layer (EntityFrameworkCore)│
│   - Repositories                        │
│   - DbContext                           │
└─────────────────────────────────────────┘
              ↓
┌─────────────────────────────────────────┐
│   Database (SQL Server)                 │
│   - AppFlashSales                       │
│   - AppFlashSaleProducts                │
│   - Inventories (tích hợp)              │
└─────────────────────────────────────────┘
```

### **2. Database Schema**

#### **Bảng AppFlashSales**
```sql
- Id (int, PK)
- Name (nvarchar(256))
- Description (nvarchar(2000))
- StartTime (datetime)
- EndTime (datetime)
- Status (tinyint) -- 0: NotStarted, 1: Ongoing, 2: Ended, 3: Cancelled
- IsActive (bit)
- IsHidden (bit)
- CreationTime, LastModificationTime, DeletionTime
- CreatorUserId, LastModifierUserId, DeleterUserId
```

#### **Bảng AppFlashSaleProducts**
```sql
- Id (int, PK)
- FlashSaleId (int, FK -> AppFlashSales)
- ProductId (int, FK -> Products)
- FlashSalePrice (decimal)
- FlashSaleQuantity (int)
- SoldQuantity (int)
- MaxQuantityPerUser (int, nullable)
- ReservedQuantity (int)
- IsReturnedToInventory (bit)
- CreationTime, LastModificationTime, DeletionTime
- CreatorUserId, LastModifierUserId, DeleterUserId
```

---

## 📊 LUỒNG CHỨC NĂNG CHI TIẾT

## 🎪 1. QUẢN LÝ FLASHSALE EVENT

### **1.1. Danh sách FlashSale (Index)**

#### **Frontend Flow:**
```
1. User truy cập: /FlashSales/Index
   ↓
2. Controller: FlashSalesController.Index()
   - Gọi: _flashSaleAppService.GetAll(input)
   - Trả về: View với model PagedResultDto<FlashSaleDto>
   ↓
3. View: Index.cshtml
   - Render DataTable với server-side processing
   - Load JavaScript: Index.js
   ↓
4. JavaScript: Index.js
   - Khởi tạo DataTable với AJAX
   - Gọi: abp.services.app.flashSale.getAll()
   - Hiển thị danh sách FlashSale
```

#### **Backend Flow:**
```
1. FlashSaleAppService.GetAll(GetAllFlashSalesInput input)
   ↓
2. Query với filters:
   - Keyword (tìm trong Name, Description)
   - Status (NotStarted, Ongoing, Ended, Cancelled)
   - IsActive (true/false)
   - IsHidden (true/false)
   ↓
3. Include FlashSaleProducts để tính:
   - TotalProducts = FlashSaleProducts.Count
   - TotalSold = FlashSaleProducts.Sum(p => p.SoldQuantity)
   ↓
4. Tính StatusText sau khi query (trong memory)
   - Gọi: GetStatusText(Status) - static method
   ↓
5. Trả về: PagedResultDto<FlashSaleDto>
```

#### **Code Reference:**
- **Controller:** `FlashSalesController.cs` - Line 21-30
- **Service:** `FlashSaleAppService.cs` - Line 79-149
- **View:** `Index.cshtml`
- **JavaScript:** `Index.js` - Line 8-107

---

### **1.2. Tạo mới FlashSale (Create)**

#### **Frontend Flow:**
```
1. User click nút "Tạo mới" trong Index.cshtml
   ↓
2. Mở modal: #FlashSaleCreateModal
   - Modal được render sẵn trong Index.cshtml
   - Model: CreateFlashSaleDto (rỗng)
   ↓
3. User điền form:
   - Name (required)
   - Description (optional)
   - StartTime (required, datetime-local)
   - EndTime (required, datetime-local)
   - IsActive (checkbox, default: true)
   - IsHidden (checkbox, default: false)
   ↓
4. User click "Lưu"
   ↓
5. JavaScript: Index.js - Line 117-143
   - Validate form
   - Convert datetime-local → ISO string
   - Gọi: abp.services.app.flashSale.create(flashSale)
   ↓
6. Nếu thành công:
   - Đóng modal
   - Reset form
   - Reload DataTable
   - Hiển thị thông báo thành công
```

#### **Backend Flow:**
```
1. FlashSaleAppService.Create(CreateFlashSaleDto input)
   ↓
2. Validate:
   - StartTime < EndTime
   - StartTime >= DateTime.Now (không được trong quá khứ)
   ↓
3. Tạo entity mới:
   - Name = input.Name
   - Description = input.Description
   - StartTime = input.StartTime
   - EndTime = input.EndTime
   - IsActive = input.IsActive
   - IsHidden = input.IsHidden
   - Status = FlashSaleStatus.NotStarted (mặc định)
   ↓
4. Lưu vào database:
   - _flashSaleRepository.InsertAsync(flashSale)
   - CurrentUnitOfWork.SaveChangesAsync()
   ↓
5. Trả về: FlashSaleDto (gọi GetById để lấy đầy đủ thông tin)
```

#### **Validation Rules:**
- ✅ Name: Required, MaxLength(256)
- ✅ StartTime: Required, Must be in future
- ✅ EndTime: Required, Must be after StartTime
- ✅ Description: Optional, MaxLength(2000)

#### **Code Reference:**
- **Controller:** `FlashSalesController.cs` - Line 37-45
- **Service:** `FlashSaleAppService.cs` - Line 215-247
- **View:** `_CreateModal.cshtml`
- **JavaScript:** `Index.js` - Line 117-143

---

### **1.3. Sửa FlashSale (Update)**

#### **Frontend Flow:**
```
1. User click nút "Sửa" trong DataTable
   ↓
2. JavaScript: Index.js - Line 171-185
   - Gọi AJAX: /FlashSales/EditModal?flashSaleId={id}
   - Load HTML vào modal: #FlashSaleEditModal
   ↓
3. Controller: FlashSalesController.EditModal(flashSaleId)
   - Gọi: _flashSaleAppService.GetById(flashSaleId)
   - Map sang: UpdateFlashSaleDto
   - Trả về: PartialView("_EditModal", updateDto)
   ↓
4. User sửa thông tin và click "Lưu"
   ↓
5. JavaScript: _EditModal.js - Line 7-30
   - Validate form
   - Convert datetime-local → ISO string
   - Gọi: abp.services.app.flashSale.update(flashSale)
   ↓
6. Nếu thành công:
   - Đóng modal
   - Trigger event: 'flashSale.edited'
   - Reload DataTable (từ Index.js listener)
```

#### **Backend Flow:**
```
1. FlashSaleAppService.Update(UpdateFlashSaleDto input)
   ↓
2. Validate:
   - StartTime < EndTime
   - Status != Ongoing (không cho phép sửa khi đang diễn ra)
   ↓
3. Cập nhật entity:
   - Name = input.Name
   - Description = input.Description
   - StartTime = input.StartTime
   - EndTime = input.EndTime
   - IsActive = input.IsActive
   - IsHidden = input.IsHidden
   - Status = flashSale.CalculatedStatus (tính lại trạng thái)
   ↓
4. Lưu vào database:
   - _flashSaleRepository.UpdateAsync(flashSale)
   - CurrentUnitOfWork.SaveChangesAsync()
   ↓
5. Trả về: FlashSaleDto
```

#### **Business Rules:**
- ❌ **Không cho phép sửa khi FlashSale đang diễn ra (Ongoing)**
- ✅ Cho phép sửa khi: NotStarted hoặc Ended
- ✅ Trạng thái tự động tính lại dựa trên StartTime/EndTime

#### **Code Reference:**
- **Controller:** `FlashSalesController.cs` - Line 47-62
- **Service:** `FlashSaleAppService.cs` - Line 256-291
- **View:** `_EditModal.cshtml`
- **JavaScript:** `_EditModal.js`

---

### **1.4. Xóa FlashSale (Delete)**

#### **Frontend Flow:**
```
1. User click nút "Xóa" trong DataTable
   ↓
2. JavaScript: Index.js - Line 145-169
   - Hiển thị confirm dialog
   - Nếu confirm: Gọi abp.services.app.flashSale.delete({id: flashSaleId})
   ↓
3. Nếu thành công:
   - Hiển thị thông báo
   - Reload DataTable
```

#### **Backend Flow:**
```
1. FlashSaleAppService.Delete(int id)
   ↓
2. Lấy FlashSale kèm FlashSaleProducts:
   - Include(fs => fs.FlashSaleProducts)
   ↓
3. Validate:
   - Status != Ongoing (không cho phép xóa khi đang diễn ra)
   ↓
4. Hoàn trả số lượng về Inventory:
   - Với mỗi FlashSaleProduct:
     - Nếu !IsReturnedToInventory && ReservedQuantity > 0:
       - Gọi: ReturnProductQuantityToInventory(product)
       - Giảm ReservedQuantity trong Inventory
   ↓
5. Xóa FlashSale:
   - _flashSaleRepository.DeleteAsync(flashSale)
   - Cascade delete sẽ xóa các FlashSaleProduct
```

#### **Business Rules:**
- ❌ **Không cho phép xóa khi FlashSale đang diễn ra (Ongoing)**
- ✅ Tự động hoàn trả số lượng chưa bán về Inventory
- ✅ Cascade delete: Xóa FlashSale sẽ xóa tất cả FlashSaleProducts

#### **Code Reference:**
- **Service:** `FlashSaleAppService.cs` - Line 299-330
- **JavaScript:** `Index.js` - Line 145-169

---

### **1.5. Ẩn/Hiện FlashSale (ToggleHide)**

#### **Frontend Flow:**
```
1. User click nút "Ẩn" hoặc "Hiện" trong DataTable
   ↓
2. JavaScript: Index.js - Line 187-196
   - Gọi: abp.services.app.flashSale.toggleHide({id: flashSaleId})
   ↓
3. Nếu thành công:
   - Hiển thị thông báo
   - Reload DataTable
```

#### **Backend Flow:**
```
1. FlashSaleAppService.ToggleHide(int id)
   ↓
2. Lấy FlashSale:
   - _flashSaleRepository.GetAsync(id)
   ↓
3. Đảo ngược trạng thái:
   - flashSale.IsHidden = !flashSale.IsHidden
   ↓
4. Lưu vào database:
   - _flashSaleRepository.UpdateAsync(flashSale)
   - CurrentUnitOfWork.SaveChangesAsync()
```

#### **Code Reference:**
- **Service:** `FlashSaleAppService.cs` - Line 337-344
- **JavaScript:** `Index.js` - Line 187-196

---

### **1.6. Xem chi tiết FlashSale (Detail)**

#### **Frontend Flow:**
```
1. User click nút "Chi tiết" trong DataTable
   ↓
2. JavaScript: Index.js - Line 198-201
   - Redirect: /FlashSales/Detail?flashSaleId={id}
   ↓
3. Controller: FlashSalesController.Detail(flashSaleId)
   - Gọi: _flashSaleAppService.GetById(flashSaleId)
   - Trả về: View(flashSale) với model FlashSaleDto
   ↓
4. View: Detail.cshtml
   - Hiển thị thông tin FlashSale
   - Hiển thị danh sách sản phẩm trong FlashSale
   - Load JavaScript: Detail.js
```

#### **Backend Flow:**
```
1. FlashSaleAppService.GetById(int id)
   ↓
2. Lấy FlashSale kèm FlashSaleProducts và Product:
   - Include(fs => fs.FlashSaleProducts)
   - ThenInclude(fsp => fsp.Product)
   ↓
3. Map sang DTO:
   - FlashSaleDto với đầy đủ thông tin
   - List<FlashSaleProductDto> với thông tin Product
   ↓
4. Tính StatusText sau khi query
   ↓
5. Trả về: FlashSaleDto
```

#### **Code Reference:**
- **Controller:** `FlashSalesController.cs` - Line 64-68
- **Service:** `FlashSaleAppService.cs` - Line 157-207
- **View:** `Detail.cshtml`
- **JavaScript:** `Detail.js`

---

## 🛍️ 2. QUẢN LÝ SẢN PHẨM TRONG FLASHSALE

### **2.1. Thêm sản phẩm vào FlashSale (AddProduct)**

#### **Frontend Flow:**
```
1. User ở trang Detail.cshtml
   ↓
2. User click nút "Thêm sản phẩm"
   - Mở modal: #AddProductModal
   ↓
3. JavaScript: Detail.js - Line 114-116
   - Load danh sách sản phẩm vào select
   - Gọi: abp.services.app.product.getAllProducts()
   ↓
4. User chọn sản phẩm
   ↓
5. JavaScript: Detail.js - Line 31-49
   - Load thông tin Inventory:
     - Gọi: abp.services.app.inventory.getInventoryByProductId(productId)
     - Hiển thị: "Số lượng khả dụng: {availableQuantity}"
     - Set max cho input FlashSaleQuantity
   ↓
6. User điền form:
   - ProductId (select)
   - FlashSalePrice (decimal)
   - FlashSaleQuantity (int, max = availableQuantity)
   - MaxQuantityPerUser (int, optional)
   ↓
7. User click "Lưu"
   ↓
8. JavaScript: Detail.js - Line 52-67
   - Gọi: abp.services.app.flashSale.addProduct(formData)
   ↓
9. Nếu thành công:
   - Đóng modal
   - Reset form
   - Reload trang (location.reload())
```

#### **Backend Flow:**
```
1. FlashSaleAppService.AddProduct(AddProductToFlashSaleDto input)
   ↓
2. Validate:
   - FlashSale tồn tại
   - Product tồn tại
   - Sản phẩm chưa có trong FlashSale (không trùng)
   ↓
3. Kiểm tra Inventory:
   - Lấy Inventory của Product
   - Tính: availableQuantity = Quantity - ReservedQuantity
   - Kiểm tra: availableQuantity >= FlashSaleQuantity
   ↓
4. Khóa số lượng trong Inventory:
   - inventory.ReservedQuantity += input.FlashSaleQuantity
   - _inventoryRepository.UpdateAsync(inventory)
   ↓
5. Tạo FlashSaleProduct:
   - FlashSaleId = input.FlashSaleId
   - ProductId = input.ProductId
   - FlashSalePrice = input.FlashSalePrice
   - FlashSaleQuantity = input.FlashSaleQuantity
   - MaxQuantityPerUser = input.MaxQuantityPerUser
   - ReservedQuantity = input.FlashSaleQuantity (lưu để hoàn trả sau)
   ↓
6. Lưu vào database:
   - _flashSaleProductRepository.InsertAsync(flashSaleProduct)
   - CurrentUnitOfWork.SaveChangesAsync()
   ↓
7. Trả về: FlashSaleProductDto
```

#### **Business Rules:**
- ✅ Mỗi sản phẩm chỉ được thêm 1 lần vào FlashSale
- ✅ Số lượng FlashSale không được vượt quá số lượng khả dụng (Quantity - ReservedQuantity)
- ✅ Tự động khóa số lượng trong Inventory (tăng ReservedQuantity)
- ✅ Lưu ReservedQuantity để có thể hoàn trả sau

#### **Tích hợp Inventory:**
```
Khi thêm sản phẩm vào FlashSale:
Inventory.Quantity = 100
Inventory.ReservedQuantity = 20 (đã khóa cho FlashSale khác)
AvailableQuantity = 100 - 20 = 80

User thêm 50 sản phẩm vào FlashSale:
→ Inventory.ReservedQuantity = 20 + 50 = 70
→ AvailableQuantity = 100 - 70 = 30 (còn lại)
```

#### **Code Reference:**
- **Service:** `FlashSaleAppService.cs` - Line 355-402
- **JavaScript:** `Detail.js` - Line 11-67

---

### **2.2. Sửa sản phẩm trong FlashSale (UpdateProduct)**

#### **Frontend Flow:**
```
1. User ở trang Detail.cshtml
   ↓
2. User click nút "Sửa" trên sản phẩm
   ↓
3. JavaScript: Detail.js - Line 96-111
   - Gọi AJAX: /FlashSales/EditProductModal?flashSaleProductId={id}
   - Load HTML vào modal: #EditProductModal
   ↓
4. Controller: FlashSalesController.EditProductModal(flashSaleProductId)
   - Gọi: _flashSaleAppService.GetFlashSaleProductById(flashSaleProductId)
   - Map sang: AddProductToFlashSaleDto
   - Trả về: PartialView("_EditProductModal", updateDto)
   ↓
5. User sửa thông tin và click "Lưu"
   ↓
6. JavaScript: _EditProductModal.js
   - Gọi: abp.services.app.flashSale.updateProduct(flashSaleProductId, formData)
   ↓
7. Nếu thành công:
   - Đóng modal
   - Reload trang
```

#### **Backend Flow:**
```
1. FlashSaleAppService.UpdateProduct(int flashSaleProductId, AddProductToFlashSaleDto input)
   ↓
2. Lấy FlashSaleProduct kèm FlashSale:
   - Include(fsp => fsp.FlashSale)
   ↓
3. Kiểm tra trạng thái FlashSale:
   
   a) Nếu FlashSale.Status == Ongoing (đang diễn ra):
      - Chỉ cho phép sửa: FlashSalePrice, MaxQuantityPerUser
      - KHÔNG cho phép sửa: FlashSaleQuantity
   
   b) Nếu FlashSale.Status != Ongoing (chưa bắt đầu hoặc đã kết thúc):
      - Cho phép sửa tất cả: FlashSalePrice, FlashSaleQuantity, MaxQuantityPerUser
      - Nếu thay đổi FlashSaleQuantity:
        - Tính: quantityDiff = newQuantity - oldQuantity
        - Kiểm tra: availableQuantity >= quantityDiff
        - Cập nhật: inventory.ReservedQuantity += quantityDiff
   ↓
4. Lưu vào database:
   - _flashSaleProductRepository.UpdateAsync(flashSaleProduct)
   - CurrentUnitOfWork.SaveChangesAsync()
   ↓
5. Trả về: FlashSaleProductDto
```

#### **Business Rules:**
- ❌ **Khi FlashSale đang diễn ra (Ongoing):**
  - Chỉ cho phép sửa: FlashSalePrice, MaxQuantityPerUser
  - Không cho phép sửa: FlashSaleQuantity (để tránh ảnh hưởng đơn hàng)
- ✅ **Khi FlashSale chưa bắt đầu hoặc đã kết thúc:**
  - Cho phép sửa tất cả
  - Tự động cập nhật ReservedQuantity trong Inventory

#### **Code Reference:**
- **Controller:** `FlashSalesController.cs` - Line 70-84
- **Service:** `FlashSaleAppService.cs` - Line 448-510
- **View:** `_EditProductModal.cshtml`
- **JavaScript:** `_EditProductModal.js`, `Detail.js` - Line 96-111

---

### **2.3. Xóa sản phẩm khỏi FlashSale (RemoveProduct)**

#### **Frontend Flow:**
```
1. User ở trang Detail.cshtml
   ↓
2. User click nút "Xóa" trên sản phẩm
   ↓
3. JavaScript: Detail.js - Line 70-93
   - Hiển thị confirm dialog
   - Nếu confirm: Gọi abp.services.app.flashSale.removeProduct({id: flashSaleProductId})
   ↓
4. Nếu thành công:
   - Hiển thị thông báo
   - Reload trang
```

#### **Backend Flow:**
```
1. FlashSaleAppService.RemoveProduct(int flashSaleProductId)
   ↓
2. Lấy FlashSaleProduct kèm FlashSale:
   - Include(fsp => fsp.FlashSale)
   ↓
3. Validate:
   - Nếu FlashSale.Status == Ongoing && SoldQuantity > 0:
     - Không cho phép xóa (đã có người mua)
   ↓
4. Hoàn trả số lượng về Inventory:
   - Nếu !IsReturnedToInventory && ReservedQuantity > 0:
     - Gọi: ReturnProductQuantityToInventory(flashSaleProduct)
     - Tính: remainingQuantity = FlashSaleQuantity - SoldQuantity
     - Giảm: inventory.ReservedQuantity -= remainingQuantity
   ↓
5. Xóa FlashSaleProduct:
   - _flashSaleProductRepository.DeleteAsync(flashSaleProduct)
```

#### **Business Rules:**
- ❌ **Không cho phép xóa khi:**
  - FlashSale đang diễn ra (Ongoing) VÀ đã có người mua (SoldQuantity > 0)
- ✅ **Tự động hoàn trả số lượng chưa bán về Inventory**
- ✅ Chỉ hoàn trả nếu chưa được hoàn trả trước đó (!IsReturnedToInventory)

#### **Code Reference:**
- **Service:** `FlashSaleAppService.cs` - Line 410-438
- **JavaScript:** `Detail.js` - Line 70-93

---

## 🔄 3. TÍCH HỢP VỚI INVENTORY

### **3.1. Khóa số lượng khi thêm sản phẩm**

```
Khi thêm sản phẩm vào FlashSale:
1. Kiểm tra số lượng khả dụng:
   availableQuantity = Inventory.Quantity - Inventory.ReservedQuantity

2. Khóa số lượng:
   Inventory.ReservedQuantity += FlashSaleQuantity

3. Lưu ReservedQuantity vào FlashSaleProduct:
   FlashSaleProduct.ReservedQuantity = FlashSaleQuantity
```

**Ví dụ:**
```
Trước khi thêm:
- Inventory.Quantity = 100
- Inventory.ReservedQuantity = 10
- AvailableQuantity = 90

Thêm 50 sản phẩm vào FlashSale:
- Inventory.ReservedQuantity = 10 + 50 = 60
- AvailableQuantity = 100 - 60 = 40
- FlashSaleProduct.ReservedQuantity = 50
```

---

### **3.2. Hoàn trả số lượng khi xóa sản phẩm**

```
Khi xóa sản phẩm khỏi FlashSale:
1. Tính số lượng còn lại:
   remainingQuantity = FlashSaleQuantity - SoldQuantity

2. Hoàn trả về Inventory:
   Inventory.ReservedQuantity -= remainingQuantity

3. Đánh dấu đã hoàn trả:
   FlashSaleProduct.IsReturnedToInventory = true
```

**Ví dụ:**
```
Trước khi xóa:
- FlashSaleQuantity = 50
- SoldQuantity = 10
- RemainingQuantity = 40
- Inventory.ReservedQuantity = 60

Xóa sản phẩm khỏi FlashSale:
- Inventory.ReservedQuantity = 60 - 40 = 20
- FlashSaleProduct.IsReturnedToInventory = true
```

---

### **3.3. Hoàn trả số lượng khi FlashSale kết thúc**

```
Khi FlashSale kết thúc (Ended):
1. Gọi: ReturnRemainingQuantityToInventory(flashSaleId)

2. Với mỗi FlashSaleProduct:
   - Nếu !IsReturnedToInventory:
     - Tính: remainingQuantity = FlashSaleQuantity - SoldQuantity
     - Giảm: Inventory.ReservedQuantity -= remainingQuantity
     - Đánh dấu: IsReturnedToInventory = true

3. Lưu vào database
```

**Code Reference:**
- **Service:** `FlashSaleAppService.cs` - Line 518-543
- **Private Method:** `ReturnProductQuantityToInventory` - Line 566-583

---

## 📈 4. TRẠNG THÁI FLASHSALE

### **4.1. FlashSaleStatus Enum**

```csharp
public enum FlashSaleStatus : byte
{
    NotStarted = 0,  // Chưa bắt đầu
    Ongoing = 1,     // Đang diễn ra
    Ended = 2,       // Đã kết thúc
    Cancelled = 3    // Đã hủy
}
```

### **4.2. Tính toán trạng thái (CalculatedStatus)**

```csharp
public FlashSaleStatus CalculatedStatus
{
    get
    {
        var now = DateTime.Now;
        if (now < StartTime)
            return FlashSaleStatus.NotStarted;
        if (now >= StartTime && now <= EndTime)
            return FlashSaleStatus.Ongoing;
        if (now > EndTime)
            return FlashSaleStatus.Ended;
        return FlashSaleStatus.Cancelled;
    }
}
```

### **4.3. Business Rules theo trạng thái**

| Trạng thái | Cho phép sửa FlashSale | Cho phép sửa số lượng sản phẩm | Cho phép xóa FlashSale | Cho phép xóa sản phẩm |
|------------|------------------------|--------------------------------|------------------------|----------------------|
| NotStarted | ✅ Có | ✅ Có | ✅ Có | ✅ Có (nếu chưa bán) |
| Ongoing    | ❌ Không | ❌ Không | ❌ Không | ❌ Không (nếu đã bán) |
| Ended      | ✅ Có | ✅ Có | ✅ Có | ✅ Có |
| Cancelled  | ✅ Có | ✅ Có | ✅ Có | ✅ Có |

---

## 🔐 5. VALIDATION & SECURITY

### **5.1. Validation Rules**

#### **CreateFlashSaleDto:**
- ✅ Name: Required, MaxLength(256)
- ✅ StartTime: Required, Must be in future
- ✅ EndTime: Required, Must be after StartTime
- ✅ Description: Optional, MaxLength(2000)

#### **AddProductToFlashSaleDto:**
- ✅ FlashSaleId: Required
- ✅ ProductId: Required
- ✅ FlashSalePrice: Required, Must be > 0
- ✅ FlashSaleQuantity: Required, Must be > 0, Must be <= AvailableQuantity
- ✅ MaxQuantityPerUser: Optional

### **5.2. Authorization**

```csharp
[AbpMvcAuthorize(PermissionNames.Pages_Products)]
public class FlashSalesController : MyProjectControllerBase
```

- ✅ Yêu cầu quyền: `Pages_Products`
- ✅ Chỉ admin mới có thể quản lý FlashSale

---

## 📝 6. CÁC FILE QUAN TRỌNG

### **6.1. Backend**

| File | Mô tả |
|------|-------|
| `FlashSale.cs` | Entity FlashSale |
| `FlashSaleProduct.cs` | Entity FlashSaleProduct |
| `FlashSaleAppService.cs` | Business logic |
| `IFlashSaleAppService.cs` | Interface |
| `FlashSaleDto.cs` | DTO cho FlashSale |
| `FlashSaleProductDto.cs` | DTO cho FlashSaleProduct |
| `CreateFlashSaleDto.cs` | DTO tạo mới FlashSale |
| `UpdateFlashSaleDto.cs` | DTO cập nhật FlashSale |
| `AddProductToFlashSaleDto.cs` | DTO thêm sản phẩm |
| `GetAllFlashSalesInput.cs` | DTO tìm kiếm và phân trang |

### **6.2. Frontend**

| File | Mô tả |
|------|-------|
| `FlashSalesController.cs` | MVC Controller |
| `Index.cshtml` | View danh sách FlashSale |
| `Detail.cshtml` | View chi tiết FlashSale |
| `_CreateModal.cshtml` | Modal tạo mới FlashSale |
| `_EditModal.cshtml` | Modal sửa FlashSale |
| `_EditProductModal.cshtml` | Modal sửa sản phẩm |
| `Index.js` | JavaScript cho danh sách |
| `Detail.js` | JavaScript cho chi tiết |
| `_EditModal.js` | JavaScript cho modal sửa |
| `_EditProductModal.js` | JavaScript cho modal sửa sản phẩm |

---

## 🎯 7. TỔNG KẾT LUỒNG

### **7.1. Luồng tạo FlashSale và thêm sản phẩm**

```
1. Admin tạo FlashSale
   → FlashSale.Status = NotStarted
   
2. Admin thêm sản phẩm vào FlashSale
   → Khóa số lượng trong Inventory (ReservedQuantity += FlashSaleQuantity)
   → FlashSaleProduct.ReservedQuantity = FlashSaleQuantity
   
3. FlashSale bắt đầu (StartTime <= Now <= EndTime)
   → FlashSale.Status = Ongoing
   → Khách hàng có thể mua sản phẩm
   
4. FlashSale kết thúc (Now > EndTime)
   → FlashSale.Status = Ended
   → Hoàn trả số lượng chưa bán về Inventory
   → Inventory.ReservedQuantity -= RemainingQuantity
   → FlashSaleProduct.IsReturnedToInventory = true
```

### **7.2. Luồng xóa FlashSale**

```
1. Admin xóa FlashSale
   → Kiểm tra: Status != Ongoing
   → Hoàn trả số lượng chưa bán về Inventory
   → Xóa FlashSale (cascade delete FlashSaleProducts)
```

### **7.3. Luồng xóa sản phẩm khỏi FlashSale**

```
1. Admin xóa sản phẩm khỏi FlashSale
   → Kiểm tra: !(Status == Ongoing && SoldQuantity > 0)
   → Hoàn trả số lượng chưa bán về Inventory
   → Xóa FlashSaleProduct
```

---

## 🚀 8. CÁCH SỬ DỤNG

### **8.1. Tạo FlashSale mới**

1. Truy cập: `/FlashSales/Index`
2. Click nút "Tạo mới"
3. Điền thông tin:
   - Tên FlashSale
   - Mô tả (optional)
   - Thời gian bắt đầu (phải trong tương lai)
   - Thời gian kết thúc (phải sau thời gian bắt đầu)
   - IsActive (mặc định: true)
   - IsHidden (mặc định: false)
4. Click "Lưu"

### **8.2. Thêm sản phẩm vào FlashSale**

1. Click nút "Chi tiết" trên FlashSale
2. Click nút "Thêm sản phẩm"
3. Chọn sản phẩm (sẽ hiển thị số lượng khả dụng)
4. Điền thông tin:
   - Giá FlashSale
   - Số lượng FlashSale (không được vượt quá số lượng khả dụng)
   - Giới hạn mua mỗi người (optional)
5. Click "Lưu"

### **8.3. Quản lý FlashSale**

- **Sửa:** Click nút "Sửa" (chỉ khi chưa bắt đầu hoặc đã kết thúc)
- **Xóa:** Click nút "Xóa" (chỉ khi chưa bắt đầu hoặc đã kết thúc)
- **Ẩn/Hiện:** Click nút "Ẩn" hoặc "Hiện"
- **Chi tiết:** Click nút "Chi tiết" để quản lý sản phẩm

---

## 📚 9. TÀI LIỆU THAM KHẢO

- **ABP Framework:** https://docs.abp.io/
- **Entity Framework Core:** https://docs.microsoft.com/en-us/ef/core/
- **DataTable.js:** https://datatables.net/
- **jQuery:** https://jquery.com/

---

**Tài liệu được tạo bởi:** AI Assistant  
**Ngày tạo:** 2025-01-08  
**Phiên bản:** 1.0


