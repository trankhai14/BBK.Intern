# HƯỚNG DẪN TEST BACKGROUND JOB

## 1. CÁCH KIỂM TRA BACKGROUND JOB CÓ CHẠY KHÔNG

### Phương pháp 1: Kiểm tra Log File (Khuyến nghị)

**Bước 1**: Tìm file log
- File log thường nằm ở: 
  - `App_Data/Logs/Logs.txt` (trong project)
  - `bin/Debug/net7.0/App_Data/Logs/Logs.txt` (sau khi build)
- Hoặc kiểm tra trong **Output** window của Visual Studio

**Bước 2**: Tìm log message từ Background Job
- Mở file log và tìm các dòng có chứa: `PaymentVerificationBackgroundJob`
- Hoặc tìm: `Bắt đầu kiểm tra thanh toán cho X đơn hàng`

**Bước 3**: Kiểm tra chu kỳ chạy
- Log sẽ xuất hiện mỗi 1 phút một lần
- Nếu có đơn hàng Pending, sẽ thấy log: `Bắt đầu kiểm tra thanh toán cho X đơn hàng`
- Nếu không có đơn hàng, không có log (job vẫn chạy nhưng return sớm)

**Ví dụ log:**
```
INFO  2025-12-20 10:00:00 [X] PaymentVerificationBackgroundJob - Bắt đầu kiểm tra thanh toán cho 2 đơn hàng
INFO  2025-12-20 10:01:00 [X] PaymentVerificationBackgroundJob - Bắt đầu kiểm tra thanh toán cho 2 đơn hàng
INFO  2025-12-20 10:02:00 [X] PaymentVerificationBackgroundJob - Bắt đầu kiểm tra thanh toán cho 1 đơn hàng
```

---

### Phương pháp 2: Sử dụng Test Controller (Nhanh nhất)

**Bước 1**: Truy cập test endpoint
- URL: `http://localhost:XXXX/TestBackgroundJob/TriggerJob`
- Method: GET
- Yêu cầu: Đã đăng nhập (có quyền)

**Bước 2**: Xem kết quả
- Response sẽ cho biết:
  - Số đơn hàng đang chờ kiểm tra
  - Số đơn hàng đã được xử lý
  - Số đơn hàng đã được xác nhận thanh toán

**Bước 3**: Kiểm tra log sau khi trigger
- Xem log để biết chi tiết quá trình xử lý

---

### Phương pháp 3: Kiểm tra trong Visual Studio Output

**Bước 1**: Mở Output window
- `View` → `Output` hoặc nhấn `Ctrl+Alt+O`

**Bước 2**: Chọn output source
- Chọn **Show output from**: `Debug` hoặc tên project

**Bước 3**: Tìm log
- Tìm kiếm: `PaymentVerificationBackgroundJob` hoặc `Bắt đầu kiểm tra`

---

## 2. CÁC TEST ENDPOINT CÓ SẴN

### 2.1. Trigger Job Thủ Công
```
GET: /TestBackgroundJob/TriggerJob
```
**Mô tả**: Chạy logic của Background Job ngay lập tức (không cần đợi 1 phút)

**Response mẫu:**
```json
{
  "success": true,
  "message": "Background Job đã được trigger thành công",
  "data": {
    "totalOrders": 2,
    "processedOrders": 2,
    "verifiedOrders": 0,
    "timestamp": "2025-12-20 10:30:00"
  }
}
```

### 2.2. Kiểm tra Trạng Thái Job
```
GET: /TestBackgroundJob/GetJobStatus
```
**Mô tả**: Xem thông tin về Background Job (tên, chu kỳ, mô tả)

**Response mẫu:**
```json
{
  "success": true,
  "data": {
    "jobName": "PaymentVerificationBackgroundJob",
    "jobType": "MyProject.Payments.BackgroundJobs.PaymentVerificationBackgroundJob",
    "period": "60000 milliseconds (1 phút)",
    "description": "Tự động kiểm tra và xác nhận thanh toán cho các đơn hàng Pending",
    "timestamp": "2025-12-20 10:30:00"
  }
}
```

### 2.3. Xem Danh Sách Đơn Hàng Pending
```
GET: /TestBackgroundJob/GetPendingOrders
```
**Mô tả**: Xem danh sách đơn hàng đang chờ kiểm tra thanh toán

**Response mẫu:**
```json
{
  "success": true,
  "data": {
    "totalCount": 2,
    "orders": [
      {
        "orderId": 1,
        "paymentReference": "MP20241220120000",
        "totalAmount": 5000000,
        "creationTime": "2025-12-20 10:00:00",
        "paymentExpiredAt": "2025-12-20 10:30:00"
      }
    ],
    "timestamp": "2025-12-20 10:30:00"
  }
}
```

---

## 3. CÁC BƯỚC TEST CHI TIẾT

### Test 1: Kiểm tra Job có được đăng ký không

**Mục đích**: Đảm bảo Background Job được ABP đăng ký và khởi động

**Các bước:**
1. Chạy ứng dụng (F5 hoặc `dotnet run`)
2. Kiểm tra log khi khởi động - không có lỗi về Background Job
3. Đợi 1-2 phút
4. Kiểm tra log xem có message từ Background Job không

**Kết quả mong đợi:**
- Không có lỗi khi khởi động
- Sau 1 phút, có log từ Background Job (nếu có đơn hàng Pending)

---

### Test 2: Test với đơn hàng thật

**Mục đích**: Kiểm tra job có xử lý đơn hàng thật không

**Các bước:**
1. Tạo một đơn hàng test:
   - Đăng nhập với tài khoản khách hàng
   - Thêm sản phẩm vào giỏ hàng
   - Checkout và tạo đơn hàng (Status: Pending, IsPaid: false)
2. Đợi 1 phút (hoặc trigger job thủ công qua test endpoint)
3. Kiểm tra log:
   - Tìm: `Bắt đầu kiểm tra thanh toán cho X đơn hàng`
   - Xem có log xử lý đơn hàng không
4. Kiểm tra database:
   - Xem đơn hàng có được cập nhật không (nếu có giao dịch khớp)

**Kết quả mong đợi:**
- Log hiển thị số đơn hàng đang kiểm tra
- Nếu có giao dịch khớp, đơn hàng được tự động xác nhận

---

### Test 3: Test với giao dịch giả (Simulate Payment)

**Mục đích**: Kiểm tra job có tự động xác nhận khi có giao dịch khớp

**Các bước:**
1. Tạo đơn hàng test (như Test 2)
2. Tạo PaymentTransaction giả trong database:
   ```sql
   INSERT INTO AppPaymentTransactions 
   (OrderId, PaymentReference, Amount, TransactionTime, Status, VerifiedBy, VerifiedAt, CreationTime)
   VALUES 
   (1, 'MP20241220120000', 5000000, GETUTCDATE(), 1, 'Manual', GETUTCDATE(), GETUTCDATE())
   ```
3. Đợi job chạy (1 phút) hoặc trigger thủ công
4. Kiểm tra:
   - Log: `Đã tự động xác nhận thanh toán cho đơn hàng #X`
   - Database: Order.IsPaid = true, OrderStatus = Confirmed
   - Inventory: Quantity và ReservedQuantity đã được cập nhật

**Kết quả mong đợi:**
- Job tìm thấy giao dịch và tự động xác nhận
- Đơn hàng chuyển sang trạng thái Confirmed
- Hàng được xuất kho (giảm Quantity)

---

### Test 4: Test với nhiều đơn hàng

**Mục đích**: Kiểm tra job có xử lý được nhiều đơn hàng cùng lúc không

**Các bước:**
1. Tạo 3-5 đơn hàng test (tất cả đều Pending, chưa thanh toán)
2. Tạo giao dịch cho 2 đơn hàng đầu
3. Trigger job thủ công
4. Kiểm tra:
   - Log hiển thị số đơn hàng được xử lý
   - 2 đơn hàng có giao dịch được xác nhận
   - 3 đơn hàng còn lại vẫn ở trạng thái Pending

**Kết quả mong đợi:**
- Job xử lý tất cả đơn hàng
- Chỉ đơn hàng có giao dịch khớp được xác nhận

---

## 4. CÁCH XEM LOG TRONG VISUAL STUDIO

### 4.1. Output Window

1. Mở **Output** window:
   - Menu: `View` → `Output`
   - Hoặc nhấn: `Ctrl+Alt+O`
2. Chọn output source:
   - Dropdown: **Show output from**
   - Chọn: `Debug` hoặc tên project (ví dụ: `MyProject.Web.Mvc`)
3. Tìm kiếm:
   - Nhấn `Ctrl+F` để mở Find
   - Tìm: `PaymentVerificationBackgroundJob` hoặc `Bắt đầu kiểm tra`

### 4.2. Log File

1. Tìm file log:
   - Trong project: `App_Data/Logs/Logs.txt`
   - Sau khi build: `bin/Debug/net7.0/App_Data/Logs/Logs.txt`
2. Mở file bằng text editor (Notepad++, VS Code, etc.)
3. Tìm kiếm:
   - `PaymentVerificationBackgroundJob`
   - `Bắt đầu kiểm tra thanh toán`
   - `Đã tự động xác nhận thanh toán`

### 4.3. Debug với Breakpoint

1. Đặt breakpoint trong `PaymentVerificationBackgroundJob.cs`:
   - Dòng 43: `var pendingOrders = await _orderAppService.GetPendingUnpaidOrdersAsync();`
   - Dòng 57: `var verificationResult = await _paymentVerificationService.VerifyPaymentAsync(...)`
2. Chạy ứng dụng ở chế độ **Debug** (F5)
3. Đợi 1 phút hoặc trigger job thủ công
4. Debug từng bước để xem logic

---

## 5. CÁCH TEST NHANH (5 PHÚT)

### Bước 1: Tạo đơn hàng test
1. Đăng nhập → Thêm sản phẩm vào giỏ → Checkout
2. Ghi nhớ `PaymentReference` (ví dụ: `MP20241220120000`)

### Bước 2: Tạo giao dịch giả trong database
```sql
-- Lấy OrderId từ bảng AppOrders dựa vào PaymentReference
DECLARE @OrderId INT = (SELECT Id FROM AppOrders WHERE PaymentReference = 'MP20241220120000')
DECLARE @Amount DECIMAL = (SELECT totalAmount FROM AppOrders WHERE Id = @OrderId)

-- Tạo PaymentTransaction
INSERT INTO AppPaymentTransactions 
(OrderId, PaymentReference, Amount, BankCode, BankAccount, TransactionId, TransactionTime, Status, VerifiedBy, VerifiedAt, CreationTime)
VALUES 
(@OrderId, 'MP20241220120000', @Amount, 'VCB', '123456789', 'TXN' + CAST(NEWID() AS VARCHAR(36)), GETUTCDATE(), 1, 'Manual', GETUTCDATE(), GETUTCDATE())
```

### Bước 3: Trigger job thủ công
- Truy cập: `http://localhost:XXXX/TestBackgroundJob/TriggerJob`
- Hoặc đợi 1 phút để job tự động chạy

### Bước 4: Kiểm tra kết quả
1. Kiểm tra log: Có message `Đã tự động xác nhận thanh toán`
2. Kiểm tra database:
   ```sql
   SELECT Id, OrderStatus, IsPaid, PaidTime 
   FROM AppOrders 
   WHERE PaymentReference = 'MP20241220120000'
   ```
   - Kết quả mong đợi: `OrderStatus = 1` (Confirmed), `IsPaid = 1`

---

## 6. TROUBLESHOOTING

### Vấn đề: Job không chạy

**Nguyên nhân có thể:**
1. Background Jobs bị tắt
   - **Kiểm tra**: `MyProjectApplicationModule.cs` có `Configuration.BackgroundJobs.IsJobExecutionEnabled = true;` không?
2. Job chưa được đăng ký
   - **Kiểm tra**: Class có implement `ISingletonDependency` không?
   - **Kiểm tra**: Có kế thừa `AsyncPeriodicBackgroundWorkerBase` không?
3. Ứng dụng chưa khởi động đầy đủ
   - **Giải pháp**: Restart ứng dụng

**Cách kiểm tra:**
- Xem log khi khởi động có lỗi không
- Dùng test endpoint `/TestBackgroundJob/GetJobStatus`

---

### Vấn đề: Job chạy nhưng không tìm thấy giao dịch

**Nguyên nhân có thể:**
1. Chưa có PaymentTransaction trong database
2. PaymentReference không khớp
3. Amount không khớp
4. TransactionTime nằm ngoài khoảng thời gian kiểm tra

**Cách kiểm tra:**
- Dùng test endpoint `/TestBackgroundJob/GetPendingOrders` để xem đơn hàng
- Kiểm tra database xem có PaymentTransaction không:
  ```sql
  SELECT * FROM AppPaymentTransactions WHERE Status = 1
  ```

---

### Vấn đề: Job chạy nhưng không xác nhận đơn hàng

**Nguyên nhân có thể:**
1. Lỗi khi commit inventory
2. Lỗi khi cập nhật order
3. Đơn hàng đã được xác nhận trước đó

**Cách kiểm tra:**
- Xem log chi tiết để tìm lỗi
- Kiểm tra exception trong log

---

## 7. MONITORING VÀ LOGGING

### Log Messages để theo dõi:

1. **Job bắt đầu chạy:**
   ```
   INFO - Bắt đầu kiểm tra thanh toán cho X đơn hàng
   ```

2. **Tìm thấy giao dịch và xác nhận:**
   ```
   INFO - Đã tự động xác nhận thanh toán cho đơn hàng #X (PaymentReference: MP...)
   ```

3. **Lỗi khi xử lý:**
   ```
   ERROR - Lỗi khi kiểm tra thanh toán cho đơn hàng #X: [Error Message]
   ```

4. **Lỗi tổng thể:**
   ```
   ERROR - Lỗi trong PaymentVerificationBackgroundJob: [Error Message]
   ```

---

## 8. TIPS VÀ BEST PRACTICES

1. **Test trong Development trước**: Luôn test kỹ trước khi deploy production
2. **Monitor log thường xuyên**: Kiểm tra log để đảm bảo job chạy ổn định
3. **Test với dữ liệu thật**: Tạo đơn hàng và giao dịch thật để test
4. **Kiểm tra performance**: Nếu có nhiều đơn hàng, job có thể chạy lâu
5. **Backup database**: Trước khi test, backup database để có thể restore nếu cần

---

## 9. CÁC LỆNH SQL HỮU ÍCH

### Xem đơn hàng Pending:
```sql
SELECT Id, PaymentReference, totalAmount, OrderStatus, IsPaid, CreationTime, PaymentExpiredAt
FROM AppOrders
WHERE OrderStatus = 0 AND IsPaid = 0
AND (PaymentExpiredAt IS NULL OR PaymentExpiredAt > GETUTCDATE())
```

### Xem giao dịch đã xác nhận:
```sql
SELECT pt.*, o.PaymentReference, o.totalAmount
FROM AppPaymentTransactions pt
INNER JOIN AppOrders o ON pt.OrderId = o.Id
WHERE pt.Status = 1
ORDER BY pt.VerifiedAt DESC
```

### Xem log gần đây (nếu lưu trong database):
```sql
-- Nếu dùng AbpAuditLogs
SELECT TOP 50 * 
FROM AbpAuditLogs 
WHERE ServiceName LIKE '%PaymentVerification%'
ORDER BY ExecutionTime DESC
```

---

**Chúc bạn test thành công!** 🎉
