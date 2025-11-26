using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using MyProject.ImportSlips.Dto;

namespace MyProject.ImportSlips
{
	/// <summary>
	/// Interface cho service quản lý phiếu nhập kho
	/// </summary>
	public interface IImportSlipAppService : IApplicationService
	{
		/// <summary>
		/// Tạo phiếu nhập kho mới (trạng thái Draft)
		/// </summary>
		Task<ImportSlipDto> CreateImportSlip(CreateImportSlipDto input);

		/// <summary>
		/// Cập nhật phiếu nhập kho (chỉ khi Status = Draft)
		/// </summary>
		Task<ImportSlipDto> UpdateImportSlip(UpdateImportSlipDto input);

		/// <summary>
		/// Xác nhận và hoàn thành phiếu nhập kho
		/// - Cập nhật Inventory.Quantity cho từng sản phẩm
		/// - Tạo InventoryTransaction cho từng sản phẩm
		/// - Chuyển Status = Completed
		/// </summary>
		Task CompleteImportSlip(int importSlipId);

		/// <summary>
		/// Hủy phiếu nhập kho (chỉ khi Status = Draft)
		/// </summary>
		Task CancelImportSlip(int importSlipId);

		/// <summary>
		/// Lấy danh sách phiếu nhập kho có phân trang và lọc
		/// </summary>
		Task<PagedResultDto<ImportSlipDto>> GetAllImportSlips(GetAllImportSlipsInput input);

		/// <summary>
		/// Lấy chi tiết phiếu nhập kho theo ID
		/// </summary>
		Task<ImportSlipDto> GetImportSlipById(int id);

		/// <summary>
		/// Xóa phiếu nhập kho (chỉ khi Status = Draft)
		/// </summary>
		Task DeleteImportSlip(int id);
	}
}


