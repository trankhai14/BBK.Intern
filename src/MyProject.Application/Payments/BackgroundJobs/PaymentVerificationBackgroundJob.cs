using System;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Threading;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using Microsoft.Extensions.DependencyInjection;
using MyProject.Orders;
using MyProject.Orders.Dto;
using MyProject.Payments.Dtos;
using OrderStatusEnum = MyProject.Orders.OrderStatus;

namespace MyProject.Payments.BackgroundJobs
{
	/// <summary>
	/// Background Job tự động kiểm tra và xác nhận thanh toán cho các đơn hàng chưa thanh toán
	/// 
	/// <para><strong>Chức năng chính:</strong></para>
	/// <list type="bullet">
	/// <item>Chạy định kỳ mỗi 1 phút để kiểm tra các đơn hàng đang chờ thanh toán</item>
	/// <item>Tự động tìm kiếm giao dịch từ ngân hàng (qua database hoặc API)</item>
	/// <item>Tự động xác nhận thanh toán khi tìm thấy giao dịch khớp</item>
	/// <item>Giúp giảm thiểu việc khách hàng phải click "Đã thanh toán" thủ công</item>
	/// </list>
	/// 
	/// <para><strong>Cách hoạt động:</strong></para>
	/// <list type="number">
	/// <item>Mỗi 1 phút, job này sẽ được ABP Framework tự động gọi</item>
	/// <item>Tìm tất cả đơn hàng có trạng thái Pending, chưa thanh toán và chưa hết hạn</item>
	/// <item>Với mỗi đơn hàng, kiểm tra xem có giao dịch thanh toán nào khớp không</item>
	/// <item>Nếu tìm thấy giao dịch khớp (PaymentReference và Amount), tự động xác nhận thanh toán</item>
	/// <item>Cập nhật trạng thái đơn hàng và xuất kho hàng</item>
	/// </list>
	/// 
	/// <para><strong>Lưu ý:</strong></para>
	/// <list type="bullet">
	/// <item>Job này chạy tự động, không cần can thiệp thủ công</item>
	/// <item>Nếu có lỗi ở một đơn hàng, job vẫn tiếp tục xử lý các đơn hàng khác</item>
	/// <item>Tất cả thao tác database được thực hiện trong một UnitOfWork để đảm bảo tính nhất quán</item>
	/// </list>
	/// </summary>
	public class PaymentVerificationBackgroundJob : AsyncPeriodicBackgroundWorkerBase, ISingletonDependency
	{
		#region Private Fields - Các service dependencies

		/// <summary>
		/// Service kiểm tra và xác nhận thanh toán
		/// 
		/// <para><strong>Chức năng:</strong></para>
		/// <list type="bullet">
		/// <item>Kiểm tra xem có giao dịch từ ngân hàng khớp với đơn hàng không</item>
		/// <item>Tự động xác nhận thanh toán khi tìm thấy giao dịch</item>
		/// <item>Lưu thông tin giao dịch vào database</item>
		/// </list>
		/// </summary>
		private readonly IPaymentVerificationService _paymentVerificationService;

		/// <summary>
		/// Service quản lý đơn hàng
		/// 
		/// <para><strong>Chức năng:</strong></para>
		/// <list type="bullet">
		/// <item>Lấy danh sách đơn hàng Pending chưa thanh toán</item>
		/// <item>Cập nhật trạng thái đơn hàng</item>
		/// </list>
		/// </summary>
		private readonly IOrderAppService _orderAppService;

		#endregion

		#region Constructor - Khởi tạo Background Job

		/// <summary>
		/// Constructor - Khởi tạo Background Job
		/// 
		/// <para><strong>Parameters:</strong></para>
		/// <list type="table">
		/// <item>
		/// <term>timer</term>
		/// <description>Timer của ABP để chạy job định kỳ. ABP sẽ tự động inject instance này.</description>
		/// </item>
		/// <item>
		/// <term>paymentVerificationService</term>
		/// <description>Service kiểm tra thanh toán. Được inject tự động bởi ABP DI container.</description>
		/// </item>
		/// <item>
		/// <term>orderAppService</term>
		/// <description>Service quản lý đơn hàng. Được inject tự động bởi ABP DI container.</description>
		/// </item>
		/// </list>
		/// 
		/// <para><strong>Lưu ý:</strong></para>
		/// <list type="bullet">
		/// <item>Class này implement ISingletonDependency nên chỉ có 1 instance duy nhất trong toàn bộ ứng dụng</item>
		/// <item>Timer.Period được set = 60000 (1 phút) - job sẽ chạy mỗi 1 phút một lần</item>
		/// </list>
		/// </summary>
		public PaymentVerificationBackgroundJob(
			IPaymentVerificationService paymentVerificationService,
			IOrderAppService orderAppService
		) : base(new AbpAsyncTimer { Period = 60000 })
		{
			// Lưu các service dependencies vào private fields để sử dụng trong DoWorkAsync
			_paymentVerificationService = paymentVerificationService;
			_orderAppService = orderAppService;
			// Không cần gán lại Timer vì base đã khởi tạo và set Period ở trên.
		}

		#endregion

		#region DoWorkAsync - Logic chính của Background Job

		/// <summary>
		/// Method chính được gọi định kỳ bởi ABP Framework
		/// 
		/// <para><strong>Luồng xử lý chi tiết:</strong></para>
		/// <list type="number">
		/// <item>
		/// <term>Lấy danh sách đơn hàng</term>
		/// <description>Tìm tất cả đơn hàng có OrderStatus = Pending, IsPaid = false, và chưa hết hạn thanh toán</description>
		/// </item>
		/// <item>
		/// <term>Kiểm tra từng đơn hàng</term>
		/// <description>Với mỗi đơn hàng, kiểm tra xem có giao dịch thanh toán khớp không (theo PaymentReference và Amount)</description>
		/// </item>
		/// <item>
		/// <term>Tự động xác nhận</term>
		/// <description>Nếu tìm thấy giao dịch khớp, tự động xác nhận thanh toán và cập nhật trạng thái đơn hàng</description>
		/// </item>
		/// </list>
		/// 
		/// <para><strong>Attribute [UnitOfWork]:</strong></para>
		/// <list type="bullet">
		/// <item>Đảm bảo tất cả thao tác database trong method này được thực hiện trong một transaction</item>
		/// <item>Nếu có lỗi xảy ra, tất cả thay đổi sẽ được rollback tự động</item>
		/// <item>Giúp đảm bảo tính nhất quán dữ liệu (ACID)</item>
		/// </list>
		/// 
		/// <para><strong>Xử lý lỗi:</strong></para>
		/// <list type="bullet">
		/// <item>Nếu có lỗi ở một đơn hàng, log lỗi nhưng tiếp tục xử lý các đơn hàng khác</item>
		/// <item>Nếu có lỗi tổng thể, log lỗi nhưng không throw exception để job có thể tiếp tục chạy ở lần tiếp theo</item>
		/// </list>
		/// </summary>
		[UnitOfWork]
		protected override async Task DoWorkAsync()
		{
			try
			{
				// ============================================
				// BƯỚC 1: LẤY DANH SÁCH ĐƠN HÀNG CẦN KIỂM TRA
				// ============================================
				// Tìm các đơn hàng có:
				// - OrderStatus = Pending (chờ xử lý - đơn hàng mới tạo, chưa thanh toán)
				// - IsPaid = false (chưa thanh toán)
				// - PaymentExpiredAt > DateTime.UtcNow (chưa hết hạn thanh toán - thường là 30 phút sau khi tạo đơn)
				//
				// Lý do: Chỉ kiểm tra các đơn hàng còn hiệu lực, không kiểm tra đơn hàng đã hết hạn
				// (đơn hàng hết hạn sẽ được hủy tự động bởi CancelExpiredOrders job)
				var pendingOrders = await _orderAppService.GetPendingUnpaidOrdersAsync();

				// Nếu không có đơn hàng nào cần kiểm tra, thoát sớm để tiết kiệm tài nguyên
				// Điều này giúp giảm tải cho database và server
				if (pendingOrders == null || pendingOrders.Count == 0)
				{
					return; // Không có đơn hàng nào cần kiểm tra, kết thúc job
				}

				// Log thông tin để theo dõi và debug
				// Giúp admin biết được job đang hoạt động và xử lý bao nhiêu đơn hàng
				Logger.Info($"Bắt đầu kiểm tra thanh toán cho {pendingOrders.Count} đơn hàng");

				// ============================================
				// BƯỚC 2: XỬ LÝ TỪNG ĐƠN HÀNG
				// ============================================
				// Dùng foreach để xử lý tuần tự từng đơn hàng
				// Mỗi đơn hàng được xử lý độc lập trong try-catch riêng
				// Nếu có lỗi ở một đơn hàng thì không ảnh hưởng đến các đơn hàng khác
				foreach (var order in pendingOrders)
				{
					try
					{
						// ============================================
						// BƯỚC 2.1: KIỂM TRA GIAO DỊCH THANH TOÁN
						// ============================================
						// Gọi PaymentVerificationService để tìm kiếm giao dịch trong database
						// (Hiện tại chỉ kiểm tra trong database - các giao dịch đã được lưu từ webhook hoặc import)
						// Có thể mở rộng để:
						// - Gọi API ngân hàng để kiểm tra real-time
						// - Đọc file statement từ ngân hàng
						//
						// Parameters:
						// - paymentReference: Mã tham chiếu thanh toán (VD: MP20241220120000)
						//   Được tạo khi khách hàng tạo đơn hàng, dùng để đối chiếu với giao dịch từ ngân hàng
						// - expectedAmount: Số tiền cần thanh toán (từ order.TotalAmount)
						//   Phải khớp chính xác với số tiền trong giao dịch
						// - fromDate: Thời điểm bắt đầu kiểm tra (5 phút trước khi tạo đơn)
						//   Lý do: Tránh miss giao dịch nếu có độ trễ về thời gian
						// - toDate: Thời điểm hiện tại (DateTime.UtcNow)
						//   Chỉ kiểm tra các giao dịch đến thời điểm hiện tại
						var verificationResult = await _paymentVerificationService.VerifyPaymentAsync(
							paymentReference: order.PaymentReference,
							expectedAmount: order.TotalAmount,
							fromDate: order.CreationTime.AddMinutes(-5), // Kiểm tra từ 5 phút trước khi tạo đơn
							toDate: DateTime.UtcNow
						);

						// ============================================
						// BƯỚC 2.2: XỬ LÝ KẾT QUẢ KIỂM TRA
						// ============================================
						// Nếu tìm thấy giao dịch khớp (verificationResult.IsVerified = true)
						if (verificationResult.IsVerified)
						{
							// Tự động xác nhận thanh toán cho đơn hàng này
							// Method AutoConfirmPaymentAsync sẽ thực hiện các bước sau:
							// 1. Lưu giao dịch vào database (nếu chưa có)
							// 2. Commit Reserved Inventory (xuất hàng thực sự - giảm Quantity và ReservedQuantity)
							// 3. Cập nhật Order:
							//    - IsPaid = true
							//    - PaidTime = TransactionTime
							//    - OrderStatus = Confirmed (từ Pending chuyển sang Confirmed)
							var confirmed = await _paymentVerificationService.AutoConfirmPaymentAsync(order.Id);

							// Kiểm tra kết quả xác nhận
							if (confirmed)
							{
								// Xác nhận thành công - log thông tin để theo dõi
								Logger.Info($"Đã tự động xác nhận thanh toán cho đơn hàng #{order.Id} (PaymentReference: {order.PaymentReference})");
							}
							else
							{
								// Xác nhận thất bại (có thể đã được xác nhận trước đó bởi webhook hoặc user)
								Logger.Warn($"Không thể xác nhận thanh toán cho đơn hàng #{order.Id} (có thể đã được xác nhận trước đó)");
							}
						}
						// Nếu không tìm thấy giao dịch (verificationResult.IsVerified = false)
						// Không làm gì, job sẽ kiểm tra lại ở lần chạy tiếp theo (1 phút sau)
						// Điều này cho phép hệ thống tự động phát hiện thanh toán ngay khi có giao dịch
					}
					catch (Exception ex)
					{
						// ============================================
						// XỬ LÝ LỖI CHO TỪNG ĐƠN HÀNG
						// ============================================
						// Nếu có lỗi khi xử lý một đơn hàng cụ thể:
						// - Log lỗi chi tiết (bao gồm OrderId và exception message)
						// - KHÔNG throw exception để không dừng việc xử lý các đơn hàng khác
						// - Tiếp tục với đơn hàng tiếp theo trong danh sách
						//
						// Ví dụ lỗi có thể xảy ra:
						// - Lỗi kết nối database
						// - Lỗi khi commit inventory
						// - Lỗi khi cập nhật order
						Logger.Error($"Lỗi khi kiểm tra thanh toán cho đơn hàng #{order.Id}: {ex.Message}", ex);
						// Tiếp tục với đơn hàng tiếp theo
					}
				}
			}
			catch (Exception ex)
			{
				// ============================================
				// XỬ LÝ LỖI TỔNG THỂ
				// ============================================
				// Nếu có lỗi ở mức tổng thể (ví dụ: lỗi khi lấy danh sách đơn hàng):
				// - Log lỗi để admin biết
				// - KHÔNG throw exception để job có thể tiếp tục chạy ở lần tiếp theo
				// - Nếu throw exception, ABP có thể dừng job vĩnh viễn
				Logger.Error("Lỗi trong PaymentVerificationBackgroundJob", ex);
				// Không throw exception để job có thể tiếp tục chạy ở lần tiếp theo
			}
		}

		#endregion
	}
}
