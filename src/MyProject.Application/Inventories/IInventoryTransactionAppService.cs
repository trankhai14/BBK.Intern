using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using MyProject.Inventories.Dto;

namespace MyProject.Inventories
{
	/// <summary>
	/// Interface cho service quản lý nhập xuất kho
	/// </summary>
	public interface IInventoryTransactionAppService : IApplicationService
	{
		/// <summary>
		/// Nhập kho
		/// </summary>
		Task<InventoryTransactionDto> ImportInventory(ImportInventoryDto input);

		/// <summary>
		/// Xuất kho
		/// </summary>
		Task<InventoryTransactionDto> ExportInventory(ExportInventoryDto input);

		/// <summary>
		/// Lấy danh sách lịch sử giao dịch kho
		/// </summary>
		Task<PagedResultDto<InventoryTransactionDto>> GetAllTransactions(GetAllInventoryTransactionsDto input);

		/// <summary>
		/// Lấy chi tiết một giao dịch
		/// </summary>
		Task<InventoryTransactionDto> GetTransactionById(int id);
	}
}
