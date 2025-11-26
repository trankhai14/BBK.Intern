# ĐỀ XUẤT CẢI TIẾN HỆ THỐNG THANH TOÁN

## TỔNG QUAN

Dựa trên tài liệu `PAYMENT_FLOW.md`, hệ thống hiện tại cần được cải tiến để:
1. Tự động kiểm tra và xác nhận thanh toán từ ngân hàng
2. Lưu trữ lịch sử giao dịch để đối chiếu
3. Cải thiện trải nghiệm người dùng với auto-check
4. Hỗ trợ webhook từ ngân hàng (nếu có)

---

## 1. CÁC THAY ĐỔI CẦN THỰC HIỆN

### 1.1. TẠO BẢNG LƯU LỊCH SỬ GIAO DỊCH

**Mục đích**: Lưu trữ thông tin giao dịch từ ngân hàng để đối chiếu

**Entity mới**: `PaymentTransaction`

```csharp
[Table("AppPaymentTransactions")]
public class PaymentTransaction : FullAuditedEntity<int>
{
    public int OrderId { get; set; }
    public string PaymentReference { get; set; }
    public decimal Amount { get; set; }
    public string BankCode { get; set; }
    public string BankAccount { get; set; }
    public string TransactionId { get; set; } // Mã giao dịch từ ngân hàng
    public DateTime TransactionTime { get; set; }
    public string Content { get; set; } // Nội dung chuyển khoản
    public PaymentTransactionStatus Status { get; set; } // Pending, Verified, Failed
    public string VerifiedBy { get; set; } // System, Manual, Webhook
    public DateTime? VerifiedAt { get; set; }
    public string Notes { get; set; }
    
    [ForeignKey("OrderId")]
    public Order Order { get; set; }
}

public enum PaymentTransactionStatus : int
{
    Pending = 0,    // Chờ xác nhận
    Verified = 1,   // Đã xác nhận
    Failed = 2,    // Không khớp/Thất bại
    Expired = 3    // Hết hạn
}
```

**Migration**: Tạo migration để thêm bảng `AppPaymentTransactions`

---

### 1.2. TẠO SERVICE KIỂM TRA GIAO DỊCH

**Mục đích**: Kiểm tra xem có giao dịch nào từ ngân hàng khớp với PaymentReference không

**Service mới**: `IPaymentVerificationService`

```csharp
public interface IPaymentVerificationService
{
    /// <summary>
    /// Kiểm tra giao dịch từ ngân hàng (qua API hoặc file statement)
    /// </summary>
    Task<PaymentVerificationResult> VerifyPaymentAsync(string paymentReference, decimal expectedAmount, DateTime fromDate, DateTime toDate);
    
    /// <summary>
    /// Lưu giao dịch vào database
    /// </summary>
    Task<PaymentTransaction> SaveTransactionAsync(int orderId, PaymentTransactionDto transaction);
    
    /// <summary>
    /// Tự động xác nhận thanh toán nếu tìm thấy giao dịch khớp
    /// </summary>
    Task<bool> AutoConfirmPaymentAsync(int orderId);
}

public class PaymentVerificationResult
{
    public bool IsVerified { get; set; }
    public PaymentTransactionDto Transaction { get; set; }
    public string Message { get; set; }
    public VerificationMethod Method { get; set; } // API, File, Manual
}

public enum VerificationMethod
{
    API = 1,      // Tích hợp API ngân hàng
    File = 2,     // Import file statement
    Manual = 3,   // Nhân viên xác nhận thủ công
    Webhook = 4   // Callback từ ngân hàng
}
```

**Các phương thức kiểm tra**:

1. **Kiểm tra qua API ngân hàng** (nếu có):
   - Gọi API của ngân hàng để lấy danh sách giao dịch
   - So khớp theo PaymentReference và Amount
   - Lưu TransactionId từ ngân hàng

2. **Kiểm tra qua file statement** (tạm thời):
   - Import file Excel/CSV từ ngân hàng
   - Parse và so khớp giao dịch
   - Lưu vào database

3. **Kiểm tra thủ công** (fallback):
   - Nhân viên xem statement và xác nhận thủ công
   - Cập nhật trạng thái qua admin panel

---

### 1.3. CẢI THIỆN `ConfirmPaid` METHOD

**File**: `CheckoutController.cs`

**Thay đổi**: Thêm logic kiểm tra giao dịch trước khi xác nhận

```csharp
[HttpPost]
public async Task<JsonResult> ConfirmPaid([FromBody] ConfirmPaidInput input)
{
    // ... validation hiện tại ...
    
    var order = await _orderAppService.GetOrderById(input.OrderId);
    
    // Kiểm tra xem đã có giao dịch được xác nhận chưa
    var existingTransaction = await _paymentVerificationService.GetVerifiedTransactionAsync(order.Id);
    if (existingTransaction != null && existingTransaction.Status == PaymentTransactionStatus.Verified)
    {
        // Đã được xác nhận tự động, chỉ cần redirect
        return Json(new { 
            success = true, 
            redirectUrl = Url.Action(nameof(Success), new { orderCode = order.PaymentReference }),
            message = "Thanh toán đã được xác nhận tự động"
        });
    }
    
    // Nếu chưa có giao dịch được xác nhận, thử kiểm tra lại
    var verificationResult = await _paymentVerificationService.VerifyPaymentAsync(
        order.PaymentReference, 
        order.TotalAmount,
        order.CreationTime.AddMinutes(-5), // Kiểm tra từ 5 phút trước khi tạo đơn
        DateTime.UtcNow
    );
    
    if (!verificationResult.IsVerified)
    {
        // Chưa tìm thấy giao dịch, yêu cầu khách hàng kiểm tra lại
        return Json(new { 
            success = false, 
            message = "Chưa tìm thấy giao dịch thanh toán. Vui lòng kiểm tra lại hoặc liên hệ hỗ trợ.",
            canRetry = true
        });
    }
    
    // Đã tìm thấy giao dịch, lưu vào database
    await _paymentVerificationService.SaveTransactionAsync(order.Id, verificationResult.Transaction);
    
    // Xác nhận thanh toán
    try
    {
        var details = await _orderDetailAppService.GetOrderListById(order.Id);
        foreach (var detail in details)
        {
            await _inventoryAppService.CommitReservedInventory(detail.ProductId, detail.Quantity);
        }

        await _orderAppService.UpdateOrder(new UpdateOrderDto
        {
            OrderId = order.Id,
            OrderStatus = (int)OrderStatus.Confirmed,
            PaymentMethod = order.PaymentMethod,
            IsPaid = true,
            PaidTime = verificationResult.Transaction.TransactionTime
        });
        
        // Cập nhật trạng thái transaction
        await _paymentVerificationService.MarkTransactionAsVerifiedAsync(order.Id, VerificationMethod.Manual);
    }
    catch (Exception ex)
    {
        Logger.Error("Không thể xác nhận thanh toán", ex);
        return Json(new { success = false, message = "Không thể cập nhật trạng thái thanh toán. Vui lòng liên hệ hỗ trợ." });
    }

    return Json(new
    {
        success = true,
        redirectUrl = Url.Action(nameof(Success), new { orderCode = order.PaymentReference })
    });
}
```

---

### 1.4. THÊM WEBHOOK ENDPOINT

**Mục đích**: Nhận callback từ ngân hàng khi có giao dịch mới

**File**: `CheckoutController.cs`

```csharp
/// <summary>
/// Webhook nhận callback từ ngân hàng khi có giao dịch
/// </summary>
[HttpPost]
[AllowAnonymous] // Hoặc dùng API Key authentication
[Route("api/payment/webhook")]
public async Task<IActionResult> PaymentWebhook([FromBody] BankWebhookDto webhookData)
{
    try
    {
        // Validate webhook signature (nếu ngân hàng hỗ trợ)
        if (!ValidateWebhookSignature(webhookData))
        {
            Logger.Warn("Webhook signature không hợp lệ");
            return BadRequest("Invalid signature");
        }
        
        // Tìm đơn hàng theo PaymentReference
        var order = await _orderAppService.GetOrderByPaymentReference(webhookData.PaymentReference);
        if (order == null)
        {
            Logger.Warn($"Không tìm thấy đơn hàng với PaymentReference: {webhookData.PaymentReference}");
            return NotFound("Order not found");
        }
        
        // Kiểm tra số tiền có khớp không
        if (webhookData.Amount != order.TotalAmount)
        {
            Logger.Warn($"Số tiền không khớp. Expected: {order.TotalAmount}, Received: {webhookData.Amount}");
            return BadRequest("Amount mismatch");
        }
        
        // Lưu giao dịch
        var transaction = await _paymentVerificationService.SaveTransactionAsync(order.Id, new PaymentTransactionDto
        {
            PaymentReference = webhookData.PaymentReference,
            Amount = webhookData.Amount,
            BankCode = webhookData.BankCode,
            BankAccount = webhookData.BankAccount,
            TransactionId = webhookData.TransactionId,
            TransactionTime = webhookData.TransactionTime,
            Content = webhookData.Content,
            Status = PaymentTransactionStatus.Verified,
            VerifiedBy = "Webhook",
            VerifiedAt = DateTime.UtcNow
        });
        
        // Tự động xác nhận thanh toán
        if (!order.IsPaid)
        {
            var details = await _orderDetailAppService.GetOrderListById(order.Id);
            foreach (var detail in details)
            {
                await _inventoryAppService.CommitReservedInventory(detail.ProductId, detail.Quantity);
            }

            await _orderAppService.UpdateOrder(new UpdateOrderDto
            {
                OrderId = order.Id,
                OrderStatus = (int)OrderStatus.Confirmed,
                PaymentMethod = order.PaymentMethod,
                IsPaid = true,
                PaidTime = webhookData.TransactionTime
            });
            
            Logger.Info($"Đã tự động xác nhận thanh toán cho đơn hàng #{order.Id} qua webhook");
        }
        
        return Ok(new { success = true, message = "Payment verified" });
    }
    catch (Exception ex)
    {
        Logger.Error("Lỗi khi xử lý webhook", ex);
        return StatusCode(500, "Internal server error");
    }
}

private bool ValidateWebhookSignature(BankWebhookDto webhookData)
{
    // Implement signature validation logic
    // Ví dụ: HMAC SHA256 với secret key
    return true; // Tạm thời return true, cần implement thực tế
}
```

**DTO cho Webhook**:

```csharp
public class BankWebhookDto
{
    public string PaymentReference { get; set; }
    public decimal Amount { get; set; }
    public string BankCode { get; set; }
    public string BankAccount { get; set; }
    public string TransactionId { get; set; }
    public DateTime TransactionTime { get; set; }
    public string Content { get; set; }
    public string Signature { get; set; } // Để validate
}
```

---

### 1.5. CẢI THIỆN UI - AUTO CHECK PAYMENT

**File**: `Payment.js`

**Thay đổi**: Thêm auto-check định kỳ và cải thiện UX

```javascript
(function () {
    var checkInterval = null;
    var orderId = null;
    var paymentReference = null;
    
    $(function () {
        // Lấy orderId và paymentReference từ view
        orderId = $('#btnPaid').data('order-id');
        paymentReference = $('#paymentReference').text();
        
        // Auto-check mỗi 10 giây (nếu chưa thanh toán)
        if (orderId && !$('#paymentStatus').data('is-paid')) {
            startAutoCheck();
        }
        
        // Nút "Đã thanh toán" thủ công
        $('#btnPaid').on('click', function (e) {
            e.preventDefault();
            confirmPayment();
        });
        
        // Dừng auto-check khi rời trang
        $(window).on('beforeunload', function() {
            stopAutoCheck();
        });
    });
    
    function startAutoCheck() {
        // Kiểm tra mỗi 10 giây
        checkInterval = setInterval(function() {
            checkPaymentStatus();
        }, 10000); // 10 giây
        
        // Kiểm tra ngay lần đầu
        checkPaymentStatus();
    }
    
    function stopAutoCheck() {
        if (checkInterval) {
            clearInterval(checkInterval);
            checkInterval = null;
        }
    }
    
    function checkPaymentStatus() {
        if (!orderId) return;
        
        abp.ajax({
            url: '/Checkout/CheckPaymentStatus',
            type: 'GET',
            data: { orderId: orderId },
            dataType: 'json'
        }).done(function (response) {
            if (response && response.isPaid) {
                // Đã thanh toán, dừng auto-check và redirect
                stopAutoCheck();
                showSuccessMessage('Thanh toán đã được xác nhận!');
                setTimeout(function() {
                    window.location.href = response.redirectUrl || '/Checkout/Success?orderCode=' + paymentReference;
                }, 2000);
            } else if (response && response.hasTransaction) {
                // Có giao dịch nhưng chưa xác nhận, hiển thị thông báo
                showInfoMessage('Đã phát hiện giao dịch. Đang xác nhận...');
                // Tự động xác nhận
                confirmPayment();
            }
        }).fail(function() {
            // Lỗi khi check, không làm gì (sẽ thử lại lần sau)
        });
    }
    
    function confirmPayment() {
        if (!orderId) {
            abp.notify.error('Không xác định được đơn hàng.');
            return;
        }
        
        abp.ui.setBusy($('body'));
        
        abp.ajax({
            url: '/Checkout/ConfirmPaid',
            type: 'POST',
            data: JSON.stringify({ orderId: orderId }),
            contentType: 'application/json',
            dataType: 'json'
        }).done(function (response) {
            if (response && response.success) {
                stopAutoCheck();
                showSuccessMessage(response.message || 'Xác nhận thanh toán thành công!');
                
                if (response.redirectUrl) {
                    setTimeout(function() {
                        window.location.href = response.redirectUrl;
                    }, 1500);
                } else {
                    window.location.href = '/Checkout/Success';
                }
            } else {
                var message = response && response.message 
                    ? response.message 
                    : 'Chưa tìm thấy giao dịch thanh toán. Vui lòng kiểm tra lại hoặc liên hệ hỗ trợ.';
                
                if (response && response.canRetry) {
                    showWarningMessage(message + ' Hệ thống sẽ tiếp tục kiểm tra tự động...');
                    // Tiếp tục auto-check
                } else {
                    abp.notify.error(message);
                }
            }
        }).always(function () {
            abp.ui.clearBusy($('body'));
        });
    }
    
    function showSuccessMessage(message) {
        abp.notify.success(message, 'Thành công');
    }
    
    function showInfoMessage(message) {
        abp.notify.info(message, 'Thông tin');
    }
    
    function showWarningMessage(message) {
        abp.notify.warn(message, 'Cảnh báo');
    }
})();
```

**Thêm endpoint mới**: `CheckPaymentStatus`

```csharp
[HttpGet]
public async Task<JsonResult> CheckPaymentStatus(int orderId)
{
    if (!AbpSession.UserId.HasValue)
    {
        return Json(new { isPaid = false });
    }
    
    var order = await _orderAppService.GetOrderById(orderId);
    if (order == null || order.UserId != AbpSession.UserId.Value)
    {
        return Json(new { isPaid = false });
    }
    
    // Kiểm tra xem có giao dịch đã được xác nhận chưa
    var transaction = await _paymentVerificationService.GetVerifiedTransactionAsync(orderId);
    var hasTransaction = transaction != null;
    
    return Json(new
    {
        isPaid = order.IsPaid,
        hasTransaction = hasTransaction,
        redirectUrl = order.IsPaid 
            ? Url.Action(nameof(Success), new { orderCode = order.PaymentReference })
            : null
    });
}
```

---

### 1.6. CẢI THIỆN VIEW - HIỂN THỊ TRẠNG THÁI RÕ RÀNG

**File**: `Payment.cshtml`

**Thay đổi**: Thêm các element để hiển thị trạng thái

```html
<!-- Thêm vào view -->
<div id="paymentStatus" data-is-paid="@(Model.Order.IsPaid ? "true" : "false")">
    @if (Model.Order.IsPaid)
    {
        <div class="alert alert-success">
            <i class="fas fa-check-circle"></i> Đơn hàng đã được thanh toán thành công!
        </div>
    }
    else
    {
        <div class="alert alert-info" id="paymentStatusAlert">
            <i class="fas fa-clock"></i> Đang chờ thanh toán...
            <br>
            <small>Hệ thống sẽ tự động kiểm tra thanh toán mỗi 10 giây</small>
        </div>
    }
</div>

<!-- Thêm loading indicator -->
<div id="paymentChecking" style="display: none;">
    <div class="text-center">
        <i class="fas fa-spinner fa-spin"></i> Đang kiểm tra thanh toán...
    </div>
</div>
```

---

### 1.7. THÊM BACKGROUND JOB - TỰ ĐỘNG KIỂM TRA GIAO DỊCH

**Mục đích**: Tự động kiểm tra giao dịch cho các đơn hàng chưa thanh toán

**File mới**: `PaymentVerificationBackgroundJob.cs`

```csharp
public class PaymentVerificationBackgroundJob : AsyncPeriodicBackgroundWorkerBase
{
    private readonly IPaymentVerificationService _paymentVerificationService;
    private readonly IOrderAppService _orderAppService;
    private readonly IOrderDetailAppService _orderDetailAppService;
    private readonly IInventoryAppService _inventoryAppService;
    
    public PaymentVerificationBackgroundJob(
        AbpAsyncTimer timer,
        IServiceScopeFactory serviceScopeFactory,
        IPaymentVerificationService paymentVerificationService,
        IOrderAppService orderAppService,
        IOrderDetailAppService orderDetailAppService,
        IInventoryAppService inventoryAppService)
        : base(timer, serviceScopeFactory)
    {
        _paymentVerificationService = paymentVerificationService;
        _orderAppService = orderAppService;
        _orderDetailAppService = orderDetailAppService;
        _inventoryAppService = inventoryAppService;
        
        Timer.Period = 60000; // Chạy mỗi 1 phút
    }
    
    protected override async Task DoWorkAsync()
    {
        // Tìm các đơn hàng Pending chưa thanh toán và chưa hết hạn
        var pendingOrders = await _orderAppService.GetPendingUnpaidOrdersAsync();
        
        foreach (var order in pendingOrders)
        {
            try
            {
                // Kiểm tra xem có giao dịch khớp không
                var verificationResult = await _paymentVerificationService.VerifyPaymentAsync(
                    order.PaymentReference,
                    order.TotalAmount,
                    order.CreationTime.AddMinutes(-5),
                    DateTime.UtcNow
                );
                
                if (verificationResult.IsVerified)
                {
                    // Tìm thấy giao dịch, tự động xác nhận
                    await _paymentVerificationService.AutoConfirmPaymentAsync(order.Id);
                    Logger.Info($"Đã tự động xác nhận thanh toán cho đơn hàng #{order.Id}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Lỗi khi kiểm tra thanh toán cho đơn hàng #{order.Id}", ex);
            }
        }
    }
}
```

**Thêm method vào OrderAppService**:

```csharp
public async Task<List<OrderListDto>> GetPendingUnpaidOrdersAsync()
{
    var orders = await _orderAppService.GetAllListAsync(o =>
        o.OrderStatus == (int)OrderStatus.Pending &&
        !o.IsPaid &&
        (!o.PaymentExpiredAt.HasValue || o.PaymentExpiredAt.Value > DateTime.UtcNow));
    
    return orders.Select(o => new OrderListDto { /* map */ }).ToList();
}
```

**Đăng ký Background Job** (trong Startup hoặc Module):

```csharp
Configuration.BackgroundJobs.IsJobExecutionEnabled = true;
```

---

### 1.8. THÊM ADMIN PANEL - QUẢN LÝ GIAO DỊCH

**Mục đích**: Cho phép nhân viên xem và xác nhận giao dịch thủ công

**Controller mới**: `PaymentTransactionsController.cs`

```csharp
[AbpMvcAuthorize]
public class PaymentTransactionsController : MyProjectControllerBase
{
    private readonly IPaymentVerificationService _paymentVerificationService;
    
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // Hiển thị danh sách giao dịch
        return View();
    }
    
    [HttpPost]
    public async Task<JsonResult> VerifyTransaction(int transactionId)
    {
        // Xác nhận giao dịch thủ công
        var result = await _paymentVerificationService.VerifyTransactionManuallyAsync(transactionId);
        return Json(result);
    }
}
```

---

## 2. CẤU TRÚC THƯ MỤC MỚI

```
MyProject.Core/
  └── Payments/
      ├── PaymentTransaction.cs
      └── PaymentTransactionStatus.cs

MyProject.Application/
  └── Payments/
      ├── IPaymentVerificationService.cs
      ├── PaymentVerificationService.cs
      ├── Dtos/
      │   ├── PaymentTransactionDto.cs
      │   ├── PaymentVerificationResult.cs
      │   └── BankWebhookDto.cs
      └── BackgroundJobs/
          └── PaymentVerificationBackgroundJob.cs

MyProject.Web.Mvc/
  └── Controllers/
      └── PaymentTransactionsController.cs
```

---

## 3. CẤU HÌNH APP.SETTINGS

**Thêm vào `appsettings.json`**:

```json
{
  "Payment": {
    "BankCode": "VCB",
    "BankAccount": "123456789",
    "BankAccountName": "CONG TY DEMO",
    "QrDescriptionTemplate": "TT {0}",
    "AutoCheckInterval": 10000,
    "WebhookSecret": "your-webhook-secret-key",
    "BankApi": {
      "Enabled": false,
      "BaseUrl": "",
      "ApiKey": "",
      "ApiSecret": ""
    }
  }
}
```

---

## 4. MIGRATION

**Tạo migration mới**:

```bash
dotnet ef migrations add AddPaymentTransactions
```

**Nội dung migration**:

```csharp
public partial class AddPaymentTransactions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AppPaymentTransactions",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                OrderId = table.Column<int>(nullable: false),
                PaymentReference = table.Column<string>(maxLength: 100, nullable: true),
                Amount = table.Column<decimal>(nullable: false),
                BankCode = table.Column<string>(maxLength: 50, nullable: true),
                BankAccount = table.Column<string>(maxLength: 50, nullable: true),
                TransactionId = table.Column<string>(maxLength: 100, nullable: true),
                TransactionTime = table.Column<DateTime>(nullable: false),
                Content = table.Column<string>(maxLength: 500, nullable: true),
                Status = table.Column<int>(nullable: false),
                VerifiedBy = table.Column<string>(maxLength: 50, nullable: true),
                VerifiedAt = table.Column<DateTime>(nullable: true),
                Notes = table.Column<string>(maxLength: 1000, nullable: true),
                CreationTime = table.Column<DateTime>(nullable: false),
                CreatorUserId = table.Column<long>(nullable: true),
                LastModificationTime = table.Column<DateTime>(nullable: true),
                LastModifierUserId = table.Column<long>(nullable: true),
                IsDeleted = table.Column<bool>(nullable: false),
                DeletionTime = table.Column<DateTime>(nullable: true),
                DeleterUserId = table.Column<long>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppPaymentTransactions", x => x.Id);
                table.ForeignKey(
                    name: "FK_AppPaymentTransactions_AppOrders_OrderId",
                    column: x => x.OrderId,
                    principalTable: "AppOrders",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AppPaymentTransactions_OrderId",
            table: "AppPaymentTransactions",
            column: "OrderId");

        migrationBuilder.CreateIndex(
            name: "IX_AppPaymentTransactions_PaymentReference",
            table: "AppPaymentTransactions",
            column: "PaymentReference");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "AppPaymentTransactions");
    }
}
```

---

## 5. THỨ TỰ TRIỂN KHAI

### Phase 1: Cơ bản (Ưu tiên cao)
1. ✅ Tạo bảng `PaymentTransaction`
2. ✅ Tạo `PaymentVerificationService` (với method kiểm tra thủ công)
3. ✅ Cải thiện `ConfirmPaid` với logic kiểm tra
4. ✅ Cải thiện UI với auto-check

### Phase 2: Tự động hóa (Ưu tiên trung bình)
5. ✅ Thêm Background Job tự động kiểm tra
6. ✅ Thêm Webhook endpoint (nếu ngân hàng hỗ trợ)
7. ✅ Tích hợp API ngân hàng (nếu có)

### Phase 3: Quản lý (Ưu tiên thấp)
8. ✅ Admin panel quản lý giao dịch
9. ✅ Import file statement từ ngân hàng
10. ✅ Báo cáo và thống kê giao dịch

---

## 6. LƯU Ý QUAN TRỌNG

1. **Bảo mật Webhook**: Phải validate signature để tránh fake request
2. **Rate Limiting**: Giới hạn số lần kiểm tra để tránh spam API
3. **Error Handling**: Xử lý lỗi kỹ lưỡng, không để mất dữ liệu
4. **Logging**: Log đầy đủ các giao dịch để audit
5. **Testing**: Test kỹ các trường hợp edge case

---

## 7. TESTING SCENARIOS

1. **Test auto-check**: Đơn hàng được tự động xác nhận khi có giao dịch
2. **Test manual confirm**: Khách hàng click "Đã thanh toán" sau khi chuyển khoản
3. **Test webhook**: Webhook từ ngân hàng tự động xác nhận
4. **Test expired order**: Đơn hàng hết hạn được hủy tự động
5. **Test duplicate payment**: Xử lý khi có nhiều giao dịch khớp
6. **Test amount mismatch**: Xử lý khi số tiền không khớp

---

**Tài liệu này mô tả chi tiết các thay đổi cần thiết để cải tiến hệ thống thanh toán theo đúng luồng đã mô tả trong PAYMENT_FLOW.md**








