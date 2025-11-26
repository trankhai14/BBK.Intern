using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using MyProject.Authorization;
using MyProject.Products;
using MyProject.Suppliers.Dto;

namespace MyProject.Suppliers
{
	/// <summary>
	/// Service xử lý các thao tác CRUD cho Nhà cung cấp
	/// Yêu cầu quyền Pages_Suppliers để truy cập
	/// </summary>
	[AbpAuthorize(PermissionNames.Pages_Suppliers)]
	public class SupplierAppService : MyProjectAppServiceBase, ISupplierAppService
	{
		private readonly IRepository<Supplier> _supplierRepository;
		private readonly IRepository<MyProject.Products.Product> _productRepository;

		/// <summary>
		/// Constructor - Inject các dependencies cần thiết
		/// </summary>
		/// <param name="supplierRepository">Repository để thao tác với bảng Suppliers</param>
		/// <param name="productRepository">Repository để kiểm tra sản phẩm khi xóa nhà cung cấp</param>
		public SupplierAppService(
			IRepository<Supplier> supplierRepository,
			IRepository<MyProject.Products.Product> productRepository)
		{
			_supplierRepository = supplierRepository;
			_productRepository = productRepository;
		}

		/// <summary>
		/// Lấy danh sách nhà cung cấp có phân trang và tìm kiếm
		/// </summary>
		/// <param name="input">Thông tin phân trang và điều kiện lọc (tên, mã, điện thoại, email, trạng thái)</param>
		/// <returns>Danh sách nhà cung cấp kèm tổng số bản ghi</returns>
		public async Task<PagedResultDto<SupplierDto>> GetAll(GetAllSuppliersInput input)
		{
			// Lấy tất cả nhà cung cấp từ database
			var query = _supplierRepository.GetAll();

			// Áp dụng các điều kiện lọc nếu có
			// Lọc theo tên nhà cung cấp (tìm kiếm chuỗi con)
			if (!string.IsNullOrWhiteSpace(input.Name))
			{
				query = query.Where(x => x.Name.Contains(input.Name));
			}

			// Lọc theo mã nhà cung cấp
			if (!string.IsNullOrWhiteSpace(input.Code))
			{
				query = query.Where(x => x.Code != null && x.Code.Contains(input.Code));
			}

			// Lọc theo số điện thoại
			if (!string.IsNullOrWhiteSpace(input.Phone))
			{
				query = query.Where(x => x.Phone != null && x.Phone.Contains(input.Phone));
			}

			// Lọc theo email
			if (!string.IsNullOrWhiteSpace(input.Email))
			{
				query = query.Where(x => x.Email != null && x.Email.Contains(input.Email));
			}

			// Lọc theo trạng thái hoạt động (true/false)
			if (input.IsActive.HasValue)
			{
				query = query.Where(x => x.IsActive == input.IsActive.Value);
			}

			// Đếm tổng số bản ghi sau khi lọc
			var totalCount = await query.CountAsync();

			// Áp dụng sắp xếp và phân trang
			var suppliers = await query
				//.OrderByIf(!string.IsNullOrWhiteSpace(input.Sorting), input.Sorting) // Sắp xếp theo yêu cầu
				.OrderBy(x => x.Name) // Mặc định sắp xếp theo tên
				.PageBy(input) // Phân trang
				.ToListAsync();

			// Map thủ công từng entity sang DTO (không dùng ObjectMapper)
			var supplierDtos = suppliers.Select(s => new SupplierDto
			{
				Id = s.Id,
				Name = s.Name,
				Code = s.Code,
				Phone = s.Phone,
				Email = s.Email,
				Address = s.Address,
				ContactPerson = s.ContactPerson,
				Notes = s.Notes,
				IsActive = s.IsActive
			}).ToList();

			return new PagedResultDto<SupplierDto>(totalCount, supplierDtos);
		}

		/// <summary>
		/// Lấy danh sách tất cả nhà cung cấp đang hoạt động (không phân trang)
		/// Dùng cho dropdown list khi tạo/sửa sản phẩm
		/// Map thủ công từ Entity sang DTO (không dùng ObjectMapper)
		/// </summary>
		/// <returns>Danh sách nhà cung cấp đang hoạt động</returns>
		public async Task<List<SupplierDto>> GetAllList()
		{
			var suppliers = await _supplierRepository.GetAllListAsync(x => x.IsActive);
			// Map thủ công từng entity sang DTO
			return suppliers.Select(s => new SupplierDto
			{
				Id = s.Id,
				Name = s.Name,
				Code = s.Code,
				Phone = s.Phone,
				Email = s.Email,
				Address = s.Address,
				ContactPerson = s.ContactPerson,
				Notes = s.Notes,
				IsActive = s.IsActive
			}).ToList();
		}

		/// <summary>
		/// Lấy thông tin chi tiết một nhà cung cấp theo ID
		/// Map thủ công từ Entity sang DTO (không dùng ObjectMapper)
		/// </summary>
		/// <param name="id">ID của nhà cung cấp cần lấy</param>
		/// <returns>Thông tin chi tiết nhà cung cấp</returns>
		public async Task<SupplierDto> GetById(int id)
		{
			var supplier = await _supplierRepository.GetAsync(id);
			// Map thủ công từ Entity sang DTO
			return new SupplierDto
			{
				Id = supplier.Id,
				Name = supplier.Name,
				Code = supplier.Code,
				Phone = supplier.Phone,
				Email = supplier.Email,
				Address = supplier.Address,
				ContactPerson = supplier.ContactPerson,
				Notes = supplier.Notes,
				IsActive = supplier.IsActive
			};
		}

		/// <summary>
		/// Tạo mới nhà cung cấp
		/// Map thủ công từ DTO sang Entity (không dùng ObjectMapper tự động)
		/// </summary>
		/// <param name="input">Thông tin nhà cung cấp mới</param>
		/// <returns>Thông tin nhà cung cấp vừa tạo</returns>
		[AbpAuthorize(PermissionNames.Pages_Suppliers_Create)]
		public async Task<SupplierDto> Create(CreateSupplierDto input)
		{
			// Tạo entity mới và map thủ công từng trường
			var supplier = new Supplier
			{
				Name = input.Name,
				Code = input.Code,
				Phone = input.Phone,
				Email = input.Email,
				Address = input.Address,
				ContactPerson = input.ContactPerson,
				Notes = input.Notes,
				IsActive = input.IsActive // Map boolean trực tiếp
			};

			await _supplierRepository.InsertAsync(supplier);
			await CurrentUnitOfWork.SaveChangesAsync();

			// Map thủ công từ Entity sang DTO để trả về
			return new SupplierDto
			{
				Id = supplier.Id,
				Name = supplier.Name,
				Code = supplier.Code,
				Phone = supplier.Phone,
				Email = supplier.Email,
				Address = supplier.Address,
				ContactPerson = supplier.ContactPerson,
				Notes = supplier.Notes,
				IsActive = supplier.IsActive,
				CreationTime = supplier.CreationTime,
				CreatorUserId = supplier.CreatorUserId
			};
		}

		/// <summary>
		/// Cập nhật thông tin nhà cung cấp
		/// Map thủ công từng trường từ DTO sang Entity (không dùng ObjectMapper tự động)
		/// </summary>
		/// <param name="input">Thông tin nhà cung cấp cần cập nhật (phải có Id)</param>
		/// <returns>Thông tin nhà cung cấp sau khi cập nhật</returns>
		[AbpAuthorize(PermissionNames.Pages_Suppliers_Edit)]
		public async Task<SupplierDto> Update(UpdateSupplierDto input)
		{
			var supplier = await _supplierRepository.GetAsync(input.Id);

			// Cập nhật từng trường thông tin (map thủ công)
			supplier.Name = input.Name;
			supplier.Code = input.Code;
			supplier.Phone = input.Phone;
			supplier.Email = input.Email;
			supplier.Address = input.Address;
			supplier.ContactPerson = input.ContactPerson;
			supplier.Notes = input.Notes;
			supplier.IsActive = input.IsActive; // Map boolean trực tiếp

			await _supplierRepository.UpdateAsync(supplier);
			await CurrentUnitOfWork.SaveChangesAsync();

			// Map thủ công từ Entity sang DTO để trả về
			return new SupplierDto
			{
				Id = supplier.Id,
				Name = supplier.Name,
				Code = supplier.Code,
				Phone = supplier.Phone,
				Email = supplier.Email,
				Address = supplier.Address,
				ContactPerson = supplier.ContactPerson,
				Notes = supplier.Notes,
				IsActive = supplier.IsActive
			};
		}

		/// <summary>
		/// Xóa nhà cung cấp
		/// Kiểm tra xem còn sản phẩm nào thuộc nhà cung cấp này không
		/// Nếu còn sản phẩm thì không cho xóa
		/// </summary>
		/// <param name="id">ID của nhà cung cấp cần xóa</param>
		/// <exception cref="UserFriendlyException">Nếu nhà cung cấp không tồn tại hoặc vẫn còn sản phẩm</exception>
		[AbpAuthorize(PermissionNames.Pages_Suppliers_Delete)]
		public async Task Delete(int id)
		{
			var supplier = await _supplierRepository.FirstOrDefaultAsync(x => x.Id == id);

			if (supplier == null)
			{
				throw new UserFriendlyException("Nhà cung cấp không tồn tại hoặc đã bị xóa.");
			}

			// Kiểm tra xem còn sản phẩm nào thuộc nhà cung cấp này không
			var hasProducts = await _productRepository.FirstOrDefaultAsync(x => x.SupplierId == id);

			if (hasProducts != null)
			{
				throw new UserFriendlyException("Không thể xóa nhà cung cấp vì vẫn còn sản phẩm thuộc nhà cung cấp này.");
			}

			await _supplierRepository.DeleteAsync(supplier);
		}
	}
}

