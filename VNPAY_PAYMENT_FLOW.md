# HƯỚNG DẪN ĐẶT HÀNG VÀ THANH TOÁN BẰNG VNPAY

## 📋 TỔNG QUAN

Tài liệu này mô tả chi tiết quy trình đặt hàng và thanh toán bằng VNPay trong hệ thống.

---

## 🔄 LUỒNG THANH TOÁN VNPAY

```
┌─────────────┐
│  Khách hàng │
└──────┬──────┘
       │
       │ 1. Chọn sản phẩm → Thêm vào giỏ hàng
       ▼
┌─────────────────┐
│  Trang Checkout │
│   (Confirm)     │
└──────┬──────────┘
       │
       │ 2. Điền thông tin + Chọn "Thanh toán VNPay"
       │
       │ 3. Click "Đặt hàng"
       ▼
┌─────────────────────┐
│ CheckoutController   │
│   Payment (POST)     │
└──────┬──────────────┘
       │
       │ 4. Tạo đơn hàng (Status: Pending)
       │ 5. Reserve Inventory
       │ 6. Tạo Payment URL từ VNPayService
       │
       ▼
┌─────────────────────┐
│   VNPayService      │
│ CreatePaymentUrl()  │
└──────┬──────────────┘
       │
       │ 7. Tạo chữ ký HMAC SHA512
       │ 8. Tạo Payment URL
       │
       ▼
┌─────────────────────┐
│   Redirect đến      │
│   VNPay Gateway     │
└──────┬──────────────┘
       │
       │ 9. Khách hàng thanh toán trên VNPay
       │    (Thẻ ATM, Visa, Mastercard, Ví điện tử)
       │
       ▼
┌─────────────────────┐
│   VNPay xử lý       │
│   thanh toán        │
└──────┬──────────────┘
       │
       ├─────────────────┐
       │                 │
       ▼                 ▼
┌──────────────┐  ┌──────────────┐
│ Return URL   │  │   IPN URL    │
│ (Redirect)   │  │  (Webhook)   │
└──────┬───────┘  └──────┬───────┘
       │                 │
       │ 10. Redirect    │ 11. Gửi IPN
       │     về website  │     (async)
       │                 │
       ▼                 ▼
┌─────────────────────┐
│ VnPayReturn (GET)   │
│ VnPayIPN (POST)     │
└──────┬──────────────┘
       │
       │ 12. Xác thực chữ ký
       │ 13. Kiểm tra số tiền
       │ 14. Cập nhật đơn hàng
       │     - IsPaid = true
       │     - OrderStatus = Confirmed
       │     - Commit Inventory
       │
       ▼
┌─────────────────────┐
│  Trang Success      │
│  (Thanh toán thành  │
│   công)             │
└─────────────────────┘
```

---

## 📝 CÁC BƯỚC CHI TIẾT

### **BƯỚC 1: Khách hàng chọn sản phẩm và vào trang Checkout**

1. Khách hàng thêm sản phẩm vào giỏ hàng
2. Click "Thanh toán" → Chuyển đến `/Checkout/Confirm`
3. Trang hiển thị:
   - Form thông tin khách hàng
   - Tóm tắt đơn hàng
   - **Phương thức thanh toán** (dropdown)

### **BƯỚC 2: Khách hàng chọn "Thanh toán VNPay"**

Trong dropdown "Phương thức thanh toán":
```html
<select name="PaymentMethod" class="form-control">
    <option value="QR">Chuyển khoản qua QR</option>
    <option value="VNPay">Thanh toán VNPay (Thẻ ATM, Visa, Mastercard, Ví điện tử)</option>
</select>
```

**Chọn:** `VNPay`

### **BƯỚC 3: Khách hàng điền thông tin và click "Đặt hàng"**

Form submit với:
- `FullName`: Tên khách hàng
- `PhoneNumber`: Số điện thoại
- `Address`: Địa chỉ giao hàng
- `PaymentMethod`: `"VNPay"` ← **Quan trọng**
- `Note`: Ghi chú (optional)

### **BƯỚC 4: CheckoutController.Payment() xử lý**

**File:** `CheckoutController.cs`

```csharp
[HttpPost]
public async Task<IActionResult> Payment(CheckoutConfirmViewModel input)
{
    // 1. Kiểm tra giỏ hàng
    var summary = await BuildCartSummaryAsync();
    
    // 2. Reserve Inventory (giữ hàng)
    foreach (var item in summary.Items)
    {
        await _inventoryAppService.ReserveInventory(item.ProductId, item.Quantity);
    }
    
    // 3. Tạo đơn hàng
    var paymentReference = $"MP{DateTime.UtcNow:yyyyMMddHHmmssfff}";
    var createOrderDto = new CreateOrderDto
    {
        PaymentMethod = input.PaymentMethod == "VNPay" 
            ? (int)CheckoutPaymentMethod.VNPay 
            : (int)CheckoutPaymentMethod.QRTransfer,
        PaymentReference = paymentReference,
        OrderStatus = (int)OrderStatus.Pending,
        IsPaid = false,
        // ... các thông tin khác
    };
    var orderId = await _orderAppService.CreateOrder(createOrderDto);
    
    // 4. Kiểm tra phương thức thanh toán
    if (input.PaymentMethod == "VNPay")
    {
        // Tạo Payment URL từ VNPayService
        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
        var orderInfo = $"Thanh toan don hang {paymentReference}";
        var vnpayUrl = _vnPayService.CreatePaymentUrl(
            orderId,
            summary.Total,
            orderInfo,
            paymentReference,
            clientIp
        );
        
        // Redirect đến VNPay
        return Redirect(vnpayUrl);
    }
    
    // Nếu không phải VNPay, xử lý QR như bình thường
    // ...
}
```

### **BƯỚC 5: VNPayService.CreatePaymentUrl() tạo Payment URL**

**File:** `VNPayService.cs`

```csharp
public string CreatePaymentUrl(
    int orderId,
    decimal amount,
    string orderInfo,
    string paymentReference,
    string clientIp)
{
    // 1. Chuyển đổi số tiền (VNPay yêu cầu × 100)
    var vnpAmount = (long)(amount * 100);
    // Ví dụ: 10,000 VND → 1000000
    
    // 2. Tạo các tham số
    var vnpParams = new Dictionary<string, string>
    {
        { "vnp_Version", "2.1.0" },
        { "vnp_Command", "pay" },
        { "vnp_TmnCode", "NJJ0R8FS" }, // Từ appsettings.json
        { "vnp_Amount", "1000000" }, // Số tiền × 100
        { "vnp_CurrCode", "VND" },
        { "vnp_TxnRef", "MP20241220120000" }, // PaymentReference
        { "vnp_OrderInfo", "Thanh toan don hang MP20241220120000" },
        { "vnp_OrderType", "other" },
        { "vnp_Locale", "vn" },
        { "vnp_ReturnUrl", "https://localhost:44300/Checkout/VnPayReturn" },
        { "vnp_IpAddr", "192.168.1.1" }, // IP khách hàng
        { "vnp_CreateDate", "20241220120000" } // yyyyMMddHHmmss
    };
    
    // 3. Sắp xếp theo thứ tự alphabet
    var sortedParams = vnpParams.OrderBy(x => x.Key).ToList();
    
    // 4. Tạo query string
    var queryString = string.Join("&", sortedParams.Select(x => 
        $"{x.Key}={WebUtility.UrlEncode(x.Value)}"));
    // Kết quả: vnp_Amount=1000000&vnp_Command=pay&vnp_CreateDate=20241220120000&...
    
    // 5. Tạo chữ ký HMAC SHA512
    var secureHash = CreateSecureHash(queryString);
    // Sử dụng HashSecret từ appsettings.json
    
    // 6. Tạo Payment URL
    var paymentUrl = $"{_paymentUrl}?{queryString}&vnp_SecureHash={secureHash}";
    // https://sandbox.vnpayment.vn/paymentv2/vpcpay.html?vnp_Amount=1000000&...&vnp_SecureHash=ABC123...
    
    return paymentUrl;
}
```

**Ví dụ Payment URL:**
```
https://sandbox.vnpayment.vn/paymentv2/vpcpay.html?
vnp_Amount=1000000&
vnp_Command=pay&
vnp_CreateDate=20241220120000&
vnp_CurrCode=VND&
vnp_IpAddr=192.168.1.1&
vnp_Locale=vn&
vnp_OrderInfo=Thanh%20toan%20don%20hang%20MP20241220120000&
vnp_OrderType=other&
vnp_ReturnUrl=https%3A%2F%2Flocalhost%3A44300%2FCheckout%2FVnPayReturn&
vnp_TmnCode=NJJ0R8FS&
vnp_TxnRef=MP20241220120000&
vnp_Version=2.1.0&
vnp_SecureHash=A1B2C3D4E5F6...
```

### **BƯỚC 6: Khách hàng được redirect đến VNPay**

1. Browser tự động redirect đến VNPay Gateway
2. VNPay hiển thị form thanh toán với:
   - Số tiền
   - Mô tả đơn hàng
   - Các phương thức thanh toán:
     - Thẻ ATM nội địa
     - Thẻ tín dụng/ghi nợ quốc tế (Visa, Mastercard, JCB)
     - Ví điện tử (ZaloPay, Momo, ShopeePay)
     - Internet Banking

### **BƯỚC 7: Khách hàng thanh toán trên VNPay**

1. Chọn phương thức thanh toán
2. Nhập thông tin (số thẻ, mật khẩu, OTP, ...)
3. Xác nhận thanh toán

### **BƯỚC 8: VNPay xử lý và gửi kết quả**

VNPay sẽ:
1. **Redirect về Return URL** (synchronous) - Khách hàng thấy ngay
2. **Gửi IPN** (asynchronous) - Webhook để đảm bảo xử lý

### **BƯỚC 9: CheckoutController.VnPayReturn() xử lý redirect**

**File:** `CheckoutController.cs`

```csharp
[HttpGet]
[AllowAnonymous]
public async Task<IActionResult> VnPayReturn()
{
    // 1. Lấy query string từ VNPay
    var queryString = Request.QueryString.ToString();
    // ?vnp_Amount=1000000&vnp_ResponseCode=00&vnp_TxnRef=MP20241220120000&...
    
    // 2. Parse các tham số
    var vnpayParams = _vnPayService.ParseResponse(queryString.TrimStart('?'));
    
    // 3. Lấy các tham số quan trọng
    var vnp_TxnRef = vnpayParams["vnp_TxnRef"]; // MP20241220120000
    var vnp_ResponseCode = vnpayParams["vnp_ResponseCode"]; // "00" = thành công
    var vnp_SecureHash = vnpayParams["vnp_SecureHash"]; // Chữ ký
    var vnp_Amount = vnpayParams["vnp_Amount"]; // "1000000"
    
    // 4. Xác thực chữ ký
    if (!_vnPayService.ValidateSignature(vnpayParams, vnp_SecureHash))
    {
        // Chữ ký không hợp lệ → Có thể bị giả mạo
        TempData["PaymentError"] = "Chữ ký không hợp lệ";
        return RedirectToAction(nameof(Confirm));
    }
    
    // 5. Tìm đơn hàng
    var order = await _orderAppService.GetOrderByPaymentReference(vnp_TxnRef);
    
    // 6. Kiểm tra số tiền
    var orderAmount = (long)(order.TotalAmount * 100);
    var vnpayAmount = long.Parse(vnp_Amount);
    if (orderAmount != vnpayAmount)
    {
        // Số tiền không khớp → Có thể bị giả mạo
        TempData["PaymentError"] = "Số tiền không khớp";
        return RedirectToAction(nameof(Confirm));
    }
    
    // 7. Xử lý kết quả thanh toán
    if (_vnPayService.IsPaymentSuccess(vnp_ResponseCode)) // "00" = thành công
    {
        if (!order.IsPaid) // Tránh xử lý trùng lặp
        {
            // Lưu transaction
            var transactionDto = new PaymentTransactionDto
            {
                OrderId = order.Id,
                PaymentReference = vnp_TxnRef,
                Amount = order.TotalAmount,
                BankCode = "VNPay",
                TransactionId = vnpayParams["vnp_TransactionNo"],
                Status = PaymentTransactionStatus.Verified,
                VerifiedBy = "VNPay",
                VerifiedAt = DateTime.UtcNow
            };
            await _paymentVerificationService.SaveTransactionAsync(order.Id, transactionDto);
            
            // Tự động xác nhận thanh toán
            await _paymentVerificationService.AutoConfirmPaymentAsync(order.Id);
            // - IsPaid = true
            // - OrderStatus = Confirmed
            // - Commit Inventory (xuất hàng)
        }
        
        // Redirect đến trang thành công
        return RedirectToAction(nameof(Success), new { orderCode = vnp_TxnRef });
    }
    else
    {
        // Thanh toán thất bại
        var errorMessage = _vnPayService.GetResponseMessage(vnp_ResponseCode);
        TempData["PaymentError"] = $"Thanh toán thất bại: {errorMessage}";
        return RedirectToAction(nameof(Confirm));
    }
}
```

### **BƯỚC 10: CheckoutController.VnPayIPN() xử lý webhook (tùy chọn)**

IPN (Instant Payment Notification) là webhook từ VNPay để đảm bảo xử lý thanh toán ngay cả khi khách hàng đóng browser.

**File:** `CheckoutController.cs`

```csharp
[HttpPost]
[AllowAnonymous]
public async Task<IActionResult> VnPayIPN()
{
    // 1. Lấy dữ liệu từ form
    var form = await Request.ReadFormAsync();
    var vnpayParams = new Dictionary<string, string>();
    foreach (var key in form.Keys)
    {
        vnpayParams[key] = form[key].ToString();
    }
    
    // 2. Xác thực chữ ký (giống VnPayReturn)
    // 3. Kiểm tra số tiền (giống VnPayReturn)
    // 4. Xử lý thanh toán (giống VnPayReturn)
    
    // 5. Trả về JSON response cho VNPay
    return Json(new { RspCode = "00", Message = "Success" });
}
```

**Lưu ý:** IPN URL phải accessible từ internet. Nếu test local, dùng ngrok.

### **BƯỚC 11: Trang Success**

Sau khi thanh toán thành công, khách hàng được redirect đến:
```
/Checkout/Success?orderCode=MP20241220120000
```

Trang hiển thị:
- Thông báo "Thanh toán thành công"
- Mã đơn hàng
- Thông tin đơn hàng
- Link xem chi tiết đơn hàng

---

## 🔐 BẢO MẬT

### **1. Xác thực chữ ký**

**QUAN TRỌNG:** Luôn xác thực chữ ký trước khi xử lý thanh toán.

```csharp
if (!_vnPayService.ValidateSignature(vnpayParams, vnp_SecureHash))
{
    // KHÔNG xử lý thanh toán
    return BadRequest("Invalid signature");
}
```

### **2. Kiểm tra số tiền**

Luôn kiểm tra số tiền từ VNPay có khớp với đơn hàng không.

```csharp
var orderAmount = (long)(order.TotalAmount * 100);
var vnpayAmount = long.Parse(vnp_Amount);

if (orderAmount != vnpayAmount)
{
    // Số tiền không khớp → Có thể bị giả mạo
    return BadRequest("Amount mismatch");
}
```

### **3. Tránh xử lý trùng lặp**

Kiểm tra `order.IsPaid` trước khi cập nhật.

```csharp
if (!order.IsPaid)
{
    // Chỉ xử lý nếu chưa thanh toán
    await _paymentVerificationService.AutoConfirmPaymentAsync(order.Id);
}
```

---

## 📊 VÍ DỤ MINH HỌA

### **Ví dụ 1: Thanh toán thành công**

**Input:**
- Sản phẩm: iPhone 15 Pro Max
- Giá: 25,000,000 VND
- PaymentMethod: "VNPay"

**Quy trình:**
1. Khách hàng chọn "Thanh toán VNPay" → Click "Đặt hàng"
2. Hệ thống tạo đơn hàng: `MP20241220120000`
3. Tạo Payment URL với:
   - `vnp_Amount`: `2500000000` (25,000,000 × 100)
   - `vnp_TxnRef`: `MP20241220120000`
4. Redirect đến VNPay
5. Khách hàng thanh toán bằng thẻ Visa
6. VNPay redirect về với `vnp_ResponseCode=00`
7. Hệ thống xác thực chữ ký → OK
8. Kiểm tra số tiền → OK
9. Cập nhật đơn hàng:
   - `IsPaid = true`
   - `OrderStatus = Confirmed`
   - Commit Inventory
10. Redirect đến `/Checkout/Success?orderCode=MP20241220120000`

### **Ví dụ 2: Thanh toán thất bại**

**Quy trình:**
1-5. Giống ví dụ 1
6. VNPay redirect về với `vnp_ResponseCode=51` (Không đủ số dư)
7. Hệ thống xác thực chữ ký → OK
8. Kiểm tra số tiền → OK
9. `IsPaymentSuccess("51")` → `false`
10. Hiển thị lỗi: "Thanh toán thất bại: Tài khoản không đủ số dư để thực hiện giao dịch"
11. Đơn hàng vẫn ở trạng thái `Pending`, `IsPaid = false`

---

## ⚠️ LƯU Ý QUAN TRỌNG

1. **Return URL và IPN URL phải accessible từ internet**
   - Test local: Dùng ngrok
   - Production: Đảm bảo domain có SSL (HTTPS)

2. **Luôn xác thực chữ ký trước khi xử lý**
   - Không xử lý nếu chữ ký không hợp lệ

3. **Kiểm tra số tiền**
   - VNPay yêu cầu số tiền × 100
   - So sánh với `order.TotalAmount * 100`

4. **Tránh xử lý trùng lặp**
   - Kiểm tra `order.IsPaid` trước khi cập nhật

5. **Xử lý lỗi**
   - Log tất cả lỗi để debug
   - Hiển thị thông báo rõ ràng cho khách hàng

---

## 🧪 TESTING

### **Test với Sandbox VNPay**

1. Đăng ký tài khoản tại: https://sandbox.vnpayment.vn/
2. Lấy `TmnCode` và `HashSecret`
3. Cập nhật `appsettings.json`
4. Test thanh toán với thẻ test (VNPay cung cấp)

### **Test IPN (Webhook)**

1. Dùng ngrok để expose local server:
   ```bash
   ngrok http 44300
   ```
2. Cập nhật IPN URL trong `appsettings.json`:
   ```json
   "IpnUrl": "https://abc123.ngrok.io/Checkout/VnPayIPN"
   ```
3. Test thanh toán → Kiểm tra IPN có được gửi không

---

## 📚 TÀI LIỆU THAM KHẢO

- **VNPay Documentation**: https://sandbox.vnpayment.vn/apis/docs/
- **VNPay Sandbox**: https://sandbox.vnpayment.vn/
- **File code**: `VNPayService.cs`, `CheckoutController.cs`

---

**Chúc bạn tích hợp thành công!** 🎉







