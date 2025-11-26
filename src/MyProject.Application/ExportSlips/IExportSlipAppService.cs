using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using MyProject.ExportSlips.Dto;

namespace MyProject.ExportSlips
{
	/// <summary>
	/// Interface cho service quản lý phiếu xuất kho
	/// </summary>
	public interface IExportSlipAppService : IApplicationService
	{
		/// <summary>
		/// Tạo phiếu xuất kho mới (trạng thái Draft)
		/// </summary>
		Task<ExportSlipDto> CreateExportSlip(CreateExportSlipDto input);

		/// <summary>
		/// Cập nhật phiếu xuất kho (chỉ khi Status = Draft)
		/// </summary>
		Task<ExportSlipDto> UpdateExportSlip(UpdateExportSlipDto input);

		/// <summary>
		/// Xác nhận và hoàn thành phiếu xuất kho
		/// - Giảm Inventory.Quantity cho từng sản phẩm
		/// - Tạo InventoryTransaction cho từng sản phẩm
		/// - Chuyển Status = Completed
		/// </summary>
		Task CompleteExportSlip(int exportSlipId);

		/// <summary>
		/// Hủy phiếu xuất kho (chỉ khi Status = Draft)
		/// </summary>
		Task CancelExportSlip(int exportSlipId);

		/// <summary>
		/// Lấy danh sách phiếu xuất kho có phân trang và lọc
		/// </summary>
		Task<PagedResultDto<ExportSlipDto>> GetAllExportSlips(GetAllExportSlipsInput input);

		/// <summary>
		/// Lấy chi tiết phiếu xuất kho theo ID
		/// </summary>
		Task<ExportSlipDto> GetExportSlipById(int id);

		/// <summary>
		/// Xóa phiếu xuất kho (chỉ khi Status = Draft)
		/// </summary>
		Task DeleteExportSlip(int id);
	}
}

