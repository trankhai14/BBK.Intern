using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Timing;
using MyProject.Products;

namespace MyProject.Products
{
	[Table("AppProductSpecifications")]
	public class ProductSpecification : Entity, IHasCreationTime, IHasModificationTime
	{
		public const int MaxSkuLength = 100;
		public const int MaxModelNumberLength = 100;
		public const int MaxChipsetLength = 256;
		public const int MaxRamLength = 100;
		public const int MaxStorageLength = 200;
		public const int MaxScreenLength = 300;
		public const int MaxOsLength = 200;
		public const int MaxBatteryLength = 100;
		public const int MaxColorLength = 100;
		public const int MaxWarrantyLength = 200;
		public const int MaxCameraLength = 500;
		public const int MaxFrontCameraLength = 300;
		public const int MaxSimLength = 100;
		public const int MaxConnectivityLength = 300;
		public const int MaxSecurityLength = 200;
		public const int MaxChargingLength = 200;
		public const int MaxPortLength = 100;

		/// <summary>
		/// ID sản phẩm (Foreign Key) - Quan hệ 1:1 với Product
		/// </summary>
		[Required]
		public int ProductId { get; set; }

		[ForeignKey("ProductId")]
		public Product Product { get; set; }

		/// <summary>
		/// Mã sản phẩm/SKU (Stock Keeping Unit) - Mã định danh duy nhất cho sản phẩm
		/// </summary>
		[StringLength(MaxSkuLength)]
		public string Sku { get; set; }

		/// <summary>
		/// Số model của điện thoại - Ví dụ: iPhone 15 Pro Max, Samsung Galaxy S24 Ultra
		/// </summary>
		[StringLength(MaxModelNumberLength)]
		public string ModelNumber { get; set; }

		/// <summary>
		/// Chip xử lý - Ví dụ: Apple A17 Pro, Snapdragon 8 Gen 3, MediaTek Dimensity 9300
		/// </summary>
		[StringLength(MaxChipsetLength)]
		public string Chipset { get; set; }

		/// <summary>
		/// Bộ nhớ RAM - Ví dụ: 8GB, 12GB, 16GB
		/// </summary>
		[StringLength(MaxRamLength)]
		public string Ram { get; set; }

		/// <summary>
		/// Bộ nhớ trong - Ví dụ: 128GB, 256GB, 512GB, 1TB
		/// </summary>
		[StringLength(MaxStorageLength)]
		public string Storage { get; set; }

		/// <summary>
		/// Thông tin màn hình - Ví dụ: 6.7 inch Super Retina XDR OLED, 6.8 inch Dynamic AMOLED 2X 120Hz
		/// </summary>
		[StringLength(MaxScreenLength)]
		public string Screen { get; set; }

		/// <summary>
		/// Hệ điều hành - Ví dụ: iOS 17, Android 14, HarmonyOS
		/// </summary>
		[StringLength(MaxOsLength)]
		public string OperatingSystem { get; set; }

		/// <summary>
		/// Dung lượng pin (mAh) - Ví dụ: 4422 mAh, 5000 mAh
		/// </summary>
		[StringLength(MaxBatteryLength)]
		public string Battery { get; set; }

		/// <summary>
		/// Camera sau - Ví dụ: 48MP + 12MP + 12MP, Triple camera 50MP + 12MP + 12MP
		/// </summary>
		[StringLength(MaxCameraLength)]
		public string Camera { get; set; }

		/// <summary>
		/// Camera trước - Ví dụ: 12MP, 32MP, Dual camera 12MP + ToF
		/// </summary>
		[StringLength(MaxFrontCameraLength)]
		public string FrontCamera { get; set; }

		/// <summary>
		/// Thông tin SIM - Ví dụ: Dual SIM (nano-SIM), 1 eSIM + 1 nano-SIM, 2 nano-SIM
		/// </summary>
		[StringLength(MaxSimLength)]
		public string Sim { get; set; }

		/// <summary>
		/// Kết nối - Ví dụ: 5G, 4G LTE, Wi-Fi 6E, Bluetooth 5.3, NFC, GPS
		/// </summary>
		[StringLength(MaxConnectivityLength)]
		public string Connectivity { get; set; }

		/// <summary>
		/// Bảo mật - Ví dụ: Face ID, Vân tay dưới màn hình, Vân tay cạnh viền, Mở khóa khuôn mặt
		/// </summary>
		[StringLength(MaxSecurityLength)]
		public string Security { get; set; }

		/// <summary>
		/// Sạc nhanh - Ví dụ: Sạc nhanh 25W, Sạc không dây 15W, Sạc ngược
		/// </summary>
		[StringLength(MaxChargingLength)]
		public string Charging { get; set; }

		/// <summary>
		/// Cổng sạc - Ví dụ: USB-C, Lightning, USB-C 3.1
		/// </summary>
		[StringLength(MaxPortLength)]
		public string ChargingPort { get; set; }

		/// <summary>
		/// Màu sắc sản phẩm - Ví dụ: Đen Titanium, Bạc, Xanh, Tím, Trắng
		/// </summary>
		[StringLength(MaxColorLength)]
		public string Color { get; set; }

		/// <summary>
		/// Thông tin bảo hành - Ví dụ: 12 tháng, 24 tháng, Bảo hành chính hãng
		/// </summary>
		[StringLength(MaxWarrantyLength)]
		public string Warranty { get; set; }

		/// <summary>
		/// Thông tin kỹ thuật chi tiết bổ sung (JSON hoặc text)
		/// Dùng để lưu các thông tin kỹ thuật phức tạp hoặc không có trường riêng
		/// Ví dụ: Cảm biến, Loa, Chuẩn chống nước, Trọng lượng, Kích thước chi tiết
		/// </summary>
		public string TechnicalSpecifications { get; set; }

		public DateTime CreationTime { get; set; }
		public DateTime? LastModificationTime { get; set; }
	}
}

