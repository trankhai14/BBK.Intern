using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using MyProject.CustomerProfiles.Dto;

namespace MyProject.CustomerProfiles
{
	public interface ICustomerProfileAppService : IApplicationService
	{
		/// <summary>
		/// Lấy danh sách thông tin khách hàng của user hiện tại
		/// </summary>
		Task<List<CustomerProfileDto>> GetAllByCurrentUser();

		/// <summary>
		/// Lấy thông tin khách hàng theo ID
		/// </summary>
		Task<CustomerProfileDto> GetById(int id);

		/// <summary>
		/// Lấy thông tin khách hàng mặc định của user hiện tại
		/// </summary>
		Task<CustomerProfileDto> GetDefaultProfile();

		/// <summary>
		/// Tạo mới thông tin khách hàng
		/// </summary>
		Task<CustomerProfileDto> Create(CreateCustomerProfileDto input);

		/// <summary>
		/// Cập nhật thông tin khách hàng
		/// </summary>
		Task<CustomerProfileDto> Update(UpdateCustomerProfileDto input);

		/// <summary>
		/// Xóa thông tin khách hàng
		/// </summary>
		Task Delete(int id);

		/// <summary>
		/// Đặt thông tin khách hàng làm mặc định
		/// </summary>
		Task SetAsDefault(int id);

		/// <summary>
		/// Lấy tất cả thông tin khách hàng (cho admin)
		/// </summary>
		Task<PagedResultDto<CustomerProfileDto>> GetAll(GetAllCustomerProfilesInput input);

		/// <summary>
		/// Lấy thông tin khách hàng theo ID (cho admin - không kiểm tra UserId)
		/// </summary>
		Task<CustomerProfileDto> GetByIdForAdmin(int id);

		/// <summary>
		/// Cập nhật thông tin khách hàng (cho admin - không kiểm tra UserId)
		/// </summary>
		Task<CustomerProfileDto> UpdateForAdmin(UpdateCustomerProfileDto input);

		/// <summary>
		/// Xóa thông tin khách hàng (cho admin - không kiểm tra UserId)
		/// </summary>
		Task DeleteForAdmin(int id);
	}
}

