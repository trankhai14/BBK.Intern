//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Abp.AspNetCore.Mvc.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using MyProject.Controllers;
//using MyProject.Orders;
//using MyProject.Payments;

//namespace MyProject.Web.Controllers
//{
//	/// <summary>
//	/// Controller để test Background Job (chỉ dùng trong Development)
//	/// </summary>
//	[AbpMvcAuthorize]
//	public class TestBackgroundJobController : MyProjectControllerBase
//	{
//		private readonly IPaymentVerificationService _paymentVerificationService;
//		private readonly IOrderAppService _orderAppService;

//		public TestBackgroundJobController(
//			IPaymentVerificationService paymentVerificationService,
//			IOrderAppService orderAppService)
//		{
//			_paymentVerificationService = paymentVerificationService;
//			_orderAppService = orderAppService;
//		}

//		/// <summary>
//		/// Test: Trigger Background Job thủ công ngay lập tức
//		/// GET: /TestBackgroundJob/TriggerJob
//		/// </summary>
//		[HttpGet]
//		public async Task<JsonResult> TriggerJob()
//		{
//			try
//			{
//				// Lấy danh sách đơn hàng cần kiểm tra
//				var pendingOrders = await _orderAppService.GetPendingUnpaidOrdersAsync();
//				var orderCount = pendingOrders?.Count ?? 0;

//				// Trigger job thủ công bằng cách gọi DoWorkAsync thông qua reflection
//				// Lưu ý: DoWorkAsync là protected, nên cần dùng cách khác
//				// Thay vào đó, ta sẽ gọi logic tương tự

//				var processedCount = 0;
//				var verifiedCount = 0;

//				if (pendingOrders != null && pendingOrders.Count > 0)
//				{
//					foreach (var order in pendingOrders)
//					{
//						try
//						{
//							var verificationResult = await _paymentVerificationService.VerifyPaymentAsync(
//								order.PaymentReference,
//								order.TotalAmount,
//								order.CreationTime.AddMinutes(-5),
//								DateTime.UtcNow
//							);

//							if (verificationResult.IsVerified)
//							{
//								var confirmed = await _paymentVerificationService.AutoConfirmPaymentAsync(order.Id);
//								if (confirmed)
//								{
//									verifiedCount++;
//								}
//							}
//							processedCount++;
//						}
//						catch (Exception ex)
//						{
//							Logger.Error($"Lỗi khi xử lý đơn hàng #{order.Id}", ex);
//						}
//					}
//				}

//				return Json(new
//				{
//					success = true,
//					message = "Background Job đã được trigger thành công",
//					data = new
//					{
//						totalOrders = orderCount,
//						processedOrders = processedCount,
//						verifiedOrders = verifiedCount,
//						timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
//					}
//				});
//			}
//			catch (Exception ex)
//			{
//				Logger.Error("Lỗi khi trigger Background Job", ex);
//				return Json(new
//				{
//					success = false,
//					message = $"Lỗi: {ex.Message}",
//					timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
//				});
//			}
//		}

//		/// <summary>
//		/// Test: Kiểm tra trạng thái Background Job
//		/// GET: /TestBackgroundJob/GetJobStatus
//		/// </summary>
//		[HttpGet]
//		public JsonResult GetJobStatus()
//		{
//			try
//			{
//				return Json(new
//				{
//					success = true,
//					data = new
//					{
//						jobName = "PaymentVerificationBackgroundJob",
//						jobType = "MyProject.Payments.BackgroundJobs.PaymentVerificationBackgroundJob",
//						period = "60000 milliseconds (1 phút)",
//						description = "Tự động kiểm tra và xác nhận thanh toán cho các đơn hàng Pending",
//						note = "Job được đăng ký tự động qua ISingletonDependency. Kiểm tra log để xem job có chạy không.",
//						timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
//					}
//				});
//			}
//			catch (Exception ex)
//			{
//				return Json(new
//				{
//					success = false,
//					message = $"Lỗi: {ex.Message}"
//				});
//			}
//		}

//		/// <summary>
//		/// Test: Lấy danh sách đơn hàng đang chờ kiểm tra
//		/// GET: /TestBackgroundJob/GetPendingOrders
//		/// </summary>
//		[HttpGet]
//		public async Task<JsonResult> GetPendingOrders()
//		{
//			try
//			{
//				var pendingOrders = await _orderAppService.GetPendingUnpaidOrdersAsync();

//				return Json(new
//				{
//					success = true,
//					data = new
//					{
//						totalCount = pendingOrders?.Count ?? 0,
//						orders = pendingOrders?.Select(o => new
//						{
//							orderId = o.Id,
//							paymentReference = o.PaymentReference,
//							totalAmount = o.TotalAmount,
//							creationTime = o.CreationTime.ToString("yyyy-MM-dd HH:mm:ss"),
//							paymentExpiredAt = o.PaymentExpiredAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A"
//						}).ToList() ?? new List<object>(),
//						timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
//					}
//				});
//			}
//			catch (Exception ex)
//			{
//				Logger.Error("Lỗi khi lấy danh sách đơn hàng Pending", ex);
//				return Json(new
//				{
//					success = false,
//					message = $"Lỗi: {ex.Message}"
//				});
//			}
//		}
//	}
//}

