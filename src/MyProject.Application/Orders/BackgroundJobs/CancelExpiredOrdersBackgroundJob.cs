using System;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Threading;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using MyProject.Orders;

namespace MyProject.Orders.BackgroundJobs
{
	/// <summary>
	/// Background Job tự động hủy các đơn hàng đã hết hạn thanh toán
	/// 
	/// <para><strong>Chức năng chính:</strong></para>
	/// <list type="bullet">
	/// <item>Chạy định kỳ mỗi 5 phút để kiểm tra các đơn hàng đã hết hạn thanh toán</item>
	/// <item>Tự động hủy các đơn hàng có PaymentExpiredAt đã qua</item>
	/// <item>Tự động giải phóng inventory đã reserve cho các đơn hàng bị hủy</item>
	/// <item>Giúp quản lý kho hàng hiệu quả, tránh giữ hàng quá lâu cho đơn hàng không thanh toán</item>
	/// </list>
	/// 
	/// <para><strong>Cách hoạt động:</strong></para>
	/// <list type="number">
	/// <item>Mỗi 5 phút, job này sẽ được ABP Framework tự động gọi</item>
	/// <item>Tìm tất cả đơn hàng có: OrderStatus = Pending, IsPaid = false, PaymentExpiredAt &lt; DateTime.UtcNow</item>
	/// <item>Với mỗi đơn hàng hết hạn:</item>
	/// <item>  - Giải phóng inventory đã reserve (ReleaseReservedInventory)</item>
	/// <item>  - Cập nhật OrderStatus = Canceled</item>
	/// <item>  - Ghi log để theo dõi</item>
	/// </list>
	/// 
	/// <para><strong>Lưu ý:</strong></para>
	/// <list type="bullet">
	/// <item>Job này chạy tự động, không cần can thiệp thủ công</item>
	/// <item>Nếu có lỗi ở một đơn hàng, job vẫn tiếp tục xử lý các đơn hàng khác</item>
	/// <item>Thời gian hết hạn mặc định là 30 phút sau khi tạo đơn (PaymentExpiredAt = CreationTime + 30 phút)</item>
	/// </list>
	/// </summary>
	public class CancelExpiredOrdersBackgroundJob : AsyncPeriodicBackgroundWorkerBase, ISingletonDependency
	{
		#region Private Fields

		/// <summary>
		/// Service quản lý đơn hàng
		/// </summary>
		private readonly IOrderAppService _orderAppService;

		#endregion

		#region Constructor

		/// <summary>
		/// Constructor - Khởi tạo Background Job
		/// </summary>
		/// <param name="orderAppService">Service quản lý đơn hàng</param>
		public CancelExpiredOrdersBackgroundJob(
			IOrderAppService orderAppService
		) : base(new AbpAsyncTimer { Period = 300000 }) // Chạy mỗi 5 phút (300000 ms)
		{
			_orderAppService = orderAppService;
		}

		#endregion

		#region DoWorkAsync

		/// <summary>
		/// Method chính được gọi định kỳ bởi ABP Framework
		/// </summary>
		protected override async Task DoWorkAsync()
		{
			try
			{
				Logger.Info("Bắt đầu kiểm tra và hủy các đơn hàng đã hết hạn thanh toán");

				// Gọi method CancelExpiredOrders từ OrderAppService
				var canceledCount = await _orderAppService.CancelExpiredOrders();

				if (canceledCount > 0)
				{
					Logger.Info($"Đã tự động hủy {canceledCount} đơn hàng do hết hạn thanh toán");
				}
				else
				{
					Logger.Debug("Không có đơn hàng nào hết hạn thanh toán");
				}
			}
			catch (Exception ex)
			{
				Logger.Error("Lỗi khi chạy job hủy đơn hàng hết hạn", ex);
			}
		}

		#endregion
	}
}






