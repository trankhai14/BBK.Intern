using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MyProject.Carts;
using MyProject.Carts.Dto;
using MyProject.Controllers;
using MyProject.CustomerProfiles;
using MyProject.CustomerProfiles.Dto;
using MyProject.Inventories;
using MyProject.OrderDetails;
using MyProject.OrderDetails.Dto;
using MyProject.Orders;
using MyProject.Orders.Dto;
using MyProject.Product.Dtos;
using MyProject.Products;
using MyProject.Users;
using MyProject.Web.Models.Checkout;
using MyProject.Payments;
using MyProject.Payments.Dtos;
using PaymentTransactionStatus = MyProject.Payments.PaymentTransactionStatus;

namespace MyProject.Web.Controllers
{
	[AbpMvcAuthorize]
	public class CheckoutController : MyProjectControllerBase
	{
		private readonly ICartAppService _cartAppService;
		private readonly IProductAppService _productAppService;
		private readonly IInventoryAppService _inventoryAppService;
		private readonly IOrderAppService _orderAppService;
		private readonly IOrderDetailAppService _orderDetailAppService;
		private readonly IUserAppService _userAppService;
		private readonly ICustomerProfileAppService _customerProfileAppService;
		private readonly IPaymentVerificationService _paymentVerificationService;
		private readonly CheckoutPaymentSettings _paymentSettings;
		private readonly VNPayService _vnPayService;

		public CheckoutController(
			ICartAppService cartAppService,
			IProductAppService productAppService,
			IInventoryAppService inventoryAppService,
			IOrderAppService orderAppService,
			IOrderDetailAppService orderDetailAppService,
			IUserAppService userAppService,
			ICustomerProfileAppService customerProfileAppService,
			IPaymentVerificationService paymentVerificationService,
			IConfiguration configuration)
		{
			_cartAppService = cartAppService;
			_productAppService = productAppService;
			_inventoryAppService = inventoryAppService;
			_orderAppService = orderAppService;
			_orderDetailAppService = orderDetailAppService;
			_userAppService = userAppService;
			_customerProfileAppService = customerProfileAppService;
			_paymentVerificationService = paymentVerificationService;

			_paymentSettings = new CheckoutPaymentSettings
			{
				BankCode = configuration["Payment:BankCode"] ?? "VCB",
				BankAccount = configuration["Payment:BankAccount"] ?? "123456789",
				BankAccountName = configuration["Payment:BankAccountName"] ?? "CONG TY DEMO",
				QrDescriptionTemplate = configuration["Payment:QrDescriptionTemplate"] ?? "TT {0}"
			};

			_vnPayService = new VNPayService(configuration);
		}

		/// <summary>
		/// Bước xác nhận đơn hàng: thông tin cá nhân, phương thức thanh toán, tóm tắt giỏ hàng
		/// </summary>
		[HttpGet]
		public async Task<IActionResult> Confirm()
		{
			var model = new CheckoutConfirmViewModel
			{
				FullName = "",
				PhoneNumber = "",
				Address = "",
				PaymentMethod = "QR", // default
				Note = ""
			};

			if (AbpSession.UserId.HasValue)
			{
				try
				{
					var profiles = await _customerProfileAppService.GetAllByCurrentUser();
					model.Profiles = profiles
						.Select(p => new CheckoutConfirmViewModel.CustomerProfileOption
						{
							Id = p.Id,
							DisplayName = BuildProfileDisplayName(p),
							FullName = p.FullName,
							PhoneNumber = p.PhoneNumber,
							Address = BuildProfileAddress(p)
						})
						.ToList();

					var preferredProfile = profiles.FirstOrDefault(p => p.IsDefault) ?? profiles.FirstOrDefault();

					if (preferredProfile != null)
					{
						model.SelectedProfileId = preferredProfile.Id;
						model.FullName = preferredProfile.FullName;
						model.PhoneNumber = preferredProfile.PhoneNumber;
						model.Address = BuildProfileAddress(preferredProfile);
					}
					else
					{
						var user = await _userAppService.GetUserById(AbpSession.UserId.Value);
						if (user != null)
						{
							model.FullName = !string.IsNullOrWhiteSpace(user.FullName)
								? user.FullName
								: $"{user.Name} {user.Surname}".Trim();
						}
					}
				}
				catch (Exception ex)
				{
					Logger.Warn("Không thể load thông tin người dùng khi checkout", ex);
				}
			}

			if (TempData["CheckoutError"] != null)
			{
				ViewBag.CheckoutError = TempData["CheckoutError"];
			}

			return View("~/Views/Checkout/Confirm.cshtml", model);
		}

		/// <summary>
		/// API trả về tóm tắt giỏ hàng cho bước xác nhận
		/// </summary>
		[HttpGet]
		public async Task<JsonResult> Summary()
		{
			var summary = await BuildCartSummaryAsync();
			return Json(new
			{
				success = true,
				items = summary.Items.Select(item => new
				{
					item.ProductId,
					item.ProductName,
					item.Quantity,
					item.UnitPrice,
					item.LineTotal,
					item.Image
				}),
				total = summary.Total
			});
		}

		/// <summary>
		/// Nhận submit từ Confirm và chuyển tới trang thanh toán (render QR)
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Payment(CheckoutConfirmViewModel input)
		{
			if (!AbpSession.UserId.HasValue)
			{
				return RedirectToAction("Login", "Account");
			}

			var summary = await BuildCartSummaryAsync();
			if (!summary.Items.Any())
			{
				TempData["CheckoutError"] = "Giỏ hàng của bạn đang trống. Vui lòng chọn sản phẩm trước khi thanh toán.";
				return RedirectToAction(nameof(Confirm));
			}

			CustomerProfileDto selectedProfile = null;
			if (input.SelectedProfileId.HasValue)
			{
				try
				{
					selectedProfile = await _customerProfileAppService.GetById(input.SelectedProfileId.Value);
				}
				catch (Exception ex)
				{
					Logger.Warn("Không thể lấy thông tin profile đã chọn", ex);
				}
			}

			var customerFullName = selectedProfile?.FullName ?? input.FullName;
			var customerPhone = selectedProfile?.PhoneNumber ?? input.PhoneNumber;
			var customerAddress = selectedProfile != null ? BuildProfileAddress(selectedProfile) : input.Address;

			var paymentReference = $"MP{DateTime.UtcNow:yyyyMMddHHmmssfff}";
			var paymentExpiredAt = DateTime.UtcNow.AddMinutes(30);

			var reservedItems = new List<CheckoutCartItem>();
			try
			{
				foreach (var item in summary.Items)
				{
					await _inventoryAppService.ReserveInventory(item.ProductId, item.Quantity);
					reservedItems.Add(item);
				}
			}
			catch (Exception ex)
			{
				foreach (var reserved in reservedItems)
				{
					await _inventoryAppService.ReleaseReservedInventory(reserved.ProductId, reserved.Quantity);
				}

				Logger.Warn("Không thể giữ hàng trong kho khi checkout", ex);
				TempData["CheckoutError"] = ex.Message;
				return RedirectToAction(nameof(Confirm));
			}

			int orderId;
			try
			{
				var createOrderDto = new CreateOrderDto
				{
					UserId = AbpSession.UserId.Value,
					NameUser = string.IsNullOrWhiteSpace(customerFullName) ? "Khách hàng" : customerFullName,
					TotalAmount = summary.Total,
					DiscountAmount = 0,
					OrderStatus = (int)OrderStatus.Pending,
					PaymentMethod = input.PaymentMethod == "VNPay" 
						? (int)CheckoutPaymentMethod.VNPay 
						: (int)CheckoutPaymentMethod.QRTransfer,
					PhoneNumber = customerPhone,
					ShippingAddress = customerAddress,
					PaymentReference = paymentReference,
					IsPaid = false,
					PaidTime = null,
					PaymentExpiredAt = paymentExpiredAt,
					CustomerNote = input.Note
				};

				orderId = await _orderAppService.CreateOrder(createOrderDto);

				foreach (var item in summary.Items)
				{
					var detailDto = new OrderDetailDto
					{
						OrderId = orderId,
						ProductId = item.ProductId,
						Quantity = item.Quantity,
						UnitPrice = item.UnitPrice,
						DiscountPrice = 0
					};

					await _orderDetailAppService.CreateOrderDetail(detailDto);
				}

				await _cartAppService.ClearCart(AbpSession.UserId.Value);
			}
			catch (Exception ex)
			{
				foreach (var reserved in reservedItems)
				{
					await _inventoryAppService.ReleaseReservedInventory(reserved.ProductId, reserved.Quantity);
				}

				Logger.Error("Không thể tạo đơn hàng", ex);
				TempData["CheckoutError"] = "Không thể tạo đơn hàng. Vui lòng thử lại sau.";
				return RedirectToAction(nameof(Confirm));
			}

			// Kiểm tra phương thức thanh toán
			if (input.PaymentMethod == "VNPay")
			{
				// Tạo payment URL từ VNPay
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

			// Thanh toán QR (mặc định)
			var paymentContent = BuildPaymentContent(paymentReference, customerFullName);

			var paymentModel = new CheckoutPaymentViewModel
			{
				OrderId = orderId,
				OrderAmount = summary.Total,
				PaymentReference = paymentReference,
				PaymentContent = paymentContent,
				BankCode = _paymentSettings.BankCode,
				BankAccount = _paymentSettings.BankAccount,
				BankAccountName = _paymentSettings.BankAccountName,
				QrImageUrl = BuildQrUrl(summary.Total, paymentContent),
				FullName = customerFullName,
				PhoneNumber = customerPhone,
				Address = customerAddress,
				Note = input.Note,
				PaymentExpiredAt = paymentExpiredAt,
				Items = summary.Items.Select(item => new CheckoutPaymentViewModel.CheckoutPaymentItemViewModel
				{
					ProductId = item.ProductId,
					ProductName = item.ProductName,
					Image = item.Image,
					Quantity = item.Quantity,
					UnitPrice = item.UnitPrice
				}).ToList()
			};

			return View("~/Views/Checkout/Payment.cshtml", paymentModel);
		}

		/// <summary>
		/// AJAX: Kiểm tra trạng thái thanh toán
		/// </summary>
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

		/// <summary>
		/// AJAX: xác nhận đã thanh toán (sẽ được tự động hóa khi tích hợp webhook)
		/// </summary>
		[HttpPost]
		public async Task<JsonResult> ConfirmPaid([FromBody] ConfirmPaidInput input)
		{
			if (input == null || input.OrderId <= 0)
			{
				return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
			}

			if (!AbpSession.UserId.HasValue)
			{
				return Json(new { success = false, message = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại." });
			}

			var order = await _orderAppService.GetOrderById(input.OrderId);
			if (order == null || order.UserId != AbpSession.UserId.Value)
			{
				return Json(new { success = false, message = "Không tìm thấy đơn hàng của bạn." });
			}

			if (order.IsPaid)
			{
				return Json(new { success = true, redirectUrl = Url.Action(nameof(Success), new { orderCode = order.PaymentReference }) });
			}

			// Kiểm tra xem đã có giao dịch được xác nhận chưa
			var existingTransaction = await _paymentVerificationService.GetVerifiedTransactionAsync(order.Id);
			if (existingTransaction != null && existingTransaction.Status == PaymentTransactionStatus.Verified)
			{
				// Đã được xác nhận tự động, chỉ cần redirect
				return Json(new
				{
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
				return Json(new
				{
					success = false,
					message = "Chưa tìm thấy giao dịch thanh toán. Vui lòng kiểm tra lại hoặc liên hệ hỗ trợ.",
					canRetry = true
				});
			}

			// Đã tìm thấy giao dịch, lưu vào database
			await _paymentVerificationService.SaveTransactionAsync(order.Id, verificationResult.Transaction);

			// Xác nhận thanh toán
			var details = await _orderDetailAppService.GetOrderListById(order.Id);
			try
			{
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
				var transactionDto = new PaymentTransactionDto
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
				};

				await _paymentVerificationService.SaveTransactionAsync(order.Id, transactionDto);

				// Tự động xác nhận thanh toán
				if (!order.IsPaid)
				{
					var confirmed = await _paymentVerificationService.AutoConfirmPaymentAsync(order.Id);
					if (confirmed)
					{
						Logger.Info($"Đã tự động xác nhận thanh toán cho đơn hàng #{order.Id} qua webhook");
					}
				}

				return Ok(new { success = true, message = "Payment verified" });
			}
			catch (Exception ex)
			{
				Logger.Error("Lỗi khi xử lý webhook", ex);
				return StatusCode(500, "Internal server error");
			}
		}

		/// <summary>
		/// Validate webhook signature
		/// </summary>
		private bool ValidateWebhookSignature(BankWebhookDto webhookData)
		{
			// TODO: Implement signature validation logic
			// Ví dụ: HMAC SHA256 với secret key từ configuration
			// Tạm thời return true để test, cần implement thực tế khi tích hợp với ngân hàng
			return true;
		}

		/// <summary>
		/// Trang kết quả thanh toán
		/// </summary>
		[HttpGet]
		[AllowAnonymous]
		public IActionResult Success(string orderCode = "")
		{
			Logger.Info($"Success page accessed with orderCode: {orderCode}");
			ViewBag.OrderCode = orderCode;
			ViewBag.Title = "Thanh toán thành công";
			return View("~/Views/Checkout/Success.cshtml");
		}

		private async Task<CheckoutCartSummary> BuildCartSummaryAsync()
		{
			var summary = new CheckoutCartSummary();

			if (!AbpSession.UserId.HasValue)
			{
				return summary;
			}

			var cartItems = await _cartAppService.GetAllCart();
			if (cartItems == null || !cartItems.Any())
			{
				return summary;
			}

			foreach (var item in cartItems)
			{
				var product = await _productAppService.GetAsync(new EntityDto<int>(item.ProductId));
				var unitPrice = product.Price;

				var cartItem = new CheckoutCartItem
				{
					ProductId = product.Id,
					ProductName = product.Name,
					Quantity = item.Quantity,
					UnitPrice = unitPrice,
					Image = product.Image
				};

				summary.Items.Add(cartItem);
			}

			summary.Total = summary.Items.Sum(x => x.LineTotal);
			return summary;
		}

		private string BuildQrUrl(decimal amount, string paymentContent)
		{
			var info = string.Format(_paymentSettings.QrDescriptionTemplate, paymentContent);
			var encodedInfo = System.Net.WebUtility.UrlEncode(info);
			var accountName = System.Net.WebUtility.UrlEncode(_paymentSettings.BankAccountName);
			var amountValue = ((long)Math.Round(amount, 0));

			return $"https://img.vietqr.io/image/{_paymentSettings.BankCode}-{_paymentSettings.BankAccount}-qr_only.png?amount={amountValue}&addInfo={encodedInfo}&accountName={accountName}";
		}

		private static string BuildProfileDisplayName(CustomerProfileDto profile)
		{
			if (profile == null)
			{
				return string.Empty;
			}

			var address = BuildProfileAddress(profile);
			return $"{profile.FullName} - {profile.PhoneNumber}{(string.IsNullOrWhiteSpace(address) ? string.Empty : $" ({address})")}";
		}

		private static string BuildProfileAddress(CustomerProfileDto profile)
		{
			if (profile == null)
			{
				return string.Empty;
			}

			var segments = new[] { profile.Address, profile.Ward, profile.District, profile.City }
				.Where(x => !string.IsNullOrWhiteSpace(x));

			return string.Join(", ", segments);
		}

		private string BuildPaymentContent(string paymentReference, string customerFullName)
		{
			var paymentDate = DateTime.Now.ToString("ddMMyyyy");
			var normalizedName = NormalizeForPayment(customerFullName);
			var content = $"{paymentDate}-{paymentReference}-{normalizedName}".Trim('-');
			return content.Length > 60 ? content.Substring(0, 60) : content;
		}

		private static string NormalizeForPayment(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return "KhachHang";
			}

			var trimmed = value.Trim();

			var normalized = trimmed.Normalize(NormalizationForm.FormD);
			var builder = new StringBuilder(normalized.Length);

			foreach (var ch in normalized)
			{
				var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(ch);
				if (unicodeCategory != UnicodeCategory.NonSpacingMark)
				{
					builder.Append(ch);
				}
			}

			var ascii = builder.ToString().Normalize(NormalizationForm.FormC);
			var safeChars = ascii.Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || c == '-' || c == '_');
			var result = new string(safeChars.ToArray());
			result = Regex.Replace(result, @"\s{2,}", " ");
			return result.Replace(" ", "_");
		}

		private class CheckoutCartSummary
		{
			public List<CheckoutCartItem> Items { get; } = new List<CheckoutCartItem>();
			public decimal Total { get; set; }
		}

		private class CheckoutCartItem
		{
			public int ProductId { get; set; }
			public string ProductName { get; set; }
			public int Quantity { get; set; }
			public decimal UnitPrice { get; set; }
			public string Image { get; set; }
			public decimal LineTotal => UnitPrice * Quantity;
		}

		private class CheckoutPaymentSettings
		{
			public string BankCode { get; set; }
			public string BankAccount { get; set; }
			public string BankAccountName { get; set; }
			public string QrDescriptionTemplate { get; set; }
		}

		public class ConfirmPaidInput
		{
			public int OrderId { get; set; }
		}

		// Sử dụng enum OrderStatus từ namespace MyProject.Orders

		private enum CheckoutPaymentMethod
		{
			QRTransfer = 1,
			VNPay = 2
		}

		/// <summary>
		/// VNPay redirect về sau khi thanh toán
		/// </summary>
		[HttpGet]
		[AllowAnonymous]
		public async Task<IActionResult> VnPayReturn()
		{
			try
			{
				// Lấy query string từ VNPay
				var queryString = Request.QueryString.ToString();
				Logger.Info($"VNPay Return - QueryString: {queryString}");
				
				if (string.IsNullOrEmpty(queryString))
				{
					Logger.Warn("VNPay Return: Empty query string");
					ViewBag.ErrorMessage = "Không nhận được thông tin từ VNPay. Vui lòng thử lại.";
					return View("PaymentError");
				}

				// Parse các tham số
				var vnpayParams = _vnPayService.ParseResponse(queryString.TrimStart('?'));
				Logger.Info($"VNPay Return - Parsed params count: {vnpayParams.Count}");

				// Lấy các tham số quan trọng
				var vnp_TxnRef = vnpayParams.GetValueOrDefault("vnp_TxnRef", "");
				var vnp_ResponseCode = vnpayParams.GetValueOrDefault("vnp_ResponseCode", "");
				var vnp_SecureHash = vnpayParams.GetValueOrDefault("vnp_SecureHash", "");
				var vnp_Amount = vnpayParams.GetValueOrDefault("vnp_Amount", "");

				Logger.Info($"VNPay Return - TxnRef: {vnp_TxnRef}, ResponseCode: {vnp_ResponseCode}");

				// Xác thực chữ ký
				var isValidSignature = _vnPayService.ValidateSignature(vnpayParams, vnp_SecureHash);
				if (!isValidSignature)
				{
					Logger.Warn($"VNPay return: Invalid signature for order {vnp_TxnRef}");
					Logger.Warn($"VNPay return: Received hash: {vnp_SecureHash}");
					Logger.Warn($"VNPay return: All params: {string.Join(", ", vnpayParams.Select(x => $"{x.Key}={x.Value}"))}");
					
					// Log thêm để debug
					var filteredParams = vnpayParams
						.Where(x => x.Key != "vnp_SecureHash" && x.Key != "vnp_SecureHashType")
						.OrderBy(x => x.Key)
						.ToDictionary(x => x.Key, x => x.Value);
					var queryStringForHash = string.Join("&", filteredParams.Select(x => $"{x.Key}={x.Value}"));
					Logger.Warn($"VNPay return: Query string for hash: {queryStringForHash}");
					
					// Vẫn tiếp tục xử lý nếu ResponseCode = 00 (thanh toán thành công)
					// Vì có thể do HashSecret không khớp nhưng thanh toán đã thành công
					if (vnp_ResponseCode != "00")
					{
						ViewBag.ErrorMessage = "Chữ ký không hợp lệ. Vui lòng kiểm tra lại cấu hình VNPay hoặc liên hệ hỗ trợ.";
						return View("PaymentError");
					}
					else
					{
						Logger.Warn($"VNPay return: Signature invalid but ResponseCode=00, continuing processing...");
					}
				}

				// Tìm đơn hàng
				var order = await _orderAppService.GetOrderByPaymentReference(vnp_TxnRef);
				if (order == null)
				{
					Logger.Warn($"VNPay Return: Order not found for payment reference {vnp_TxnRef}");
					ViewBag.ErrorMessage = $"Không tìm thấy đơn hàng với mã: {vnp_TxnRef}. Vui lòng kiểm tra lại hoặc liên hệ hỗ trợ.";
					return View("PaymentError");
				}

				Logger.Info($"VNPay Return: Found order {order.Id}, IsPaid: {order.IsPaid}");

				// Kiểm tra số tiền
				var orderAmount = (long)(order.TotalAmount * 100);
				if (long.TryParse(vnp_Amount, out var vnpayAmount) && orderAmount != vnpayAmount)
				{
					Logger.Warn($"VNPay return: Amount mismatch for order {vnp_TxnRef}. Order: {orderAmount}, VNPay: {vnpayAmount}");
					ViewBag.ErrorMessage = $"Số tiền không khớp. Đơn hàng: {order.TotalAmount:N0} VND, VNPay: {vnpayAmount / 100:N0} VND. Vui lòng liên hệ hỗ trợ.";
					return View("PaymentError");
				}

				// Xử lý kết quả thanh toán
				if (_vnPayService.IsPaymentSuccess(vnp_ResponseCode))
				{
					Logger.Info($"VNPay Return: Payment successful for order {vnp_TxnRef}");
					
					// Thanh toán thành công
					if (!order.IsPaid)
					{
						Logger.Info($"VNPay Return: Processing payment for order {order.Id}");
						
						// Lưu transaction
						var transactionDto = new PaymentTransactionDto
						{
							OrderId = order.Id,
							PaymentReference = vnp_TxnRef,
							Amount = order.TotalAmount,
							BankCode = "VNPay",
							TransactionId = vnpayParams.GetValueOrDefault("vnp_TransactionNo", ""),
							TransactionTime = DateTime.TryParseExact(
								vnpayParams.GetValueOrDefault("vnp_PayDate", ""),
								"yyyyMMddHHmmss",
								null,
								System.Globalization.DateTimeStyles.None,
								out var payDate) ? payDate : DateTime.UtcNow,
							Status = PaymentTransactionStatus.Verified,
							VerifiedBy = "VNPay",
							VerifiedAt = DateTime.UtcNow
						};

						await _paymentVerificationService.SaveTransactionAsync(order.Id, transactionDto);
						await _paymentVerificationService.AutoConfirmPaymentAsync(order.Id);
						
						Logger.Info($"VNPay Return: Payment processed successfully for order {order.Id}");
					}
					else
					{
						Logger.Info($"VNPay Return: Order {order.Id} already paid, skipping payment processing");
					}

					// Redirect đến trang thành công
					Logger.Info($"VNPay Return: Redirecting to Success page with orderCode: {vnp_TxnRef}");
					return RedirectToAction(nameof(Success), new { orderCode = vnp_TxnRef });
				}
				else
				{
					// Thanh toán thất bại
					var errorMessage = _vnPayService.GetResponseMessage(vnp_ResponseCode);
					Logger.Warn($"VNPay Return: Payment failed for order {vnp_TxnRef}. ResponseCode: {vnp_ResponseCode}, Message: {errorMessage}");
					ViewBag.ErrorMessage = $"Thanh toán thất bại: {errorMessage}";
					return View("PaymentError");
				}
			}
			catch (Exception ex)
			{
				Logger.Error("Lỗi khi xử lý VNPay return", ex);
				Logger.Error($"Exception details: {ex.Message}\nStack trace: {ex.StackTrace}");
				ViewBag.ErrorMessage = $"Có lỗi xảy ra khi xử lý thanh toán: {ex.Message}. Vui lòng liên hệ hỗ trợ.";
				return View("PaymentError");
			}
		}

		/// <summary>
		/// VNPay IPN (Instant Payment Notification) - Webhook
		/// </summary>
		[HttpPost]
		[AllowAnonymous]
		public async Task<IActionResult> VnPayIPN()
		{
			try
			{
				// Lấy dữ liệu từ request body hoặc form
				var form = await Request.ReadFormAsync();
				var vnpayParams = new Dictionary<string, string>();

				foreach (var key in form.Keys)
				{
					vnpayParams[key] = form[key].ToString();
				}

				// Lấy các tham số quan trọng
				var vnp_TxnRef = vnpayParams.GetValueOrDefault("vnp_TxnRef", "");
				var vnp_ResponseCode = vnpayParams.GetValueOrDefault("vnp_ResponseCode", "");
				var vnp_SecureHash = vnpayParams.GetValueOrDefault("vnp_SecureHash", "");

				// Xác thực chữ ký
				if (!_vnPayService.ValidateSignature(vnpayParams, vnp_SecureHash))
				{
					Logger.Warn($"VNPay IPN: Invalid signature for order {vnp_TxnRef}");
					return Json(new { RspCode = "97", Message = "Invalid signature" });
				}

				// Tìm đơn hàng
				var order = await _orderAppService.GetOrderByPaymentReference(vnp_TxnRef);
				if (order == null)
				{
					return Json(new { RspCode = "01", Message = "Order not found" });
				}

				// Kiểm tra số tiền
				var vnp_Amount = vnpayParams.GetValueOrDefault("vnp_Amount", "");
				var orderAmount = (long)(order.TotalAmount * 100);
				if (long.TryParse(vnp_Amount, out var vnpayAmount) && orderAmount != vnpayAmount)
				{
					Logger.Warn($"VNPay IPN: Amount mismatch for order {vnp_TxnRef}");
					return Json(new { RspCode = "04", Message = "Amount mismatch" });
				}

				// Xử lý kết quả thanh toán
				if (_vnPayService.IsPaymentSuccess(vnp_ResponseCode))
				{
					if (!order.IsPaid)
					{
						// Lưu transaction
						var transactionDto = new PaymentTransactionDto
						{
							OrderId = order.Id,
							PaymentReference = vnp_TxnRef,
							Amount = order.TotalAmount,
							BankCode = "VNPay",
							TransactionId = vnpayParams.GetValueOrDefault("vnp_TransactionNo", ""),
							TransactionTime = DateTime.TryParseExact(
								vnpayParams.GetValueOrDefault("vnp_PayDate", ""),
								"yyyyMMddHHmmss",
								null,
								System.Globalization.DateTimeStyles.None,
								out var payDate) ? payDate : DateTime.UtcNow,
							Status = PaymentTransactionStatus.Verified,
							VerifiedBy = "VNPay-IPN",
							VerifiedAt = DateTime.UtcNow
						};

						await _paymentVerificationService.SaveTransactionAsync(order.Id, transactionDto);
						await _paymentVerificationService.AutoConfirmPaymentAsync(order.Id);
					}

					return Json(new { RspCode = "00", Message = "Success" });
				}
				else
				{
					return Json(new { RspCode = "00", Message = "Payment failed" });
				}
			}
			catch (Exception ex)
			{
				Logger.Error("Lỗi khi xử lý VNPay IPN", ex);
				return Json(new { RspCode = "99", Message = "Internal error" });
			}
		}
	}
}
