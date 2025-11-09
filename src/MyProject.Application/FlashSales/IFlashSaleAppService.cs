using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using MyProject.FlashSales.Dto;

namespace MyProject.FlashSales
{
	public interface IFlashSaleAppService : IApplicationService
	{
		/// <summary>
		/// Lấy danh sách FlashSale có phân trang
		/// </summary>
		Task<PagedResultDto<FlashSaleDto>> GetAll(GetAllFlashSalesInput input);

		/// <summary>
		/// Lấy thông tin chi tiết FlashSale theo ID
		/// </summary>
		Task<FlashSaleDto> GetById(int id);

		/// <summary>
		/// Tạo mới FlashSale
		/// </summary>
		Task<FlashSaleDto> Create(CreateFlashSaleDto input);

		/// <summary>
		/// Cập nhật FlashSale
		/// </summary>
		Task<FlashSaleDto> Update(UpdateFlashSaleDto input);

		/// <summary>
		/// Xóa FlashSale
		/// </summary>
		Task Delete(int id);

		/// <summary>
		/// Ẩn/Hiện FlashSale
		/// </summary>
		Task ToggleHide(int id);

		/// <summary>
		/// Thêm sản phẩm vào FlashSale (khóa số lượng trong Inventory)
		/// </summary>
		Task<FlashSaleProductDto> AddProduct(AddProductToFlashSaleDto input);

		/// <summary>
		/// Xóa sản phẩm khỏi FlashSale (hoàn trả số lượng về Inventory)
		/// </summary>
		Task RemoveProduct(int flashSaleProductId);

		/// <summary>
		/// Cập nhật thông tin sản phẩm trong FlashSale
		/// </summary>
		Task<FlashSaleProductDto> UpdateProduct(int flashSaleProductId, AddProductToFlashSaleDto input);

		/// <summary>
		/// Hoàn trả số lượng chưa bán về Inventory khi FlashSale kết thúc
		/// </summary>
		Task ReturnRemainingQuantityToInventory(int flashSaleId);

		/// <summary>
		/// Lấy thông tin FlashSaleProduct theo ID
		/// </summary>
		Task<FlashSaleProductDto> GetFlashSaleProductById(int flashSaleProductId);

		/// <summary>
		/// Lấy danh sách FlashSale đang active và không bị ẩn (cho Frontend)
		/// </summary>
		Task<List<FlashSaleDto>> GetActiveFlashSales();

		/// <summary>
		/// Lấy danh sách FlashSale đang diễn ra (Status = Ongoing) (cho Frontend)
		/// </summary>
		Task<List<FlashSaleDto>> GetOngoingFlashSales();

		/// <summary>
		/// Lấy danh sách sản phẩm trong FlashSale theo FlashSaleId (cho Frontend)
		/// </summary>
		Task<List<FlashSaleProductDto>> GetFlashSaleProductsByFlashSaleId(int flashSaleId);

		/// <summary>
		/// Kiểm tra sản phẩm có trong FlashSale đang diễn ra không (cho Frontend)
		/// </summary>
		Task<FlashSaleProductDto> GetFlashSaleProductByProductId(int productId);

		/// <summary>
		/// Mua sản phẩm FlashSale - Cập nhật SoldQuantity (cho Frontend)
		/// </summary>
		Task PurchaseFlashSaleProduct(int flashSaleProductId, int quantity, long userId);
	}
}

