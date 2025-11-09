using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using MyProject.FlashSales.Dto;
using MyProject.Inventories;
using MyProject.Products;

namespace MyProject.FlashSales
{
	/// <summary>
	/// Application Service cho quản lý FlashSale
	/// Xử lý các nghiệp vụ: tạo, sửa, xóa FlashSale và quản lý sản phẩm trong FlashSale
	/// Tích hợp với Inventory để khóa và hoàn trả số lượng sản phẩm
	/// </summary>
	public class FlashSaleAppService : MyProjectAppServiceBase, IFlashSaleAppService
	{
		#region Private Fields

		/// <summary>
		/// Repository cho FlashSale entity - dùng để truy vấn và thao tác với bảng FlashSales
		/// </summary>
		private readonly IRepository<FlashSale> _flashSaleRepository;

		/// <summary>
		/// Repository cho FlashSaleProduct entity - dùng để quản lý sản phẩm trong FlashSale
		/// </summary>
		private readonly IRepository<FlashSaleProduct> _flashSaleProductRepository;

		/// <summary>
		/// Repository cho Product entity - dùng để kiểm tra thông tin sản phẩm
		/// </summary>
		private readonly IRepository<MyProject.Products.Product> _productRepository;

		/// <summary>
		/// Repository cho Inventory entity - dùng để quản lý số lượng tồn kho và khóa số lượng
		/// </summary>
		private readonly IRepository<Inventory> _inventoryRepository;

		#endregion

		#region Constructor

		/// <summary>
		/// Constructor - Khởi tạo các repository dependencies
		/// Được gọi tự động bởi Dependency Injection container của ABP Framework
		/// </summary>
		/// <param name="flashSaleRepository">Repository cho FlashSale</param>
		/// <param name="flashSaleProductRepository">Repository cho FlashSaleProduct</param>
		/// <param name="productRepository">Repository cho Product</param>
		/// <param name="inventoryRepository">Repository cho Inventory</param>
		public FlashSaleAppService(
			IRepository<FlashSale> flashSaleRepository,
			IRepository<FlashSaleProduct> flashSaleProductRepository,
			IRepository<MyProject.Products.Product> productRepository,
			IRepository<Inventory> inventoryRepository)
		{
			_flashSaleRepository = flashSaleRepository;
			_flashSaleProductRepository = flashSaleProductRepository;
			_productRepository = productRepository;
			_inventoryRepository = inventoryRepository;
		}

		#endregion

		#region Public Methods - FlashSale Management

		/// <summary>
		/// Lấy danh sách FlashSale có phân trang và tìm kiếm
		/// Được sử dụng trong: FlashSales/Index.cshtml - hiển thị danh sách FlashSale trong admin panel
		/// </summary>
		/// <param name="input">Input chứa thông tin tìm kiếm, lọc, phân trang</param>
		/// <returns>Danh sách FlashSale có phân trang</returns>
		public async Task<PagedResultDto<FlashSaleDto>> GetAll(GetAllFlashSalesInput input)
		{
			// Include để tính TotalProducts và TotalSold
			var query = _flashSaleRepository.GetAll()
				.Include(fs => fs.FlashSaleProducts).AsQueryable();

			// Filter by keyword - Tìm kiếm theo tên hoặc mô tả
			if (!string.IsNullOrWhiteSpace(input.Keyword))
			{
				string keywordLower = input.Keyword.ToLower();
				query = query.Where(fs => fs.Name.ToLower().Contains(keywordLower) ||
										  (fs.Description != null && fs.Description.ToLower().Contains(keywordLower)));
			}

			// Filter by status - Lọc theo trạng thái (NotStarted, Ongoing, Ended, Cancelled)
			if (input.Status.HasValue)
			{
				query = query.Where(fs => fs.Status == (FlashSaleStatus)input.Status.Value);
			}

			// Filter by IsActive - Lọc theo trạng thái active/inactive
			if (input.IsActive.HasValue)
			{
				query = query.Where(fs => fs.IsActive == input.IsActive.Value);
			}

			// Filter by IsHidden - Lọc theo trạng thái ẩn/hiện
			if (input.IsHidden.HasValue)
			{
				query = query.Where(fs => fs.IsHidden == input.IsHidden.Value);
			}

			// Get total count - Đếm tổng số bản ghi trước khi phân trang
			var totalCount = await query.CountAsync();

			// Apply sorting - Sắp xếp theo thời gian bắt đầu (mới nhất trước)
			if (string.IsNullOrWhiteSpace(input.Sorting))
			{
				input.Sorting = "StartTime DESC";
			}

			// Apply pagination and get results - Lấy dữ liệu có phân trang
			// Lưu ý: StatusText được tính sau khi query để tránh lỗi EF Core client projection
			var flashSaleDtos = await query
				.OrderByDescending(fs => fs.StartTime)
				.PageBy(input)
				.Select(fs => new FlashSaleDto
				{
					Id = fs.Id,
					Name = fs.Name,
					Description = fs.Description,
					StartTime = fs.StartTime,
					EndTime = fs.EndTime,
					Status = fs.Status,
					IsActive = fs.IsActive,
					IsHidden = fs.IsHidden,
					CreationTime = fs.CreationTime,
					LastModificationTime = fs.LastModificationTime,
					TotalProducts = fs.FlashSaleProducts.Count,
					TotalSold = fs.FlashSaleProducts.Sum(p => p.SoldQuantity)
				})
				.ToListAsync();

			// Tính StatusText sau khi query (trong memory) để tránh lỗi EF Core client projection
			// EF Core không thể dịch instance method thành SQL, nên phải tính sau khi query
			foreach (var dto in flashSaleDtos)
			{
				dto.StatusText = GetStatusText(dto.Status);
			}

			return new PagedResultDto<FlashSaleDto>(totalCount, flashSaleDtos);
		}

		/// <summary>
		/// Lấy thông tin chi tiết FlashSale theo ID
		/// Được sử dụng trong: FlashSales/Detail.cshtml - hiển thị chi tiết và quản lý sản phẩm
		/// </summary>
		/// <param name="id">ID của FlashSale cần lấy</param>
		/// <returns>Thông tin chi tiết FlashSale bao gồm danh sách sản phẩm</returns>
		public async Task<FlashSaleDto> GetById(int id)
		{
			// Lấy FlashSale kèm theo danh sách sản phẩm và thông tin Product
			var flashSale = await _flashSaleRepository.GetAll()
				.Include(fs => fs.FlashSaleProducts)
				.ThenInclude(fsp => fsp.Product)
				.FirstOrDefaultAsync(fs => fs.Id == id);

			if (flashSale == null)
			{
				throw new UserFriendlyException("Không tìm thấy chương trình FlashSale");
			}

			// Map entity sang DTO
			var dto = new FlashSaleDto
			{
				Id = flashSale.Id,
				Name = flashSale.Name,
				Description = flashSale.Description,
				StartTime = flashSale.StartTime,
				EndTime = flashSale.EndTime,
				Status = flashSale.Status,
				IsActive = flashSale.IsActive,
				IsHidden = flashSale.IsHidden,
				CreationTime = flashSale.CreationTime,
				LastModificationTime = flashSale.LastModificationTime,
				TotalProducts = flashSale.FlashSaleProducts.Count,
				TotalSold = flashSale.FlashSaleProducts.Sum(p => p.SoldQuantity),
				Products = flashSale.FlashSaleProducts.Select(fsp => new FlashSaleProductDto
				{
					Id = fsp.Id,
					FlashSaleId = fsp.FlashSaleId,
					ProductId = fsp.ProductId,
					ProductName = fsp.Product.Name,
					ProductImage = fsp.Product.Image,
					OriginalPrice = fsp.Product.Price,
					FlashSalePrice = fsp.FlashSalePrice,
					FlashSaleQuantity = fsp.FlashSaleQuantity,
					SoldQuantity = fsp.SoldQuantity,
					RemainingQuantity = fsp.RemainingQuantity,
					MaxQuantityPerUser = fsp.MaxQuantityPerUser,
					ReservedQuantity = fsp.ReservedQuantity,
					IsReturnedToInventory = fsp.IsReturnedToInventory
				}).ToList()
			};

			// Tính StatusText sau khi query (trong memory) để tránh lỗi EF Core client projection
			dto.StatusText = GetStatusText(dto.Status);

			return dto;
		}

		/// <summary>
		/// Tạo mới FlashSale
		/// Được sử dụng trong: FlashSales/_CreateModal.cshtml - form tạo mới FlashSale
		/// </summary>
		/// <param name="input">Thông tin FlashSale cần tạo</param>
		/// <returns>FlashSale vừa tạo</returns>
		public async Task<FlashSaleDto> Create(CreateFlashSaleDto input)
		{
			// Validate thời gian - Kiểm tra thời gian kết thúc phải sau thời gian bắt đầu
			if (input.StartTime >= input.EndTime)
			{
				throw new UserFriendlyException("Thời gian kết thúc phải sau thời gian bắt đầu");
			}

			// Validate thời gian bắt đầu - Không được trong quá khứ
			if (input.StartTime < DateTime.Now)
			{
				throw new UserFriendlyException("Thời gian bắt đầu không được trong quá khứ");
			}

			// Tạo entity mới với trạng thái NotStarted
			var flashSale = new FlashSale
			{
				Name = input.Name,
				Description = input.Description,
				StartTime = input.StartTime,
				EndTime = input.EndTime,
				IsActive = input.IsActive,
				IsHidden = input.IsHidden,
				Status = FlashSaleStatus.NotStarted // Mặc định là chưa bắt đầu
			};

			// Lưu vào database
			await _flashSaleRepository.InsertAsync(flashSale);
			await CurrentUnitOfWork.SaveChangesAsync();

			// Trả về DTO với đầy đủ thông tin
			return await GetById(flashSale.Id);
		}

		/// <summary>
		/// Cập nhật FlashSale
		/// Được sử dụng trong: FlashSales/_EditModal.cshtml - form chỉnh sửa FlashSale
		/// Lưu ý: Chỉ cho phép sửa khi FlashSale chưa bắt đầu hoặc đã kết thúc
		/// </summary>
		/// <param name="input">Thông tin FlashSale cần cập nhật</param>
		/// <returns>FlashSale đã cập nhật</returns>
		public async Task<FlashSaleDto> Update(UpdateFlashSaleDto input)
		{
			var flashSale = await _flashSaleRepository.GetAsync(input.Id);

			// Validate thời gian - Kiểm tra thời gian kết thúc phải sau thời gian bắt đầu
			if (input.StartTime >= input.EndTime)
			{
				throw new UserFriendlyException("Thời gian kết thúc phải sau thời gian bắt đầu");
			}

			// Kiểm tra trạng thái - Chỉ cho phép sửa nếu chưa bắt đầu hoặc đã kết thúc
			// Không cho phép sửa khi đang diễn ra để tránh ảnh hưởng đến đơn hàng đang xử lý
			if (flashSale.Status == FlashSaleStatus.Ongoing)
			{
				throw new UserFriendlyException("Không thể sửa chương trình FlashSale đang diễn ra");
			}

			// Cập nhật thông tin
			flashSale.Name = input.Name;
			flashSale.Description = input.Description;
			flashSale.StartTime = input.StartTime;
			flashSale.EndTime = input.EndTime;
			flashSale.IsActive = input.IsActive;
			flashSale.IsHidden = input.IsHidden;

			// Cập nhật trạng thái dựa trên thời gian hiện tại
			// Sử dụng property CalculatedStatus để tự động tính toán
			flashSale.Status = flashSale.CalculatedStatus;

			// Lưu vào database
			await _flashSaleRepository.UpdateAsync(flashSale);
			await CurrentUnitOfWork.SaveChangesAsync();

			// Trả về DTO với đầy đủ thông tin
			return await GetById(flashSale.Id);
		}

		/// <summary>
		/// Xóa FlashSale
		/// Được sử dụng trong: FlashSales/Index.js - xử lý sự kiện xóa FlashSale
		/// Lưu ý: Tự động hoàn trả số lượng sản phẩm về Inventory trước khi xóa
		/// </summary>
		/// <param name="id">ID của FlashSale cần xóa</param>
		public async Task Delete(int id)
		{
			// Lấy FlashSale kèm theo danh sách sản phẩm
			var flashSale = await _flashSaleRepository.GetAll()
				.Include(fs => fs.FlashSaleProducts)
				.FirstOrDefaultAsync(fs => fs.Id == id);

			if (flashSale == null)
			{
				throw new UserFriendlyException("Không tìm thấy chương trình FlashSale");
			}

			// Kiểm tra trạng thái - Không cho phép xóa khi đang diễn ra
			// Để tránh ảnh hưởng đến đơn hàng đang xử lý
			if (flashSale.Status == FlashSaleStatus.Ongoing)
			{
				throw new UserFriendlyException("Không thể xóa chương trình FlashSale đang diễn ra");
			}

			// Hoàn trả số lượng về Inventory trước khi xóa
			// Đảm bảo số lượng được khóa trước đó được giải phóng về kho chính
			foreach (var product in flashSale.FlashSaleProducts)
			{
				if (!product.IsReturnedToInventory && product.ReservedQuantity > 0)
				{
					await ReturnProductQuantityToInventory(product);
				}
			}

			// Xóa FlashSale (cascade delete sẽ xóa các FlashSaleProduct)
			await _flashSaleRepository.DeleteAsync(flashSale);
		}

		/// <summary>
		/// Bật/tắt trạng thái ẩn/hiện của FlashSale
		/// Được sử dụng trong: FlashSales/Index.js - xử lý sự kiện toggle hide/show
		/// </summary>
		/// <param name="id">ID của FlashSale cần thay đổi trạng thái</param>
		public async Task ToggleHide(int id)
		{
			var flashSale = await _flashSaleRepository.GetAsync(id);
			// Đảo ngược trạng thái ẩn/hiện
			flashSale.IsHidden = !flashSale.IsHidden;
			await _flashSaleRepository.UpdateAsync(flashSale);
			await CurrentUnitOfWork.SaveChangesAsync();
		}

		#endregion

		#region Public Methods - FlashSale Product Management

		/// <summary>
		/// Thêm sản phẩm vào FlashSale
		/// Được sử dụng trong: FlashSales/Detail.cshtml - form thêm sản phẩm vào FlashSale
		/// Tự động khóa số lượng sản phẩm trong Inventory (tăng ReservedQuantity)
		/// </summary>
		/// <param name="input">Thông tin sản phẩm cần thêm: FlashSaleId, ProductId, FlashSalePrice, FlashSaleQuantity, MaxQuantityPerUser</param>
		/// <returns>FlashSaleProduct vừa tạo</returns>
		public async Task<FlashSaleProductDto> AddProduct(AddProductToFlashSaleDto input)
		{
			var flashSale = await _flashSaleRepository.GetAsync(input.FlashSaleId);
			var product = await _productRepository.GetAsync(input.ProductId);

			// Kiểm tra sản phẩm đã có trong FlashSale chưa - Mỗi sản phẩm chỉ được thêm 1 lần
			var existingProduct = await _flashSaleProductRepository.FirstOrDefaultAsync(
				fsp => fsp.FlashSaleId == input.FlashSaleId && fsp.ProductId == input.ProductId);

			if (existingProduct != null)
			{
				throw new UserFriendlyException("Sản phẩm này đã có trong chương trình FlashSale");
			}

			// Kiểm tra số lượng trong Inventory - Đảm bảo có đủ số lượng để khóa
			var inventory = await _inventoryRepository.FirstOrDefaultAsync(i => i.ProductId == input.ProductId);
			if (inventory == null)
			{
				throw new UserFriendlyException("Không tìm thấy kho hàng cho sản phẩm này");
			}

			// Tính số lượng khả dụng = Tổng số lượng - Số lượng đã khóa
			var availableQuantity = inventory.Quantity - inventory.ReservedQuantity;
			if (availableQuantity < input.FlashSaleQuantity)
			{
				throw new UserFriendlyException($"Số lượng khả dụng không đủ. Hiện có: {availableQuantity}, yêu cầu: {input.FlashSaleQuantity}");
			}

			// Khóa số lượng trong Inventory - Tăng ReservedQuantity để tránh bán số lượng này trong kho chính
			inventory.ReservedQuantity += input.FlashSaleQuantity;
			await _inventoryRepository.UpdateAsync(inventory);

			// Tạo FlashSaleProduct với số lượng và giá FlashSale
			var flashSaleProduct = new FlashSaleProduct
			{
				FlashSaleId = input.FlashSaleId,
				ProductId = input.ProductId,
				FlashSalePrice = input.FlashSalePrice,
				FlashSaleQuantity = input.FlashSaleQuantity,
				MaxQuantityPerUser = input.MaxQuantityPerUser,
				ReservedQuantity = input.FlashSaleQuantity // Lưu số lượng đã khóa để có thể hoàn trả sau
			};

			await _flashSaleProductRepository.InsertAsync(flashSaleProduct);
			await CurrentUnitOfWork.SaveChangesAsync();

			return await GetFlashSaleProductDto(flashSaleProduct.Id);
		}

		/// <summary>
		/// Xóa sản phẩm khỏi FlashSale
		/// Được sử dụng trong: FlashSales/Detail.js - xử lý sự kiện xóa sản phẩm
		/// Tự động hoàn trả số lượng chưa bán về Inventory
		/// </summary>
		/// <param name="flashSaleProductId">ID của FlashSaleProduct cần xóa</param>
		public async Task RemoveProduct(int flashSaleProductId)
		{
			// Lấy FlashSaleProduct kèm theo thông tin FlashSale
			var flashSaleProduct = await _flashSaleProductRepository.GetAll()
				.Include(fsp => fsp.FlashSale)
				.FirstOrDefaultAsync(fsp => fsp.Id == flashSaleProductId);

			if (flashSaleProduct == null)
			{
				throw new UserFriendlyException("Không tìm thấy sản phẩm trong FlashSale");
			}

			// Kiểm tra trạng thái - Nếu FlashSale đang diễn ra và đã có người mua, không cho phép xóa
			// Để tránh ảnh hưởng đến đơn hàng đang xử lý
			if (flashSaleProduct.FlashSale.Status == FlashSaleStatus.Ongoing && flashSaleProduct.SoldQuantity > 0)
			{
				throw new UserFriendlyException("Không thể xóa sản phẩm đã có người mua trong FlashSale đang diễn ra");
			}

			// Hoàn trả số lượng về Inventory - Giải phóng số lượng đã khóa
			// Chỉ hoàn trả nếu chưa được hoàn trả trước đó
			if (!flashSaleProduct.IsReturnedToInventory && flashSaleProduct.ReservedQuantity > 0)
			{
				await ReturnProductQuantityToInventory(flashSaleProduct);
			}

			// Xóa FlashSaleProduct
			await _flashSaleProductRepository.DeleteAsync(flashSaleProduct);
		}

		/// <summary>
		/// Cập nhật thông tin sản phẩm trong FlashSale
		/// Được sử dụng trong: FlashSales/_EditProductModal.cshtml - form chỉnh sửa sản phẩm
		/// Lưu ý: Nếu FlashSale đang diễn ra, chỉ cho phép cập nhật giá và giới hạn mua, không cho phép thay đổi số lượng
		/// </summary>
		/// <param name="flashSaleProductId">ID của FlashSaleProduct cần cập nhật</param>
		/// <param name="input">Thông tin mới: FlashSalePrice, FlashSaleQuantity, MaxQuantityPerUser</param>
		/// <returns>FlashSaleProduct đã cập nhật</returns>
		public async Task<FlashSaleProductDto> UpdateProduct(int flashSaleProductId, AddProductToFlashSaleDto input)
		{
			// Lấy FlashSaleProduct kèm theo thông tin FlashSale
			var flashSaleProduct = await _flashSaleProductRepository.GetAll()
				.Include(fsp => fsp.FlashSale)
				.FirstOrDefaultAsync(fsp => fsp.Id == flashSaleProductId);

			if (flashSaleProduct == null)
			{
				throw new UserFriendlyException("Không tìm thấy sản phẩm trong FlashSale");
			}

			// Nếu FlashSale đang diễn ra, chỉ cho phép cập nhật giá và giới hạn mua
			// Không cho phép thay đổi số lượng để tránh ảnh hưởng đến đơn hàng đang xử lý
			if (flashSaleProduct.FlashSale.Status == FlashSaleStatus.Ongoing)
			{
				flashSaleProduct.FlashSalePrice = input.FlashSalePrice;
				flashSaleProduct.MaxQuantityPerUser = input.MaxQuantityPerUser;
			}
			else
			{
				// Nếu FlashSale chưa bắt đầu hoặc đã kết thúc, cho phép cập nhật số lượng
				if (input.FlashSaleQuantity != flashSaleProduct.FlashSaleQuantity)
				{
					// Kiểm tra Inventory
					var inventory = await _inventoryRepository.FirstOrDefaultAsync(i => i.ProductId == flashSaleProduct.ProductId);
					if (inventory == null)
					{
						throw new UserFriendlyException("Không tìm thấy kho hàng cho sản phẩm này");
					}

					// Tính số lượng chênh lệch
					var quantityDiff = input.FlashSaleQuantity - flashSaleProduct.FlashSaleQuantity;
					var availableQuantity = inventory.Quantity - inventory.ReservedQuantity;

					// Nếu tăng số lượng, kiểm tra có đủ số lượng khả dụng không
					if (quantityDiff > availableQuantity)
					{
						throw new UserFriendlyException($"Số lượng khả dụng không đủ. Cần thêm: {quantityDiff}, hiện có: {availableQuantity}");
					}

					// Cập nhật ReservedQuantity trong Inventory
					// Nếu tăng số lượng: tăng ReservedQuantity
					// Nếu giảm số lượng: giảm ReservedQuantity (hoàn trả về kho)
					inventory.ReservedQuantity += quantityDiff;
					await _inventoryRepository.UpdateAsync(inventory);

					// Cập nhật số lượng FlashSale
					flashSaleProduct.FlashSaleQuantity = input.FlashSaleQuantity;
					flashSaleProduct.ReservedQuantity = input.FlashSaleQuantity;
				}

				// Cập nhật giá và giới hạn mua
				flashSaleProduct.FlashSalePrice = input.FlashSalePrice;
				flashSaleProduct.MaxQuantityPerUser = input.MaxQuantityPerUser;
			}

			// Lưu vào database
			await _flashSaleProductRepository.UpdateAsync(flashSaleProduct);
			await CurrentUnitOfWork.SaveChangesAsync();

			return await GetFlashSaleProductDto(flashSaleProduct.Id);
		}

		/// <summary>
		/// Hoàn trả số lượng còn lại của tất cả sản phẩm trong FlashSale về Inventory
		/// Được sử dụng khi FlashSale kết thúc hoặc bị hủy
		/// Tự động hoàn trả số lượng chưa bán về kho chính
		/// </summary>
		/// <param name="flashSaleId">ID của FlashSale cần hoàn trả số lượng</param>
		public async Task ReturnRemainingQuantityToInventory(int flashSaleId)
		{
			// Lấy FlashSale kèm theo danh sách sản phẩm
			var flashSale = await _flashSaleRepository.GetAll()
				.Include(fs => fs.FlashSaleProducts)
				.FirstOrDefaultAsync(fs => fs.Id == flashSaleId);

			if (flashSale == null)
			{
				throw new UserFriendlyException("Không tìm thấy chương trình FlashSale");
			}

			// Hoàn trả số lượng cho từng sản phẩm
			// Chỉ hoàn trả những sản phẩm chưa được hoàn trả trước đó
			foreach (var product in flashSale.FlashSaleProducts)
			{
				if (!product.IsReturnedToInventory)
				{
					await ReturnProductQuantityToInventory(product);
					product.IsReturnedToInventory = true; // Đánh dấu đã hoàn trả
					await _flashSaleProductRepository.UpdateAsync(product);
				}
			}

			await CurrentUnitOfWork.SaveChangesAsync();
		}

		/// <summary>
		/// Lấy thông tin FlashSaleProduct theo ID
		/// Được sử dụng trong: FlashSales/_EditProductModal.js - load dữ liệu để chỉnh sửa
		/// </summary>
		/// <param name="flashSaleProductId">ID của FlashSaleProduct cần lấy</param>
		/// <returns>Thông tin FlashSaleProduct</returns>
		public async Task<FlashSaleProductDto> GetFlashSaleProductById(int flashSaleProductId)
		{
			return await GetFlashSaleProductDto(flashSaleProductId);
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Hoàn trả số lượng sản phẩm về Inventory
		/// Method private - chỉ được gọi nội bộ trong class
		/// Tính số lượng còn lại (chưa bán) và giảm ReservedQuantity trong Inventory
		/// </summary>
		/// <param name="flashSaleProduct">FlashSaleProduct cần hoàn trả số lượng</param>
		private async Task ReturnProductQuantityToInventory(FlashSaleProduct flashSaleProduct)
		{
			// Lấy Inventory của sản phẩm
			var inventory = await _inventoryRepository.FirstOrDefaultAsync(i => i.ProductId == flashSaleProduct.ProductId);
			if (inventory != null)
			{
				// Tính số lượng còn lại = Tổng số lượng FlashSale - Số lượng đã bán
				var remainingQuantity = flashSaleProduct.FlashSaleQuantity - flashSaleProduct.SoldQuantity;
				
				// Nếu còn số lượng chưa bán, hoàn trả về Inventory
				if (remainingQuantity > 0)
				{
					// Giảm ReservedQuantity (không được âm)
					inventory.ReservedQuantity = Math.Max(0, inventory.ReservedQuantity - remainingQuantity);
					await _inventoryRepository.UpdateAsync(inventory);
				}
			}
		}

		/// <summary>
		/// Lấy thông tin FlashSaleProduct và map sang DTO
		/// Method private - chỉ được gọi nội bộ trong class
		/// </summary>
		/// <param name="flashSaleProductId">ID của FlashSaleProduct</param>
		/// <returns>FlashSaleProductDto với đầy đủ thông tin</returns>
		private async Task<FlashSaleProductDto> GetFlashSaleProductDto(int flashSaleProductId)
		{
			// Lấy FlashSaleProduct kèm theo thông tin Product
			var flashSaleProduct = await _flashSaleProductRepository.GetAll()
				.Include(fsp => fsp.Product)
				.FirstOrDefaultAsync(fsp => fsp.Id == flashSaleProductId);

			if (flashSaleProduct == null)
			{
				throw new UserFriendlyException("Không tìm thấy sản phẩm FlashSale");
			}

			// Map entity sang DTO
			return new FlashSaleProductDto
			{
				Id = flashSaleProduct.Id,
				FlashSaleId = flashSaleProduct.FlashSaleId,
				ProductId = flashSaleProduct.ProductId,
				ProductName = flashSaleProduct.Product.Name,
				ProductImage = flashSaleProduct.Product.Image,
				OriginalPrice = flashSaleProduct.Product.Price,
				FlashSalePrice = flashSaleProduct.FlashSalePrice,
				FlashSaleQuantity = flashSaleProduct.FlashSaleQuantity,
				SoldQuantity = flashSaleProduct.SoldQuantity,
				RemainingQuantity = flashSaleProduct.RemainingQuantity,
				MaxQuantityPerUser = flashSaleProduct.MaxQuantityPerUser,
				ReservedQuantity = flashSaleProduct.ReservedQuantity,
				IsReturnedToInventory = flashSaleProduct.IsReturnedToInventory
			};
		}

		/// <summary>
		/// Chuyển đổi FlashSaleStatus enum sang text tiếng Việt
		/// Method static - không cần instance, có thể gọi trực tiếp
		/// Được sử dụng trong: GetAll, GetById để hiển thị trạng thái trên UI
		/// </summary>
		/// <param name="status">FlashSaleStatus enum</param>
		/// <returns>Text mô tả trạng thái bằng tiếng Việt</returns>
		private static string GetStatusText(FlashSaleStatus status)
		{
			switch (status)
			{
				case FlashSaleStatus.NotStarted:
					return "Chưa bắt đầu";
				case FlashSaleStatus.Ongoing:
					return "Đang diễn ra";
				case FlashSaleStatus.Ended:
					return "Đã kết thúc";
				case FlashSaleStatus.Cancelled:
					return "Đã hủy";
				default:
					return "Không xác định";
			}
		}

		#endregion

		#region Frontend Methods

		/// <summary>
		/// Lấy danh sách FlashSale đang active và không bị ẩn (cho Frontend)
		/// Chỉ lấy các FlashSale: IsActive = true, IsHidden = false, có sản phẩm
		/// </summary>
		/// <returns>Danh sách FlashSale active</returns>
		public async Task<List<FlashSaleDto>> GetActiveFlashSales()
		{
			var now = DateTime.Now;

			// Lấy các FlashSale active, không bị ẩn, có sản phẩm và chưa kết thúc
			var flashSales = await _flashSaleRepository.GetAll()
				.Include(fs => fs.FlashSaleProducts)
				.ThenInclude(fsp => fsp.Product)
				.Where(fs => fs.IsActive && !fs.IsHidden && fs.FlashSaleProducts.Any())
				.Where(fs => fs.EndTime > now) // Chưa kết thúc
				.OrderByDescending(fs => fs.StartTime)
				.ToListAsync();

			// Map sang DTO và tính Status dựa trên thời gian
			var result = flashSales.Select(fs =>
			{
				var status = fs.CalculatedStatus;
				return new FlashSaleDto
				{
					Id = fs.Id,
					Name = fs.Name,
					Description = fs.Description,
					StartTime = fs.StartTime,
					EndTime = fs.EndTime,
					Status = status,
					StatusText = GetStatusText(status),
					IsActive = fs.IsActive,
					IsHidden = fs.IsHidden,
					CreationTime = fs.CreationTime,
					LastModificationTime = fs.LastModificationTime,
					TotalProducts = fs.FlashSaleProducts.Count,
					TotalSold = fs.FlashSaleProducts.Sum(p => p.SoldQuantity),
					Products = fs.FlashSaleProducts.Select(fsp => new FlashSaleProductDto
					{
						Id = fsp.Id,
						FlashSaleId = fsp.FlashSaleId,
						ProductId = fsp.ProductId,
						ProductName = fsp.Product.Name,
						ProductImage = fsp.Product.Image,
						OriginalPrice = fsp.Product.Price,
						FlashSalePrice = fsp.FlashSalePrice,
						FlashSaleQuantity = fsp.FlashSaleQuantity,
						SoldQuantity = fsp.SoldQuantity,
						RemainingQuantity = fsp.RemainingQuantity,
						MaxQuantityPerUser = fsp.MaxQuantityPerUser,
						ReservedQuantity = fsp.ReservedQuantity,
						IsReturnedToInventory = fsp.IsReturnedToInventory
					}).ToList()
				};
			}).ToList();

			return result;
		}

		/// <summary>
		/// Lấy danh sách FlashSale đang diễn ra (Status = Ongoing) (cho Frontend)
		/// </summary>
		/// <returns>Danh sách FlashSale đang diễn ra</returns>
		public async Task<List<FlashSaleDto>> GetOngoingFlashSales()
		{
			var now = DateTime.Now;

			// Lấy các FlashSale đang diễn ra (trong khoảng thời gian StartTime và EndTime)
			var flashSales = await _flashSaleRepository.GetAll()
				.Include(fs => fs.FlashSaleProducts)
				.ThenInclude(fsp => fsp.Product)
				.Where(fs => fs.IsActive && !fs.IsHidden && fs.FlashSaleProducts.Any())
				.Where(fs => fs.StartTime <= now && fs.EndTime >= now) // Đang diễn ra
				.OrderByDescending(fs => fs.StartTime)
				.ToListAsync();

			// Map sang DTO
			var result = flashSales.Select(fs => new FlashSaleDto
			{
				Id = fs.Id,
				Name = fs.Name,
				Description = fs.Description,
				StartTime = fs.StartTime,
				EndTime = fs.EndTime,
				Status = FlashSaleStatus.Ongoing,
				StatusText = GetStatusText(FlashSaleStatus.Ongoing),
				IsActive = fs.IsActive,
				IsHidden = fs.IsHidden,
				CreationTime = fs.CreationTime,
				LastModificationTime = fs.LastModificationTime,
				TotalProducts = fs.FlashSaleProducts.Count,
				TotalSold = fs.FlashSaleProducts.Sum(p => p.SoldQuantity),
				Products = fs.FlashSaleProducts.Select(fsp => new FlashSaleProductDto
				{
					Id = fsp.Id,
					FlashSaleId = fsp.FlashSaleId,
					ProductId = fsp.ProductId,
					ProductName = fsp.Product.Name,
					ProductImage = fsp.Product.Image,
					OriginalPrice = fsp.Product.Price,
					FlashSalePrice = fsp.FlashSalePrice,
					FlashSaleQuantity = fsp.FlashSaleQuantity,
					SoldQuantity = fsp.SoldQuantity,
					RemainingQuantity = fsp.RemainingQuantity,
					MaxQuantityPerUser = fsp.MaxQuantityPerUser,
					ReservedQuantity = fsp.ReservedQuantity,
					IsReturnedToInventory = fsp.IsReturnedToInventory
				}).ToList()
			}).ToList();

			return result;
		}

		/// <summary>
		/// Lấy danh sách sản phẩm trong FlashSale theo FlashSaleId (cho Frontend)
		/// </summary>
		/// <param name="flashSaleId">ID của FlashSale</param>
		/// <returns>Danh sách sản phẩm trong FlashSale</returns>
		public async Task<List<FlashSaleProductDto>> GetFlashSaleProductsByFlashSaleId(int flashSaleId)
		{
			// Lấy FlashSale và các sản phẩm
			var flashSale = await _flashSaleRepository.GetAll()
				.Include(fs => fs.FlashSaleProducts)
				.ThenInclude(fsp => fsp.Product)
				.FirstOrDefaultAsync(fs => fs.Id == flashSaleId);

			if (flashSale == null)
			{
				throw new UserFriendlyException("Không tìm thấy chương trình FlashSale");
			}

			// Map sang DTO
			var result = flashSale.FlashSaleProducts.Select(fsp => new FlashSaleProductDto
			{
				Id = fsp.Id,
				FlashSaleId = fsp.FlashSaleId,
				ProductId = fsp.ProductId,
				ProductName = fsp.Product.Name,
				ProductImage = fsp.Product.Image,
				OriginalPrice = fsp.Product.Price,
				FlashSalePrice = fsp.FlashSalePrice,
				FlashSaleQuantity = fsp.FlashSaleQuantity,
				SoldQuantity = fsp.SoldQuantity,
				RemainingQuantity = fsp.RemainingQuantity,
				MaxQuantityPerUser = fsp.MaxQuantityPerUser,
				ReservedQuantity = fsp.ReservedQuantity,
				IsReturnedToInventory = fsp.IsReturnedToInventory
			}).ToList();

			return result;
		}

		/// <summary>
		/// Kiểm tra sản phẩm có trong FlashSale đang diễn ra không (cho Frontend)
		/// </summary>
		/// <param name="productId">ID của sản phẩm</param>
		/// <returns>Thông tin FlashSaleProduct nếu có, null nếu không có</returns>
		public async Task<FlashSaleProductDto> GetFlashSaleProductByProductId(int productId)
		{
			var now = DateTime.Now;

			// Tìm FlashSaleProduct của sản phẩm trong FlashSale đang diễn ra
			var flashSaleProduct = await _flashSaleProductRepository.GetAll()
				.Include(fsp => fsp.FlashSale)
				.Include(fsp => fsp.Product)
				.Where(fsp => fsp.ProductId == productId)
				.Where(fsp => fsp.FlashSale.IsActive && !fsp.FlashSale.IsHidden)
				.Where(fsp => fsp.FlashSale.StartTime <= now && fsp.FlashSale.EndTime >= now) // Đang diễn ra
				.FirstOrDefaultAsync();

			if (flashSaleProduct == null)
			{
				return null; // Sản phẩm không có trong FlashSale đang diễn ra
			}

			// Map sang DTO
			return new FlashSaleProductDto
			{
				Id = flashSaleProduct.Id,
				FlashSaleId = flashSaleProduct.FlashSaleId,
				ProductId = flashSaleProduct.ProductId,
				ProductName = flashSaleProduct.Product.Name,
				ProductImage = flashSaleProduct.Product.Image,
				OriginalPrice = flashSaleProduct.Product.Price,
				FlashSalePrice = flashSaleProduct.FlashSalePrice,
				FlashSaleQuantity = flashSaleProduct.FlashSaleQuantity,
				SoldQuantity = flashSaleProduct.SoldQuantity,
				RemainingQuantity = flashSaleProduct.RemainingQuantity,
				MaxQuantityPerUser = flashSaleProduct.MaxQuantityPerUser,
				ReservedQuantity = flashSaleProduct.ReservedQuantity,
				IsReturnedToInventory = flashSaleProduct.IsReturnedToInventory
			};
		}

		/// <summary>
		/// Mua sản phẩm FlashSale - Cập nhật SoldQuantity (cho Frontend)
		/// Method này được gọi khi user đặt hàng sản phẩm FlashSale
		/// </summary>
		/// <param name="flashSaleProductId">ID của FlashSaleProduct</param>
		/// <param name="quantity">Số lượng mua</param>
		/// <param name="userId">ID của user mua</param>
		public async Task PurchaseFlashSaleProduct(int flashSaleProductId, int quantity, long userId)
		{
			// Lấy FlashSaleProduct
			var flashSaleProduct = await _flashSaleProductRepository.GetAll()
				.Include(fsp => fsp.FlashSale)
				.FirstOrDefaultAsync(fsp => fsp.Id == flashSaleProductId);

			if (flashSaleProduct == null)
			{
				throw new UserFriendlyException("Không tìm thấy sản phẩm FlashSale");
			}

			// Kiểm tra FlashSale có đang diễn ra không
			var now = DateTime.Now;
			if (flashSaleProduct.FlashSale.StartTime > now || flashSaleProduct.FlashSale.EndTime < now)
			{
				throw new UserFriendlyException("FlashSale không đang diễn ra");
			}

			// Kiểm tra số lượng còn lại
			if (flashSaleProduct.RemainingQuantity < quantity)
			{
				throw new UserFriendlyException($"Số lượng còn lại không đủ. Còn lại: {flashSaleProduct.RemainingQuantity}");
			}

			// Kiểm tra giới hạn số lượng mua per user (nếu có)
			if (flashSaleProduct.MaxQuantityPerUser.HasValue && quantity > flashSaleProduct.MaxQuantityPerUser.Value)
			{
				throw new UserFriendlyException($"Số lượng mua tối đa là {flashSaleProduct.MaxQuantityPerUser.Value}");
			}

			// TODO: Kiểm tra user đã mua bao nhiêu sản phẩm này (cần tích hợp với Order system)
			// Hiện tại chưa có bảng Order, nên tạm thời bỏ qua check này

			// Cập nhật SoldQuantity
			flashSaleProduct.SoldQuantity += quantity;
			// RemainingQuantity là property chỉ đọc, ta không set trực tiếp được
			// Nếu cần update, hãy chắc chắn logic tính toán ở nơi dùng

			// Lưu vào database
			await _flashSaleProductRepository.UpdateAsync(flashSaleProduct);
			await CurrentUnitOfWork.SaveChangesAsync();
		}

		#endregion
	}
}

