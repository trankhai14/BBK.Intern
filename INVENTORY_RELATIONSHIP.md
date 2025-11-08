# Mối quan hệ và Luồng dữ liệu giữa Inventory và InventoryTransaction

## 1. MỐI QUAN HỆ GIỮA 2 BẢNG

```
┌─────────────────┐         ┌──────────────────────────┐
│   Product       │         │      Product             │
│   (Sản phẩm)    │         │      (Sản phẩm)          │
└────────┬────────┘         └──────────────┬───────────┘
         │                                 │
         │ FK: ProductId                  │ FK: ProductId
         │                                 │
    ┌────▼────────┐                   ┌───▼────────────────────┐
    │ Inventory   │                   │ InventoryTransaction   │
    │ (Tồn kho)   │                   │ (Lịch sử giao dịch)    │
    │             │                   │                        │
    │ - Id        │                   │ - Id                   │
    │ - ProductId │                   │ - ProductId            │
    │ - Quantity  │                   │ - Type (Import/Export) │
    │ - Reserved  │                   │ - Quantity             │
    │ - Reorder   │                   │ - QuantityBefore       │
    │             │                   │ - QuantityAfter        │
    │ 1 dòng/SP   │                   │ - Reason, Notes        │
    │             │                   │ - TransactionDate      │
    └─────────────┘                   │ Nhiều dòng/SP          │
                                      └────────────────────────┘
```

### Quan hệ:
- **Quan hệ gián tiếp**: Cả 2 bảng đều liên kết với `Product` qua `ProductId`
- **Không có Foreign Key trực tiếp** giữa Inventory và InventoryTransaction
- **Quan hệ logic**: Inventory = Snapshot hiện tại, Transaction = Lịch sử

---

## 2. LUỒNG DỮ LIỆU KHI NHẬP KHO

```
┌─────────────────────────────────────────────────────────────┐
│ NHẬP KHO: ImportInventory(ProductId=1, Quantity=50)         │
└─────────────────────────────────────────────────────────────┘
                           │
                           ▼
        ┌──────────────────────────────────────┐
        │ 1. Đọc Inventory hiện tại            │
        │    ProductId = 1                     │
        │    QuantityBefore = 100              │
        └──────────────┬───────────────────────┘
                       │
                       ▼
        ┌──────────────────────────────────────┐
        │ 2. UPDATE Inventory                  │
        │    Quantity = 100 + 50 = 150         │
        │    LastUpdated = DateTime.Now        │
        └──────────────┬───────────────────────┘
                       │
                       ▼
        ┌──────────────────────────────────────┐
        │ 3. INSERT InventoryTransaction       │
        │    Type = Import                     │
        │    ProductId = 1                     │
        │    Quantity = 50                     │
        │    QuantityBefore = 100              │
        │    QuantityAfter = 150               │
        │    Reason = "Nhập kho"               │
        │    TransactionDate = Now             │
        └──────────────────────────────────────┘

KẾT QUẢ:
├─ Inventory: Quantity = 150 (cập nhật)
└─ InventoryTransaction: 1 dòng mới (thêm vào)
```

---

## 3. LUỒNG DỮ LIỆU KHI XUẤT KHO

```
┌─────────────────────────────────────────────────────────────┐
│ XUẤT KHO: ExportInventory(ProductId=1, Quantity=30)         │
└─────────────────────────────────────────────────────────────┘
                           │
                           ▼
        ┌──────────────────────────────────────┐
        │ 1. Kiểm tra Inventory                │
        │    ProductId = 1                     │
        │    Quantity = 150                    │
        │    AvailableQuantity = 150           │
        │    ✓ Đủ hàng (150 >= 30)            │
        └──────────────┬───────────────────────┘
                       │
                       ▼
        ┌──────────────────────────────────────┐
        │ 2. UPDATE Inventory                  │
        │    Quantity = 150 - 30 = 120         │
        │    LastUpdated = DateTime.Now        │
        └──────────────┬───────────────────────┘
                       │
                       ▼
        ┌──────────────────────────────────────┐
        │ 3. INSERT InventoryTransaction       │
        │    Type = Export                     │
        │    ProductId = 1                     │
        │    Quantity = 30                     │
        │    QuantityBefore = 150              │
        │    QuantityAfter = 120               │
        │    Reason = "Xuất kho"               │
        │    TransactionDate = Now             │
        └──────────────────────────────────────┘

KẾT QUẢ:
├─ Inventory: Quantity = 120 (cập nhật)
└─ InventoryTransaction: 1 dòng mới (thêm vào)
```

---

## 4. QUAN HỆ DỮ LIỆU THEO THỜI GIAN

```
Thời gian    │ Inventory (ProductId=1)  │ InventoryTransaction
─────────────┼──────────────────────────┼─────────────────────────────
Ban đầu      │ Quantity = 0             │ (chưa có)
             │                          │
T1: Nhập 100 │ Quantity = 100           │ Transaction #1: Import 100
             │                          │   Before=0, After=100
             │                          │
T2: Nhập 50  │ Quantity = 150           │ Transaction #2: Import 50
             │                          │   Before=100, After=150
             │                          │
T3: Xuất 30  │ Quantity = 120           │ Transaction #3: Export 30
             │                          │   Before=150, After=120
             │                          │
T4: Xuất 20  │ Quantity = 100           │ Transaction #4: Export 20
             │                          │   Before=120, After=100
             │                          │
Hiện tại     │ Quantity = 100           │ 4 dòng transaction
             │                          │ (Tổng nhập: 150, Tổng xuất: 50)
```

---

## 5. ĐỒNG BỘ DỮ LIỆU

### Nguyên tắc:
- **Inventory luôn là nguồn chính xác nhất** tại thời điểm hiện tại
- **InventoryTransaction chỉ INSERT, không UPDATE/DELETE** (Audit Trail)
- **Kiểm tra tính nhất quán**: 
  ```
  Inventory.Quantity = SUM(Import) - SUM(Export) từ Transaction
  ```

### Code thực hiện trong InventoryTransactionAppService:

```csharp
// NHẬP KHO
1. Đọc Inventory.Quantity (quantityBefore)
2. UPDATE Inventory.Quantity += quantity
3. Đọc lại Inventory.Quantity (quantityAfter)
4. INSERT InventoryTransaction với Before/After

// XUẤT KHO  
1. Kiểm tra Inventory.AvailableQuantity >= quantity
2. Đọc Inventory.Quantity (quantityBefore)
3. UPDATE Inventory.Quantity -= quantity
4. Đọc lại Inventory.Quantity (quantityAfter)
5. INSERT InventoryTransaction với Before/After
```

---

## 6. CÁC TRƯỜNG HỢP SỬ DỤNG

### A. Kiểm tra tồn kho (Truy vấn Inventory)
```csharp
var inventory = await _inventoryRepository
    .FirstOrDefaultAsync(x => x.ProductId == 1);
// Kết quả: Quantity = 100 (NHANH)
```

### B. Xem lịch sử nhập xuất (Truy vấn Transaction)
```csharp
var transactions = await _transactionRepository
    .GetAll()
    .Where(x => x.ProductId == 1)
    .OrderByDescending(x => x.TransactionDate)
    .ToListAsync();
// Kết quả: Danh sách tất cả giao dịch
```

### C. Báo cáo tổng hợp (Cả 2 bảng)
```csharp
// Từ Inventory: Lấy tồn kho hiện tại
var currentStock = inventory.Quantity;

// Từ Transaction: Tính tổng nhập/xuất trong tháng
var monthlyImport = transactions
    .Where(x => x.Type == Import && x.TransactionDate.Month == 12)
    .Sum(x => x.Quantity);
```

---

## 7. ĐIỂM QUAN TRỌNG

### ✅ Luôn đồng bộ:
- Mỗi lần thay đổi Inventory → Tạo Transaction tương ứng
- QuantityAfter trong Transaction = Quantity trong Inventory sau khi UPDATE

### ✅ Transaction là bản ghi không đổi:
- Không bao giờ UPDATE hoặc DELETE Transaction
- Nếu cần sửa → Tạo Transaction mới (điều chỉnh)

### ✅ Inventory là Single Source of Truth:
- Luôn truy vấn Inventory để biết tồn kho hiện tại
- Transaction chỉ để xem lịch sử, không dùng để tính tồn kho

---

## 8. VÍ DỤ KIỂM TRA TÍNH NHẤT QUÁN

```sql
-- Tồn kho từ Inventory
SELECT Quantity FROM Inventory WHERE ProductId = 1;
-- Kết quả: 100

-- Tồn kho tính từ Transaction
SELECT 
    (SELECT SUM(Quantity) FROM InventoryTransaction 
     WHERE ProductId = 1 AND Type = 1) -  -- Import
    (SELECT SUM(Quantity) FROM InventoryTransaction 
     WHERE ProductId = 1 AND Type = 2)    -- Export
AS CalculatedQuantity;
-- Kết quả: 100 (phải khớp)

-- Nếu khác nhau → Có lỗi đồng bộ!
```

---

## 9. SƠ ĐỒ TƯƠNG TÁC

```
User → InventoryTransactionAppService
         │
         ├─→ 1. Kiểm tra Inventory
         │
         ├─→ 2. Update Inventory (tăng/giảm)
         │
         └─→ 3. Insert InventoryTransaction (ghi log)

Other Services → InventoryAppService
                   │
                   └─→ Chỉ truy vấn/cập nhật Inventory
                       (không tạo Transaction)
                       VD: Tăng ReservedQuantity khi đặt hàng
```

