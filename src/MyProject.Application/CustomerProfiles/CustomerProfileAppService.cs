using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using MyProject.CustomerProfiles.Dto;

namespace MyProject.CustomerProfiles
{
	//[AbpAuthorize(PermissionNames.Pages_CustomerProfiles)]
	public class CustomerProfileAppService : MyProjectAppServiceBase, ICustomerProfileAppService
	{
		private readonly IRepository<CustomerProfile> _customerProfileRepository;

		public CustomerProfileAppService(IRepository<CustomerProfile> customerProfileRepository)
		{
			_customerProfileRepository = customerProfileRepository;
		}

		public async Task<List<CustomerProfileDto>> GetAllByCurrentUser()
		{
			if (AbpSession.UserId == null)
			{
				return new List<CustomerProfileDto>();
			}

			var profiles = await _customerProfileRepository.GetAllListAsync(p => p.UserId == AbpSession.UserId);

			return profiles.Select(p => new CustomerProfileDto
			{
				Id = p.Id,
				UserId = p.UserId,
				FullName = p.FullName,
				PhoneNumber = p.PhoneNumber,
				Address = p.Address,
				Ward = p.Ward,
				District = p.District,
				City = p.City,
				Avatar = p.Avatar,
				IsDefault = p.IsDefault,
				CreationTime = p.CreationTime,
				LastModificationTime = p.LastModificationTime
			}).OrderByDescending(p => p.IsDefault).ThenByDescending(p => p.CreationTime).ToList();
		}

		public async Task<CustomerProfileDto> GetById(int id)
		{
			if (AbpSession.UserId == null)
			{
				throw new UserFriendlyException("Bạn cần đăng nhập để xem thông tin này");
			}

			var profile = await _customerProfileRepository.FirstOrDefaultAsync(p => p.Id == id && p.UserId == AbpSession.UserId);

			if (profile == null)
			{
				throw new UserFriendlyException("Không tìm thấy thông tin khách hàng");
			}

			return new CustomerProfileDto
			{
				Id = profile.Id,
				UserId = profile.UserId,
				FullName = profile.FullName,
				PhoneNumber = profile.PhoneNumber,
				Address = profile.Address,
				Ward = profile.Ward,
				District = profile.District,
				City = profile.City,
				Avatar = profile.Avatar,
				IsDefault = profile.IsDefault,
				CreationTime = profile.CreationTime,
				LastModificationTime = profile.LastModificationTime
			};
		}

		public async Task<CustomerProfileDto> GetDefaultProfile()
		{
			if (AbpSession.UserId == null)
			{
				return null;
			}

			var profile = await _customerProfileRepository.FirstOrDefaultAsync(p => p.UserId == AbpSession.UserId && p.IsDefault);

			if (profile == null)
			{
				return null;
			}

			return new CustomerProfileDto
			{
				Id = profile.Id,
				UserId = profile.UserId,
				FullName = profile.FullName,
				PhoneNumber = profile.PhoneNumber,
				Address = profile.Address,
				Ward = profile.Ward,
				District = profile.District,
				City = profile.City,
				Avatar = profile.Avatar,
				IsDefault = profile.IsDefault,
				CreationTime = profile.CreationTime,
				LastModificationTime = profile.LastModificationTime
			};
		}

		public async Task<CustomerProfileDto> Create(CreateCustomerProfileDto input)
		{
			if (AbpSession.UserId == null)
			{
				throw new UserFriendlyException("Bạn cần đăng nhập để tạo thông tin khách hàng");
			}

			// Nếu đặt làm mặc định, bỏ mặc định của các profile khác
			if (input.IsDefault)
			{
				var existingDefault = await _customerProfileRepository.FirstOrDefaultAsync(p => p.UserId == AbpSession.UserId && p.IsDefault);
				if (existingDefault != null)
				{
					existingDefault.IsDefault = false;
					await _customerProfileRepository.UpdateAsync(existingDefault);
				}
			}

			var profile = new CustomerProfile
			{
				UserId = AbpSession.UserId.Value,
				FullName = input.FullName,
				PhoneNumber = input.PhoneNumber,
				Address = input.Address,
				Ward = input.Ward,
				District = input.District,
				City = input.City,
				Avatar = input.Avatar,
				IsDefault = input.IsDefault
			};

			await _customerProfileRepository.InsertAsync(profile);
			await CurrentUnitOfWork.SaveChangesAsync();

			return new CustomerProfileDto
			{
				Id = profile.Id,
				UserId = profile.UserId,
				FullName = profile.FullName,
				PhoneNumber = profile.PhoneNumber,
				Address = profile.Address,
				Ward = profile.Ward,
				District = profile.District,
				City = profile.City,
				Avatar = profile.Avatar,
				IsDefault = profile.IsDefault,
				CreationTime = profile.CreationTime,
				LastModificationTime = profile.LastModificationTime
			};
		}

		public async Task<CustomerProfileDto> Update(UpdateCustomerProfileDto input)
		{
			if (AbpSession.UserId == null)
			{
				throw new UserFriendlyException("Bạn cần đăng nhập để cập nhật thông tin khách hàng");
			}

			var profile = await _customerProfileRepository.FirstOrDefaultAsync(p => p.Id == input.Id && p.UserId == AbpSession.UserId);

			if (profile == null)
			{
				throw new UserFriendlyException("Không tìm thấy thông tin khách hàng");
			}

			// Nếu đặt làm mặc định, bỏ mặc định của các profile khác
			if (input.IsDefault && !profile.IsDefault)
			{
				var existingDefault = await _customerProfileRepository.FirstOrDefaultAsync(p => p.UserId == AbpSession.UserId && p.IsDefault && p.Id != input.Id);
				if (existingDefault != null)
				{
					existingDefault.IsDefault = false;
					await _customerProfileRepository.UpdateAsync(existingDefault);
				}
			}

			profile.FullName = input.FullName;
			profile.PhoneNumber = input.PhoneNumber;
			profile.Address = input.Address;
			profile.Ward = input.Ward;
			profile.District = input.District;
			profile.City = input.City;
			profile.Avatar = input.Avatar;
			profile.IsDefault = input.IsDefault;

			await _customerProfileRepository.UpdateAsync(profile);
			await CurrentUnitOfWork.SaveChangesAsync();

			return new CustomerProfileDto
			{
				Id = profile.Id,
				UserId = profile.UserId,
				FullName = profile.FullName,
				PhoneNumber = profile.PhoneNumber,
				Address = profile.Address,
				Ward = profile.Ward,
				District = profile.District,
				City = profile.City,
				Avatar = profile.Avatar,
				IsDefault = profile.IsDefault,
				CreationTime = profile.CreationTime,
				LastModificationTime = profile.LastModificationTime
			};
		}

		public async Task Delete(int id)
		{
			if (AbpSession.UserId == null)
			{
				throw new UserFriendlyException("Bạn cần đăng nhập để xóa thông tin khách hàng");
			}

			var profile = await _customerProfileRepository.FirstOrDefaultAsync(p => p.Id == id && p.UserId == AbpSession.UserId);

			if (profile == null)
			{
				throw new UserFriendlyException("Không tìm thấy thông tin khách hàng");
			}

			await _customerProfileRepository.DeleteAsync(profile);
		}

		public async Task SetAsDefault(int id)
		{
			if (AbpSession.UserId == null)
			{
				throw new UserFriendlyException("Bạn cần đăng nhập để thực hiện thao tác này");
			}

			var profile = await _customerProfileRepository.FirstOrDefaultAsync(p => p.Id == id && p.UserId == AbpSession.UserId);

			if (profile == null)
			{
				throw new UserFriendlyException("Không tìm thấy thông tin khách hàng");
			}

			// Bỏ mặc định của các profile khác
			var existingDefault = await _customerProfileRepository.FirstOrDefaultAsync(p => p.UserId == AbpSession.UserId && p.IsDefault && p.Id != id);
			if (existingDefault != null)
			{
				existingDefault.IsDefault = false;
				await _customerProfileRepository.UpdateAsync(existingDefault);
			}

			profile.IsDefault = true;
			await _customerProfileRepository.UpdateAsync(profile);
		}

		public async Task<PagedResultDto<CustomerProfileDto>> GetAll(GetAllCustomerProfilesInput input)
		{
			var profiles = _customerProfileRepository.GetAll();

			// Filter by keyword
			if (!string.IsNullOrWhiteSpace(input.Keyword))
			{
				string keywordLower = input.Keyword.ToLower();
				profiles = profiles.Where(p => 
					p.FullName.ToLower().Contains(keywordLower) ||
					p.PhoneNumber.Contains(keywordLower) ||
					p.Address.ToLower().Contains(keywordLower) ||
					p.City.ToLower().Contains(keywordLower));
			}

			// Filter by UserId
			if (input.UserId.HasValue && input.UserId.Value > 0)
			{
				profiles = profiles.Where(p => p.UserId == input.UserId.Value);
			}

			// Filter by FullName
			if (!string.IsNullOrWhiteSpace(input.FullName))
			{
				profiles = profiles.Where(p => p.FullName.ToLower().Contains(input.FullName.ToLower()));
			}

			// Filter by PhoneNumber
			if (!string.IsNullOrWhiteSpace(input.PhoneNumber))
			{
				profiles = profiles.Where(p => p.PhoneNumber.Contains(input.PhoneNumber));
			}

			// Filter by City
			if (!string.IsNullOrWhiteSpace(input.City))
			{
				profiles = profiles.Where(p => p.City.ToLower().Contains(input.City.ToLower()));
			}

			// Get total count
			var totalCount = await profiles.CountAsync();

			// Apply sorting
			if (string.IsNullOrWhiteSpace(input.Sorting))
			{
				input.Sorting = "CreationTime DESC";
			}

			// Apply pagination and get results
			var profileDtos = await profiles
				.OrderByDescending(p => p.CreationTime)
				.PageBy(input)
				.Select(p => new CustomerProfileDto
				{
					Id = p.Id,
					UserId = p.UserId,
					FullName = p.FullName,
					PhoneNumber = p.PhoneNumber,
					Address = p.Address,
					Ward = p.Ward,
					District = p.District,
					City = p.City,
					Avatar = p.Avatar,
					IsDefault = p.IsDefault,
					CreationTime = p.CreationTime,
					LastModificationTime = p.LastModificationTime
				})
				.ToListAsync();

			return new PagedResultDto<CustomerProfileDto>(totalCount, profileDtos);
		}

		public async Task<CustomerProfileDto> GetByIdForAdmin(int id)
		{
			var profile = await _customerProfileRepository.FirstOrDefaultAsync(p => p.Id == id);

			if (profile == null)
			{
				throw new UserFriendlyException("Không tìm thấy thông tin khách hàng");
			}

			return new CustomerProfileDto
			{
				Id = profile.Id,
				UserId = profile.UserId,
				FullName = profile.FullName,
				PhoneNumber = profile.PhoneNumber,
				Address = profile.Address,
				Ward = profile.Ward,
				District = profile.District,
				City = profile.City,
				Avatar = profile.Avatar,
				IsDefault = profile.IsDefault,
				CreationTime = profile.CreationTime,
				LastModificationTime = profile.LastModificationTime
			};
		}

		//[AbpAuthorize(PermissionNames.Pages_CustomerProfiles_Edit)]
		public async Task<CustomerProfileDto> UpdateForAdmin(UpdateCustomerProfileDto input)
		{
			var profile = await _customerProfileRepository.FirstOrDefaultAsync(p => p.Id == input.Id);

			if (profile == null)
			{
				throw new UserFriendlyException("Không tìm thấy thông tin khách hàng");
			}

			// Nếu đặt làm mặc định, bỏ mặc định của các profile khác của cùng user
			if (input.IsDefault && !profile.IsDefault)
			{
				var existingDefault = await _customerProfileRepository.FirstOrDefaultAsync(
					p => p.UserId == profile.UserId && p.IsDefault && p.Id != input.Id);
				if (existingDefault != null)
				{
					existingDefault.IsDefault = false;
					await _customerProfileRepository.UpdateAsync(existingDefault);
				}
			}

			profile.FullName = input.FullName;
			profile.PhoneNumber = input.PhoneNumber;
			profile.Address = input.Address;
			profile.Ward = input.Ward;
			profile.District = input.District;
			profile.City = input.City;
			profile.Avatar = input.Avatar;
			profile.IsDefault = input.IsDefault;

			await _customerProfileRepository.UpdateAsync(profile);
			await CurrentUnitOfWork.SaveChangesAsync();

			return new CustomerProfileDto
			{
				Id = profile.Id,
				UserId = profile.UserId,
				FullName = profile.FullName,
				PhoneNumber = profile.PhoneNumber,
				Address = profile.Address,
				Ward = profile.Ward,
				District = profile.District,
				City = profile.City,
				Avatar = profile.Avatar,
				IsDefault = profile.IsDefault,
				CreationTime = profile.CreationTime,
				LastModificationTime = profile.LastModificationTime
			};
		}

		//[AbpAuthorize(PermissionNames.Pages_CustomerProfiles_Edit)]
		public async Task DeleteForAdmin(int id)
		{
			var profile = await _customerProfileRepository.FirstOrDefaultAsync(p => p.Id == id);

			if (profile == null)
			{
				throw new UserFriendlyException("Không tìm thấy thông tin khách hàng");
			}

			await _customerProfileRepository.DeleteAsync(profile);
		}
	}
}

