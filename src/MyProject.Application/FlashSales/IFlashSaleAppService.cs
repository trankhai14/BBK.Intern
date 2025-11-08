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
	}
}

