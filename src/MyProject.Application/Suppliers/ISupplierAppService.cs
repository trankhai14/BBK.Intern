using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using MyProject.Suppliers.Dto;

namespace MyProject.Suppliers
{
	public interface ISupplierAppService : IApplicationService
	{
		Task<PagedResultDto<SupplierDto>> GetAll(GetAllSuppliersInput input);
		Task<List<SupplierDto>> GetAllList();
		Task<SupplierDto> GetById(int id);
		Task<SupplierDto> Create(CreateSupplierDto input);
		Task<SupplierDto> Update(UpdateSupplierDto input);
		Task Delete(int id);
	}
}

