using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using MyProject.Inventories.Dto;

namespace MyProject.Inventories
{
	/// <summary>
	/// Interface cho service quản lý kho hàng
	/// </summary>
	public interface IInventoryAppService : IApplicationService
	{
		// READ - Đọc dữ liệu
		/// <summary>
		/// Lấy danh sách kho hàng có phân trang và lọc
		/// </summary>
		Task<PagedResultDto<InventoryListDto>> GetAllInventories(GetAllInventoriesDto input);

		/// <summary>
		/// Lấy chi tiết kho hàng theo ID
		/// </summary>
		Task<InventoryDetailDto> GetInventoryById(int id);

		/// <summary>
		/// Lấy kho hàng theo ProductId
		/// </summary>
		Task<InventoryDetailDto> GetInventoryByProductId(int productId);

		// CREATE - Tạo mới
		/// <summary>
		/// Tạo mới kho hàng
		/// </summary>
		Task<InventoryDetailDto> CreateInventory(CreateInventoryDto input);

		// UPDATE - Cập nhật
		/// <summary>
		/// Cập nhật kho hàng
		/// </summary>
		Task<InventoryDetailDto> UpdateInventory(UpdateInventoryDto input);

		/// <summary>
		/// Cập nhật số lượng kho hàng theo ID (method cũ, giữ để tương thích)
		/// </summary>
		Task<Inventory> UpdateInventoryQuantity(int id, int quantity);

		/// <summary>
		/// Cập nhật số lượng kho hàng theo ProductId (method cũ, giữ để tương thích)
		/// </summary>
		Task<Inventory> UpdateInventoryByProductId(int productId, int quantity);

		// DELETE - Xóa
		/// <summary>
		/// Xóa kho hàng theo ID
		/// </summary>
		Task DeleteInventory(int id);

		/// <summary>
		/// Xóa kho hàng theo ProductId
		/// </summary>
		Task DeleteInventoryByProductId(int productId);

		// UTILITY - Tiện ích
		/// <summary>
		/// Tăng số lượng kho hàng
		/// </summary>
		Task<Inventory> IncreaseInventory(int productId, int quantity);

		/// <summary>
		/// Giảm số lượng kho hàng
		/// </summary>
		Task<Inventory> DecreaseInventory(int productId, int quantity);

		/// <summary>
		/// Kiểm tra kho hàng có đủ số lượng không
		/// </summary>
		Task<bool> CheckInventorySufficient(int productId, int requiredQuantity);
	}
}
