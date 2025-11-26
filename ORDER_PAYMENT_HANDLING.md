# XỬ LÝ ĐƠN HÀNG CHƯA THANH TOÁN

## Tổng quan

Hệ thống tự động xử lý các đơn hàng đã đặt nhưng chưa thanh toán theo các cơ chế sau:

1. **Hạn thanh toán**: Mỗi đơn hàng có thời gian hết hạn thanh toán (mặc định: 30 phút sau khi tạo đơn)
2. **Tự động kiểm tra thanh toán**: Background job kiểm tra và xác nhận thanh toán tự động mỗi 1 phút
3. **Tự động hủy đơn hết hạn**: Background job hủy các đơn hàng đã hết hạn thanh toán mỗi 5 phút

---

## 1. QUY TRÌNH XỬ LÝ ĐƠN HÀNG CHƯA THANH TOÁN

### 1.1. Khi khách hàng tạo đơn hàng

**Trong `CheckoutController.CreateOrder()`:**

```csharp
var paymentExpiredAt = DateTime.UtcNow.AddMinutes(30); // Hạn thanh toán: 30 phút
```

**Các bước:**
1. Reserve inventory (giữ hàng trong kho)
2. Tạo đơn hàng với:
   - `OrderStatus = Pending`
   - `IsPaid = false`
   - `PaymentExpiredAt = CreationTime + 30 phút`
   - `PaymentReference = "MP{timestamp}"`

**Lưu ý:**
- Hàng đã được reserve, không thể bán cho khách hàng khác
- Nếu không thanh toán trong 30 phút, đơn hàng sẽ bị hủy tự động

---

### 1.2. Tự động kiểm tra thanh toán (PaymentVerificationBackgroundJob)

**File:** `PaymentVerificationBackgroundJob.cs`

**Chức năng:**
- Chạy mỗi **1 phút**
- Tìm các đơn hàng: `Pending`, `IsPaid = false`, `PaymentExpiredAt > DateTime.UtcNow`
- Kiểm tra xem có giao dịch thanh toán khớp không (qua VNPay hoặc QR Transfer)
- Tự động xác nhận thanh toán nếu tìm thấy giao dịch

**Kết quả khi tìm thấy thanh toán:**
- `IsPaid = true`
- `OrderStatus = Confirmed`
- `PaidTime = TransactionTime`
- Commit reserved inventory (xuất kho)

---

### 1.3. Tự động hủy đơn hết hạn (CancelExpiredOrdersBackgroundJob)

**File:** `CancelExpiredOrdersBackgroundJob.cs`

**Chức năng:**
- Chạy mỗi **5 phút**
- Tìm các đơn hàng: `Pending`, `IsPaid = false`, `PaymentExpiredAt < DateTime.UtcNow`
- Tự động hủy các đơn hàng đã hết hạn

**Quy trình hủy:**
1. Lấy danh sách OrderDetails
2. Giải phóng inventory đã reserve (ReleaseReservedInventory)
3. Cập nhật `OrderStatus = Canceled`
4. Ghi log

**Lưu ý:**
- Hàng được giải phóng, có thể bán cho khách hàng khác
- Đơn hàng không thể khôi phục sau khi hủy

---

## 2. CÁC TRẠNG THÁI ĐƠN HÀNG

| Trạng thái | Mô tả | IsPaid | Hành động |
|------------|-------|--------|-----------|
| **Pending** | Chờ thanh toán | `false` | Đang chờ khách hàng thanh toán |
| **Confirmed** | Đã xác nhận | `true` | Đã thanh toán, chờ xử lý |
| **Shipping** | Đang giao hàng | `true` | Đang vận chuyển |
| **Success** | Hoàn thành | `true` | Đã giao hàng thành công |
| **Canceled** | Đã hủy | `false` | Đơn hàng đã bị hủy |

---

## 3. QUẢN LÝ INVENTORY

### 3.1. Reserve Inventory (Khi tạo đơn)

```csharp
await _inventoryAppService.ReserveInventory(productId, quantity);
```

**Kết quả:**
- `ReservedQuantity` tăng
- `AvailableQuantity` giảm
- Hàng không thể bán cho khách hàng khác

### 3.2. Commit Reserved Inventory (Khi thanh toán thành công)

```csharp
await _inventoryAppService.CommitReservedInventory(productId, quantity);
```

**Kết quả:**
- `ReservedQuantity` giảm
- `Quantity` giảm (xuất kho thực tế)
- Hàng đã được xuất kho

### 3.3. Release Reserved Inventory (Khi hủy đơn)

```csharp
await _inventoryAppService.ReleaseReservedInventory(productId, quantity);
```

**Kết quả:**
- `ReservedQuantity` giảm
- `AvailableQuantity` tăng
- Hàng có thể bán lại cho khách hàng khác

---

## 4. CẤU HÌNH THỜI GIAN

### 4.1. Thời gian hết hạn thanh toán

**Mặc định:** 30 phút

**Thay đổi:** Sửa trong `CheckoutController.CreateOrder()`:

```csharp
var paymentExpiredAt = DateTime.UtcNow.AddMinutes(30); // Thay đổi số phút ở đây
```

**Khuyến nghị:**
- **15-30 phút**: Cho sản phẩm có nhu cầu cao, hàng tồn kho ít
- **30-60 phút**: Cho sản phẩm thông thường
- **60-120 phút**: Cho đơn hàng lớn, giá trị cao

### 4.2. Tần suất chạy Background Jobs

**PaymentVerificationBackgroundJob:**
- **Hiện tại:** 1 phút
- **File:** `PaymentVerificationBackgroundJob.cs`
- **Dòng:** `Period = 60000` (milliseconds)

**CancelExpiredOrdersBackgroundJob:**
- **Hiện tại:** 5 phút
- **File:** `CancelExpiredOrdersBackgroundJob.cs`
- **Dòng:** `Period = 300000` (milliseconds)

**Thay đổi:** Sửa giá trị `Period` trong constructor của mỗi job

---

## 5. XỬ LÝ THỦ CÔNG (CHO ADMIN)

### 5.1. Xem danh sách đơn hàng chưa thanh toán

**API:**
```csharp
var pendingOrders = await _orderAppService.GetPendingUnpaidOrdersAsync();
```

**Điều kiện:**
- `OrderStatus = Pending`
- `IsPaid = false`
- `PaymentExpiredAt > DateTime.UtcNow` (chưa hết hạn)

### 5.2. Hủy đơn hàng thủ công

**API:**
```csharp
await _orderAppService.UpdateOrder(new UpdateOrderDto
{
    OrderId = orderId,
    OrderStatus = (int)OrderStatus.Canceled
});
```

**Lưu ý:** Cần giải phóng inventory thủ công:

```csharp
var orderDetails = await _orderDetailAppService.GetOrderListById(orderId);
foreach (var detail in orderDetails)
{
    await _inventoryAppService.ReleaseReservedInventory(detail.ProductId, detail.Quantity);
}
```

### 5.3. Gia hạn thời gian thanh toán

**Cập nhật PaymentExpiredAt:**

```csharp
var order = await _orderAppService.GetOrderById(orderId);
order.PaymentExpiredAt = DateTime.UtcNow.AddMinutes(30); // Gia hạn thêm 30 phút
await _orderAppService.UpdateAsync(order);
```

---

## 6. MONITORING VÀ LOGGING

### 6.1. Logs quan trọng

**PaymentVerificationBackgroundJob:**
- `"Bắt đầu kiểm tra thanh toán cho {count} đơn hàng"`
- `"Đã tự động xác nhận thanh toán cho đơn hàng #{orderId}"`

**CancelExpiredOrdersBackgroundJob:**
- `"Bắt đầu kiểm tra và hủy các đơn hàng đã hết hạn thanh toán"`
- `"Đã tự động hủy {count} đơn hàng do hết hạn thanh toán"`
- `"Đã tự động hủy đơn hàng #{orderId} do hết hạn thanh toán. Hạn thanh toán: {expiredAt}"`

### 6.2. Kiểm tra job có chạy không

**Xem log file:**
- `App_Data/Logs/Logs.txt`

**Tìm kiếm:**
- `"PaymentVerificationBackgroundJob"` - Job kiểm tra thanh toán
- `"CancelExpiredOrdersBackgroundJob"` - Job hủy đơn hết hạn

---

## 7. TROUBLESHOOTING

### 7.1. Đơn hàng không tự động hủy sau khi hết hạn

**Nguyên nhân có thể:**
1. Background jobs bị tắt
   - **Kiểm tra:** `MyProjectApplicationModule.cs` có `Configuration.BackgroundJobs.IsJobExecutionEnabled = true;` không?
2. Job chưa được đăng ký
   - **Kiểm tra:** Class có implement `ISingletonDependency` không?
3. Ứng dụng chưa khởi động đầy đủ
   - **Giải pháp:** Restart ứng dụng

**Cách kiểm tra:**
- Xem log có message từ `CancelExpiredOrdersBackgroundJob` không
- Kiểm tra database: `SELECT * FROM AppOrders WHERE OrderStatus = 0 AND IsPaid = 0 AND PaymentExpiredAt < GETUTCDATE()`

### 7.2. Inventory không được giải phóng khi hủy đơn

**Nguyên nhân:**
- Lỗi trong quá trình `ReleaseReservedInventory`
- OrderDetails không tồn tại

**Giải pháp:**
- Kiểm tra log có warning/error không
- Kiểm tra OrderDetails có tồn tại không
- Chạy lại job hoặc giải phóng thủ công

### 7.3. Đơn hàng không tự động xác nhận thanh toán

**Nguyên nhân:**
- PaymentVerificationBackgroundJob không tìm thấy giao dịch
- PaymentReference không khớp
- Số tiền không khớp

**Giải pháp:**
- Kiểm tra PaymentTransaction có tồn tại không
- Kiểm tra PaymentReference và Amount có khớp không
- Xác nhận thanh toán thủ công nếu cần

---

## 8. BEST PRACTICES

1. **Thời gian hết hạn hợp lý:**
   - Không quá ngắn (khách hàng không kịp thanh toán)
   - Không quá dài (giữ hàng quá lâu, ảnh hưởng doanh số)

2. **Monitoring:**
   - Theo dõi log thường xuyên
   - Kiểm tra số lượng đơn hàng Pending
   - Cảnh báo nếu có nhiều đơn hàng bị hủy

3. **Thông báo khách hàng:**
   - Gửi email/SMS nhắc nhở thanh toán trước khi hết hạn
   - Hiển thị countdown timer trên trang thanh toán

4. **Xử lý ngoại lệ:**
   - Luôn có try-catch trong background jobs
   - Log đầy đủ để debug
   - Không để lỗi một đơn hàng ảnh hưởng các đơn hàng khác

---

## 9. TÓM TẮT

| Vấn đề | Giải pháp | Tự động? |
|--------|-----------|---------|
| Đơn hàng chưa thanh toán | Chờ trong 30 phút | ✅ |
| Tự động xác nhận thanh toán | PaymentVerificationBackgroundJob (1 phút) | ✅ |
| Hủy đơn hết hạn | CancelExpiredOrdersBackgroundJob (5 phút) | ✅ |
| Giải phóng inventory | Tự động khi hủy đơn | ✅ |
| Gia hạn thời gian | Admin thủ công | ❌ |
| Hủy đơn thủ công | Admin thủ công | ❌ |

---

**Cập nhật lần cuối:** 2024-12-21






