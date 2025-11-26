# HƯỚNG DẪN TÍCH HỢP VNPAY VÀO DỰ ÁN

## 1. TỔNG QUAN

VNPay là cổng thanh toán trực tuyến phổ biến tại Việt Nam, hỗ trợ thanh toán qua:
- Thẻ ATM nội địa
- Thẻ tín dụng/ghi nợ quốc tế (Visa, Mastercard, JCB)
- Ví điện tử (ZaloPay, Momo, ShopeePay)
- Internet Banking

---

## 2. ĐĂNG KÝ VÀ LẤY THÔNG TIN TỪ VNPAY

### 2.1. Đăng ký tài khoản VNPay

1. Truy cập: https://sandbox.vnpayment.vn/ (môi trường test) hoặc https://www.vnpayment.vn/ (production)
2. Đăng ký tài khoản merchant
3. Điền thông tin doanh nghiệp và gửi hồ sơ
4. Chờ VNPay phê duyệt và cung cấp thông tin:
   - **TmnCode** (Terminal Code): Mã định danh merchant
   - **HashSecret**: Mã bảo mật để tạo chữ ký
   - **Payment URL**: URL để gửi request thanh toán

### 2.2. Thông tin cần thiết từ VNPay

Sau khi đăng ký thành công, bạn sẽ nhận được:

| Thông tin | Mô tả | Ví dụ |
|-----------|-------|-------|
| **TmnCode** | Mã định danh merchant | `2QXUI4J4` |
| **HashSecret** | Mã bảo mật để tạo chữ ký | `RAOCTRGKRHJDJNQKXSTQNHGIVTSCOZDE` |
| **Payment URL (Sandbox)** | URL thanh toán test | `https://sandbox.vnpayment.vn/paymentv2/vpcpay.html` |
| **Payment URL (Production)** | URL thanh toán thật | `https://www.vnpayment.vn/paymentv2/vpcpay.html` |
| **Return URL** | URL VNPay redirect về sau khi thanh toán | `https://yourdomain.com/Checkout/VnPayReturn` |
| **IPN URL** | URL nhận webhook từ VNPay | `https://yourdomain.com/Checkout/VnPayIPN` |

---

## 3. CẤU HÌNH TRONG DỰ ÁN

### 3.1. Thêm cấu hình vào `appsettings.json`

Thêm vào file `appsettings.json`:

```json
{
  "VNPay": {
    "TmnCode": "YOUR_TMN_CODE",
    "HashSecret": "YOUR_HASH_SECRET",
    "PaymentUrl": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
    "ReturnUrl": "https://yourdomain.com/Checkout/VnPayReturn",
    "IpnUrl": "https://yourdomain.com/Checkout/VnPayIPN",
    "Command": "pay",
    "CurrCode": "VND",
    "Locale": "vn",
    "Version": "2.1.0"
  }
}
```

**Lưu ý:**
- **Sandbox (Test)**: Dùng `https://sandbox.vnpayment.vn/paymentv2/vpcpay.html`
- **Production**: Dùng `https://www.vnpayment.vn/paymentv2/vpcpay.html`
- Thay `YOUR_TMN_CODE` và `YOUR_HASH_SECRET` bằng thông tin từ VNPay
- Thay `yourdomain.com` bằng domain thực tế của bạn

### 3.2. Cấu hình cho môi trường Production

Trong `appsettings.Production.json`:

```json
{
  "VNPay": {
    "TmnCode": "YOUR_PRODUCTION_TMN_CODE",
    "HashSecret": "YOUR_PRODUCTION_HASH_SECRET",
    "PaymentUrl": "https://www.vnpayment.vn/paymentv2/vpcpay.html",
    "ReturnUrl": "https://yourdomain.com/Checkout/VnPayReturn",
    "IpnUrl": "https://yourdomain.com/Checkout/VnPayIPN"
  }
}
```

---

## 4. CẤU TRÚC TÍCH HỢP

### 4.1. Luồng thanh toán VNPay

```
1. Khách hàng chọn "Thanh toán VNPay" → Click "Thanh toán"
2. Hệ thống tạo đơn hàng (Status: Pending)
3. Hệ thống tạo Payment URL từ VNPay với các thông tin:
   - Số tiền
   - Mã đơn hàng (PaymentReference)
   - Thông tin khách hàng
   - Return URL
   - IPN URL
4. Redirect khách hàng đến VNPay
5. Khách hàng thanh toán trên VNPay
6. VNPay redirect về Return URL (Checkout/VnPayReturn)
7. VNPay gửi IPN (Instant Payment Notification) đến IPN URL (Checkout/VnPayIPN)
8. Hệ thống xác thực chữ ký và cập nhật trạng thái đơn hàng
```

### 4.2. Các file cần tạo/cập nhật

1. **VNPayService** (`MyProject.Application/Payments/VNPayService.cs`)
   - Tạo payment URL
   - Xác thực chữ ký từ VNPay
   - Xử lý kết quả thanh toán

2. **CheckoutController** (cập nhật)
   - Thêm action `VnPayPayment` để tạo payment URL
   - Thêm action `VnPayReturn` để xử lý redirect từ VNPay
   - Thêm action `VnPayIPN` để nhận webhook từ VNPay

3. **View** (cập nhật)
   - Thêm option "Thanh toán VNPay" trong `Checkout/Confirm.cshtml`

4. **Configuration** (cập nhật)
   - Thêm VNPay settings vào `appsettings.json`

---

## 5. CHI TIẾT KỸ THUẬT

### 5.1. Tạo Payment URL

VNPay yêu cầu các tham số sau:

| Tham số | Bắt buộc | Mô tả | Ví dụ |
|---------|----------|-------|-------|
| `vnp_Version` | Có | Phiên bản API | `2.1.0` |
| `vnp_Command` | Có | Loại giao dịch | `pay` |
| `vnp_TmnCode` | Có | Mã merchant | `2QXUI4J4` |
| `vnp_Amount` | Có | Số tiền (VND) | `10000000` (10,000,000 VND) |
| `vnp_CurrCode` | Có | Loại tiền tệ | `VND` |
| `vnp_TxnRef` | Có | Mã tham chiếu đơn hàng | `MP20241220120000` |
| `vnp_OrderInfo` | Có | Mô tả đơn hàng | `Thanh toan don hang MP20241220120000` |
| `vnp_OrderType` | Có | Loại đơn hàng | `other` |
| `vnp_Locale` | Có | Ngôn ngữ | `vn` |
| `vnp_ReturnUrl` | Có | URL redirect về | `https://yourdomain.com/Checkout/VnPayReturn` |
| `vnp_IpAddr` | Có | IP khách hàng | `192.168.1.1` |
| `vnp_CreateDate` | Có | Thời gian tạo (yyyyMMddHHmmss) | `20241220120000` |
| `vnp_SecureHash` | Có | Chữ ký bảo mật | (tính toán từ các tham số) |

### 5.2. Tạo chữ ký (SecureHash)

VNPay sử dụng HMAC SHA512 để tạo chữ ký:

```
1. Sắp xếp các tham số theo thứ tự alphabet (trừ vnp_SecureHash)
2. Nối các tham số thành chuỗi: key1=value1&key2=value2&...
3. Dùng HMAC SHA512 với HashSecret để tạo chữ ký
4. Chuyển chữ ký thành chữ hoa
```

**Ví dụ:**
```
Input: vnp_Amount=10000000&vnp_Command=pay&vnp_CreateDate=20241220120000&vnp_CurrCode=VND&vnp_IpAddr=192.168.1.1&vnp_Locale=vn&vnp_OrderInfo=Thanh toan don hang&vnp_OrderType=other&vnp_ReturnUrl=https://yourdomain.com/Checkout/VnPayReturn&vnp_TmnCode=2QXUI4J4&vnp_TxnRef=MP20241220120000&vnp_Version=2.1.0
HashSecret: RAOCTRGKRHJDJNQKXSTQNHGIVTSCOZDE
Output: A1B2C3D4E5F6... (chữ ký HMAC SHA512)
```

### 5.3. Xác thực chữ ký từ VNPay

Khi VNPay redirect về hoặc gửi IPN, sẽ kèm theo các tham số:
- `vnp_TxnRef`: Mã đơn hàng
- `vnp_Amount`: Số tiền
- `vnp_ResponseCode`: Mã phản hồi (`00` = thành công)
- `vnp_TransactionNo`: Mã giao dịch VNPay
- `vnp_SecureHash`: Chữ ký

**Cách xác thực:**
1. Lấy tất cả tham số từ VNPay (trừ `vnp_SecureHash`)
2. Tạo chữ ký từ các tham số đó
3. So sánh với `vnp_SecureHash` từ VNPay
4. Nếu khớp → Xác thực thành công

### 5.4. Mã phản hồi (Response Code)

| Mã | Ý nghĩa |
|----|---------|
| `00` | Giao dịch thành công |
| `07` | Trừ tiền thành công. Giao dịch bị nghi ngờ (liên quan tới lừa đảo, giao dịch bất thường) |
| `09` | Thẻ/Tài khoản chưa đăng ký dịch vụ InternetBanking |
| `10` | Xác thực thông tin thẻ/tài khoản không đúng quá 3 lần |
| `11` | Đã hết hạn chờ thanh toán. Xin vui lòng thực hiện lại giao dịch |
| `12` | Thẻ/Tài khoản bị khóa |
| `51` | Tài khoản không đủ số dư để thực hiện giao dịch |
| `65` | Tài khoản đã vượt quá hạn mức giao dịch trong ngày |
| `75` | Ngân hàng thanh toán đang bảo trì |
| `79` | Nhập sai mật khẩu thanh toán quá số lần quy định |

---

## 6. BẢO MẬT

### 6.1. Xác thực chữ ký

**QUAN TRỌNG**: Luôn xác thực chữ ký từ VNPay trước khi xử lý thanh toán.

```csharp
// ❌ SAI - Không xác thực chữ ký
if (responseCode == "00")
{
    // Cập nhật đơn hàng
}

// ✅ ĐÚNG - Xác thực chữ ký trước
if (ValidateSignature(vnpayParams, vnp_SecureHash))
{
    if (responseCode == "00")
    {
        // Cập nhật đơn hàng
    }
}
```

### 6.2. Kiểm tra số tiền

Luôn kiểm tra số tiền từ VNPay có khớp với số tiền đơn hàng không:

```csharp
var orderAmount = order.TotalAmount * 100; // VNPay yêu cầu số tiền × 100
var vnpayAmount = long.Parse(vnp_Amount);

if (orderAmount != vnpayAmount)
{
    // Số tiền không khớp → Có thể bị giả mạo
    return BadRequest("Số tiền không khớp");
}
```

### 6.3. Kiểm tra trạng thái đơn hàng

Tránh xử lý trùng lặp:

```csharp
if (order.IsPaid)
{
    // Đơn hàng đã được thanh toán → Không xử lý lại
    return Ok("Đơn hàng đã được thanh toán");
}
```

---

## 7. TESTING

### 7.1. Môi trường Sandbox

VNPay cung cấp môi trường sandbox để test:
- URL: `https://sandbox.vnpayment.vn/paymentv2/vpcpay.html`
- Thẻ test: VNPay sẽ cung cấp danh sách thẻ test

### 7.2. Test Cases

1. **Thanh toán thành công**
   - Chọn VNPay → Thanh toán → Nhập thông tin thẻ test → Xác nhận
   - Kỳ vọng: Redirect về Return URL với `vnp_ResponseCode=00`, đơn hàng được cập nhật

2. **Thanh toán thất bại**
   - Chọn VNPay → Thanh toán → Nhập sai thông tin
   - Kỳ vọng: Redirect về với mã lỗi, đơn hàng vẫn ở trạng thái Pending

3. **Hủy thanh toán**
   - Chọn VNPay → Thanh toán → Click "Hủy"
   - Kỳ vọng: Redirect về với thông báo hủy

4. **IPN (Webhook)**
   - Thanh toán thành công → Kiểm tra IPN có được gửi đến không
   - Kỳ vọng: IPN được xử lý và cập nhật đơn hàng

---

## 8. TROUBLESHOOTING

### 8.1. Lỗi "Invalid signature"

**Nguyên nhân:**
- HashSecret không đúng
- Thứ tự sắp xếp tham số sai
- Thiếu hoặc thừa tham số

**Giải pháp:**
- Kiểm tra lại HashSecret trong `appsettings.json`
- Đảm bảo sắp xếp tham số theo thứ tự alphabet
- Kiểm tra log để xem các tham số được gửi

### 8.2. Lỗi "Amount mismatch"

**Nguyên nhân:**
- Số tiền gửi lên VNPay không khớp với số tiền đơn hàng
- VNPay yêu cầu số tiền × 100 (ví dụ: 10,000 VND → 1000000)

**Giải pháp:**
- Kiểm tra logic tính toán số tiền: `amount * 100`
- So sánh `vnp_Amount` với `order.TotalAmount * 100`

### 8.3. IPN không được gửi

**Nguyên nhân:**
- IPN URL không accessible từ internet
- Firewall chặn request từ VNPay
- SSL certificate không hợp lệ

**Giải pháp:**
- Đảm bảo IPN URL có thể truy cập từ internet (dùng ngrok để test local)
- Kiểm tra firewall rules
- Sử dụng HTTPS cho IPN URL

---

## 9. TÀI LIỆU THAM KHẢO

- **VNPay Documentation**: https://sandbox.vnpayment.vn/apis/docs/
- **VNPay Sandbox**: https://sandbox.vnpayment.vn/
- **VNPay Support**: support@vnpayment.vn

---

## 10. CHECKLIST TRIỂN KHAI

- [ ] Đăng ký tài khoản VNPay và lấy TmnCode, HashSecret
- [ ] Cấu hình VNPay settings trong `appsettings.json`
- [ ] Tạo `VNPayService` để xử lý logic VNPay
- [ ] Cập nhật `CheckoutController` với các action VNPay
- [ ] Cập nhật view để thêm option "Thanh toán VNPay"
- [ ] Test thanh toán thành công
- [ ] Test thanh toán thất bại
- [ ] Test IPN (webhook)
- [ ] Kiểm tra bảo mật (chữ ký, số tiền)
- [ ] Deploy lên production và cập nhật cấu hình

---

**Chúc bạn tích hợp thành công!** 🎉







