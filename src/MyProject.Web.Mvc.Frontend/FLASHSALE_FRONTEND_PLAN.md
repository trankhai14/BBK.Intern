# 📋 KẾ HOẠCH TRIỂN KHAI FLASHSALE CHO FRONTEND

## 🎯 MỤC TIÊU
Triển khai chức năng FlashSale cho phía người dùng (Frontend), cho phép khách hàng:
- Xem danh sách FlashSale đang diễn ra
- Xem chi tiết FlashSale và sản phẩm
- Mua sản phẩm FlashSale với giá đặc biệt
- Xem countdown timer và progress bar
- Thêm sản phẩm FlashSale vào giỏ hàng

---

## 📦 CÁC BƯỚC TRIỂN KHAI

### **PHASE 1: BACKEND API (Application Layer)**

#### 1.1. Thêm Methods vào IFlashSaleAppService
- [ ] `Task<List<FlashSaleDto>> GetActiveFlashSales()` - Lấy FlashSale đang active
- [ ] `Task<List<FlashSaleDto>> GetOngoingFlashSales()` - Lấy FlashSale đang diễn ra
- [ ] `Task<List<FlashSaleProductDto>> GetFlashSaleProductsByFlashSaleId(int flashSaleId)` - Lấy sản phẩm trong FlashSale
- [ ] `Task<FlashSaleProductDto> GetFlashSaleProductByProductId(int productId)` - Kiểm tra sản phẩm có trong FlashSale
- [ ] `Task PurchaseFlashSaleProduct(int flashSaleProductId, int quantity, long userId)` - Mua sản phẩm FlashSale

#### 1.2. Implement trong FlashSaleAppService
- [ ] Implement các methods trên
- [ ] Validate số lượng (RemainingQuantity, MaxQuantityPerUser)
- [ ] Cập nhật SoldQuantity khi mua
- [ ] Xử lý logic giới hạn số lượng mua per user

---

### **PHASE 2: FRONTEND CONTROLLER**

#### 2.1. Tạo FlashSalesController.cs
- [ ] `Index()` - Trang danh sách FlashSale
- [ ] `Detail(int id)` - Trang chi tiết FlashSale
- [ ] `GetFlashSaleProducts(int flashSaleId)` - API trả về sản phẩm (JSON)
- [ ] `CheckProductInFlashSale(int productId)` - Kiểm tra sản phẩm có trong FlashSale

---

### **PHASE 3: MODELS & VIEWMODELS**

#### 3.1. Tạo Models
- [ ] `Models/FlashSales/FlashSaleViewModel.cs`
- [ ] `Models/FlashSales/FlashSaleProductViewModel.cs`
- [ ] `Models/FlashSales/FlashSaleDetailViewModel.cs`

---

### **PHASE 4: VIEWS**

#### 4.1. Tạo Views cho FlashSale
- [ ] `Views/FlashSales/Index.cshtml` - Danh sách FlashSale
- [ ] `Views/FlashSales/Detail.cshtml` - Chi tiết FlashSale
- [ ] `Views/FlashSales/_FlashSaleProductCard.cshtml` - Partial view card sản phẩm
- [ ] `Views/FlashSales/_FlashSaleBanner.cshtml` - Banner FlashSale

#### 4.2. Tích hợp vào trang chủ
- [ ] `Views/Home/_FlashSaleSection.cshtml` - Section FlashSale trên trang chủ
- [ ] Cập nhật `Views/Home/Index.cshtml` - Thêm section FlashSale

#### 4.3. Tích hợp vào trang chi tiết sản phẩm
- [ ] Cập nhật `Views/Home/_DetailProductWeb.cshtml` - Hiển thị thông tin FlashSale

---

### **PHASE 5: JAVASCRIPT**

#### 5.1. Tạo JavaScript files
- [ ] `wwwroot/view-resources/Views/FlashSales/Index.js` - Countdown timer, auto-refresh
- [ ] `wwwroot/view-resources/Views/FlashSales/Detail.js` - Xử lý mua hàng FlashSale

#### 5.2. Cập nhật JavaScript hiện có
- [ ] `wwwroot/view-resources/Views/Home/Index.js` - Hiển thị FlashSale trên trang chủ
- [ ] `wwwroot/view-resources/Views/Carts/Index.js` - Xử lý FlashSale product trong cart
- [ ] `wwwroot/view-resources/Views/Home/Index.js` - Kiểm tra FlashSale trong trang chi tiết

---

### **PHASE 6: CSS & STYLING**

#### 6.1. Tạo CSS files
- [ ] `wwwroot/css/flashsale.css` - Styles cho FlashSale
  - Countdown timer styles
  - Progress bar styles
  - FlashSale badge styles
  - Product card styles
  - Animation effects

---

### **PHASE 7: TÍCH HỢP VÀO CÁC TRANG**

#### 7.1. Trang chủ (Home/Index)
- [ ] Hiển thị section FlashSale đang diễn ra
- [ ] Top 5-10 sản phẩm FlashSale hot nhất
- [ ] Countdown timer
- [ ] Link đến trang FlashSale detail

#### 7.2. Trang chi tiết sản phẩm (Home/GetDetailProduct)
- [ ] Kiểm tra sản phẩm có trong FlashSale
- [ ] Hiển thị badge "FlashSale"
- [ ] Hiển thị giá FlashSale và giá gốc (gạch ngang)
- [ ] Hiển thị số lượng còn lại
- [ ] Progress bar % đã bán
- [ ] Validate số lượng mua (MaxQuantityPerUser)

#### 7.3. Trang danh sách sản phẩm
- [ ] Badge "FlashSale" trên product card
- [ ] Hiển thị giá FlashSale và giá gốc

#### 7.4. Giỏ hàng (Carts/Index)
- [ ] Hiển thị giá FlashSale trong cart
- [ ] Badge "FlashSale" trên sản phẩm
- [ ] Xử lý logic tính tổng tiền

---

### **PHASE 8: LOGIC NGHIỆP VỤ**

#### 8.1. Countdown Timer
- [ ] Real-time countdown đến khi kết thúc
- [ ] Auto-refresh khi FlashSale kết thúc
- [ ] Hiển thị thông báo khi FlashSale sắp kết thúc

#### 8.2. Progress Bar
- [ ] Hiển thị % đã bán (SoldQuantity / FlashSaleQuantity)
- [ ] Hiển thị số lượng còn lại (RemainingQuantity)

#### 8.3. Validate số lượng
- [ ] Kiểm tra RemainingQuantity
- [ ] Kiểm tra MaxQuantityPerUser (nếu có)
- [ ] Hiển thị thông báo lỗi nếu vượt quá

#### 8.4. Giá sản phẩm
- [ ] Ưu tiên giá FlashSale nếu có
- [ ] Hiển thị giá gốc gạch ngang
- [ ] Tính % giảm giá

#### 8.5. Cart & Order
- [ ] Lưu FlashSaleProductId trong cart
- [ ] Phân biệt sản phẩm FlashSale và sản phẩm thường
- [ ] Cập nhật SoldQuantity khi đặt hàng
- [ ] Ghi nhận sản phẩm mua từ FlashSale trong Order

---

## 🔧 CÁC FILE CẦN TẠO/SỬA

### **Backend (Application)**
- [ ] `FlashSaleAppService.cs` - Thêm methods mới
- [ ] `IFlashSaleAppService.cs` - Thêm interface methods

### **Frontend (MVC)**
- [ ] `Controllers/FlashSalesController.cs` - Controller mới
- [ ] `Models/FlashSales/FlashSaleViewModel.cs` - ViewModel
- [ ] `Models/FlashSales/FlashSaleProductViewModel.cs` - ViewModel
- [ ] `Views/FlashSales/Index.cshtml` - View danh sách
- [ ] `Views/FlashSales/Detail.cshtml` - View chi tiết
- [ ] `Views/FlashSales/_FlashSaleProductCard.cshtml` - Partial view
- [ ] `Views/Home/_FlashSaleSection.cshtml` - Partial view
- [ ] `wwwroot/view-resources/Views/FlashSales/Index.js` - JavaScript
- [ ] `wwwroot/view-resources/Views/FlashSales/Detail.js` - JavaScript
- [ ] `wwwroot/css/flashsale.css` - CSS styles

### **Files cần cập nhật**
- [ ] `Views/Home/Index.cshtml` - Thêm FlashSale section
- [ ] `Views/Home/_DetailProductWeb.cshtml` - Hiển thị FlashSale info
- [ ] `wwwroot/view-resources/Views/Home/Index.js` - Cập nhật logic
- [ ] `wwwroot/view-resources/Views/Carts/Index.js` - Xử lý FlashSale
- [ ] `Controllers/HomeController.cs` - Thêm logic FlashSale
- [ ] `Controllers/CartsController.cs` - Xử lý FlashSale trong cart
- [ ] `Controllers/OrdersController.cs` - Xử lý FlashSale trong order

---

## ✅ CHECKLIST TRIỂN KHAI

### **Backend**
- [ ] Thêm API methods vào IFlashSaleAppService
- [ ] Implement các methods trong FlashSaleAppService
- [ ] Test API với Postman/Swagger

### **Frontend Controller**
- [ ] Tạo FlashSalesController
- [ ] Implement các actions
- [ ] Test các actions

### **Views**
- [ ] Tạo Views cho FlashSale
- [ ] Tạo Partial Views
- [ ] Tích hợp vào trang chủ
- [ ] Tích hợp vào trang chi tiết sản phẩm

### **JavaScript**
- [ ] Tạo JavaScript files
- [ ] Implement countdown timer
- [ ] Implement progress bar
- [ ] Implement validate số lượng
- [ ] Implement mua hàng FlashSale

### **CSS**
- [ ] Tạo CSS styles
- [ ] Style countdown timer
- [ ] Style progress bar
- [ ] Style FlashSale badge
- [ ] Style product cards

### **Testing**
- [ ] Test hiển thị FlashSale trên trang chủ
- [ ] Test trang chi tiết FlashSale
- [ ] Test mua sản phẩm FlashSale
- [ ] Test validate số lượng
- [ ] Test countdown timer
- [ ] Test progress bar
- [ ] Test thêm vào cart
- [ ] Test đặt hàng

---

## 🎨 UI/UX FEATURES

### **FlashSale Section trên trang chủ**
- Banner FlashSale nổi bật
- Countdown timer lớn
- Carousel sản phẩm FlashSale
- Button "Xem tất cả"

### **Trang chi tiết FlashSale**
- Banner FlashSale
- Countdown timer
- Danh sách sản phẩm với:
  - Hình ảnh
  - Tên sản phẩm
  - Giá FlashSale (màu đỏ, lớn)
  - Giá gốc (gạch ngang)
  - % giảm giá
  - Số lượng còn lại
  - Progress bar
  - Button "Mua ngay"

### **Trang chi tiết sản phẩm**
- Badge "FlashSale" màu đỏ
- Hiển thị giá FlashSale và giá gốc
- Countdown timer (nếu sản phẩm trong FlashSale)
- Số lượng còn lại
- Progress bar
- Validate số lượng mua

### **Giỏ hàng**
- Badge "FlashSale" trên sản phẩm
- Hiển thị giá FlashSale
- Tổng tiền với giá FlashSale

---

## 🚀 PRIORITY

### **High Priority**
1. Backend API methods
2. Frontend Controller
3. Views cơ bản
4. Tích hợp vào trang chủ
5. Tích hợp vào trang chi tiết sản phẩm

### **Medium Priority**
1. Countdown timer
2. Progress bar
3. Validate số lượng
4. CSS styling

### **Low Priority**
1. Animation effects
2. Advanced UI features
3. Analytics tracking

---

## 📝 NOTES

- Cần đảm bảo tính nhất quán với design hiện tại
- Cần xử lý edge cases (FlashSale kết thúc, hết hàng, etc.)
- Cần optimize performance (cache, lazy loading)
- Cần xử lý lỗi gracefully
- Cần test kỹ với nhiều scenarios

---

## 🔄 NEXT STEPS

Sau khi user xác nhận, sẽ bắt đầu triển khai theo thứ tự:
1. Backend API
2. Frontend Controller
3. Views
4. JavaScript
5. CSS
6. Tích hợp
7. Testing


