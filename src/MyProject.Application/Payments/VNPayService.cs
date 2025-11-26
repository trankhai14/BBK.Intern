using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace MyProject.Payments
{
	/// <summary>
	/// Service xử lý tích hợp VNPay
	/// </summary>
	public class VNPayService
	{
		private readonly IConfiguration _configuration;
		private readonly string _tmnCode;
		private readonly string _hashSecret;
		private readonly string _paymentUrl;
		private readonly string _returnUrl;
		private readonly string _ipnUrl;

		public VNPayService(IConfiguration configuration)
		{
			_configuration = configuration;
			_tmnCode = _configuration["VNPay:TmnCode"] ?? "";
			_hashSecret = _configuration["VNPay:HashSecret"] ?? "";
			_paymentUrl = _configuration["VNPay:PaymentUrl"] ?? "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
			_returnUrl = _configuration["VNPay:ReturnUrl"] ?? "";
			_ipnUrl = _configuration["VNPay:IpnUrl"] ?? "";
		}

		/// <summary>
		/// Tạo payment URL để redirect khách hàng đến VNPay
		/// </summary>
		/// <param name="orderId">ID đơn hàng</param>
		/// <param name="amount">Số tiền (VND)</param>
		/// <param name="orderInfo">Mô tả đơn hàng</param>
		/// <param name="paymentReference">Mã tham chiếu thanh toán</param>
		/// <param name="clientIp">IP khách hàng</param>
		/// <returns>Payment URL</returns>
		public string CreatePaymentUrl(
			int orderId,
			decimal amount,
			string orderInfo,
			string paymentReference,
			string clientIp)
		{
			// VNPay yêu cầu số tiền × 100 (ví dụ: 10,000 VND → 1000000)
			var vnpAmount = (long)(amount * 100);

			// Tạo các tham số
			var vnpParams = new Dictionary<string, string>
			{
				{ "vnp_Version", _configuration["VNPay:Version"] ?? "2.1.0" },
				{ "vnp_Command", _configuration["VNPay:Command"] ?? "pay" },
				{ "vnp_TmnCode", _tmnCode },
				{ "vnp_Amount", vnpAmount.ToString() },
				{ "vnp_CurrCode", _configuration["VNPay:CurrCode"] ?? "VND" },
				{ "vnp_TxnRef", paymentReference },
				{ "vnp_OrderInfo", orderInfo },
				{ "vnp_OrderType", "other" },
				{ "vnp_Locale", _configuration["VNPay:Locale"] ?? "vn" },
				{ "vnp_ReturnUrl", _returnUrl },
				{ "vnp_IpAddr", clientIp },
				{ "vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss") }
			};

			// Sắp xếp theo thứ tự alphabet
			var sortedParams = vnpParams.OrderBy(x => x.Key).ToList();

			// Tạo query string
			var queryString = string.Join("&", sortedParams.Select(x => $"{x.Key}={WebUtility.UrlEncode(x.Value)}"));

			// Tạo chữ ký
			var secureHash = CreateSecureHash(queryString);

			// Thêm chữ ký vào query string
			var paymentUrl = $"{_paymentUrl}?{queryString}&vnp_SecureHash={secureHash}";

			return paymentUrl;
		}

		/// <summary>
		/// Xác thực chữ ký từ VNPay
		/// </summary>
		/// <param name="vnpayParams">Dictionary chứa các tham số từ VNPay</param>
		/// <param name="secureHash">Chữ ký từ VNPay</param>
		/// <returns>True nếu chữ ký hợp lệ</returns>
		public bool ValidateSignature(Dictionary<string, string> vnpayParams, string secureHash)
		{
			// Loại bỏ vnp_SecureHash và vnp_SecureHashType
			var filteredParams = vnpayParams
				.Where(x => x.Key != "vnp_SecureHash" && x.Key != "vnp_SecureHashType")
				.ToDictionary(x => x.Key, x => x.Value);

			// Sắp xếp theo thứ tự alphabet
			var sortedParams = filteredParams.OrderBy(x => x.Key).ToList();

			// Tạo query string
			var queryString = string.Join("&", sortedParams.Select(x => $"{x.Key}={x.Value}"));

			// Tạo chữ ký
			var calculatedHash = CreateSecureHash(queryString);

			// So sánh chữ ký
			return calculatedHash.Equals(secureHash, StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>
		/// Tạo chữ ký HMAC SHA512
		/// </summary>
		/// <param name="data">Dữ liệu cần ký</param>
		/// <returns>Chữ ký (chữ hoa)</returns>
		private string CreateSecureHash(string data)
		{
			using (var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(_hashSecret)))
			{
				var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
				return BitConverter.ToString(hashBytes).Replace("-", "").ToUpper();
			}
		}

		/// <summary>
		/// Parse các tham số từ VNPay response
		/// </summary>
		/// <param name="queryString">Query string từ VNPay</param>
		/// <returns>Dictionary chứa các tham số</returns>
		public Dictionary<string, string> ParseResponse(string queryString)
		{
			var result = new Dictionary<string, string>();

			if (string.IsNullOrEmpty(queryString))
				return result;

			var pairs = queryString.Split('&');
			foreach (var pair in pairs)
			{
				var keyValue = pair.Split('=');
				if (keyValue.Length == 2)
				{
					var key = WebUtility.UrlDecode(keyValue[0]);
					var value = WebUtility.UrlDecode(keyValue[1]);
					result[key] = value;
				}
			}

			return result;
		}

		/// <summary>
		/// Kiểm tra mã phản hồi từ VNPay
		/// </summary>
		/// <param name="responseCode">Mã phản hồi</param>
		/// <returns>True nếu thanh toán thành công</returns>
		public bool IsPaymentSuccess(string responseCode)
		{
			return responseCode == "00";
		}

		/// <summary>
		/// Lấy thông báo lỗi từ mã phản hồi
		/// </summary>
		/// <param name="responseCode">Mã phản hồi</param>
		/// <returns>Thông báo lỗi</returns>
		public string GetResponseMessage(string responseCode)
		{
			return responseCode switch
			{
				"00" => "Giao dịch thành công",
				"07" => "Trừ tiền thành công. Giao dịch bị nghi ngờ (liên quan tới lừa đảo, giao dịch bất thường)",
				"09" => "Thẻ/Tài khoản chưa đăng ký dịch vụ InternetBanking",
				"10" => "Xác thực thông tin thẻ/tài khoản không đúng quá 3 lần",
				"11" => "Đã hết hạn chờ thanh toán. Xin vui lòng thực hiện lại giao dịch",
				"12" => "Thẻ/Tài khoản bị khóa",
				"51" => "Tài khoản không đủ số dư để thực hiện giao dịch",
				"65" => "Tài khoản đã vượt quá hạn mức giao dịch trong ngày",
				"75" => "Ngân hàng thanh toán đang bảo trì",
				"79" => "Nhập sai mật khẩu thanh toán quá số lần quy định",
				_ => $"Mã lỗi không xác định: {responseCode}"
			};
		}
	}
}

