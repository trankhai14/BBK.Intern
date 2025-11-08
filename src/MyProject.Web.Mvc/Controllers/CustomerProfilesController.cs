using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using MyProject.Controllers;
using MyProject.CustomerProfiles;
using MyProject.CustomerProfiles.Dto;
using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using MyProject.Authorization;

namespace MyProject.Web.Controllers
{
	[AbpMvcAuthorize(PermissionNames.Pages_Users)]
	public class CustomerProfilesController : MyProjectControllerBase
	{
		private readonly ICustomerProfileAppService _customerProfileAppService;

		public CustomerProfilesController(ICustomerProfileAppService customerProfileAppService)
		{
			_customerProfileAppService = customerProfileAppService;
		}

		public async Task<ActionResult> Index(GetAllCustomerProfilesInput input)
		{
			if (input == null)
			{
				input = new GetAllCustomerProfilesInput();
			}
			
			var output = await _customerProfileAppService.GetAll(input);
			return View(output);
		}

		public async Task<ActionResult> EditModal(int customerProfileId)
		{
			var profile = await _customerProfileAppService.GetByIdForAdmin(customerProfileId);
			
			var updateDto = new UpdateCustomerProfileDto
			{
				Id = profile.Id,
				FullName = profile.FullName,
				PhoneNumber = profile.PhoneNumber,
				Address = profile.Address,
				Ward = profile.Ward,
				District = profile.District,
				City = profile.City,
				Avatar = profile.Avatar,
				IsDefault = profile.IsDefault
			};

			return PartialView("_EditModal", updateDto);
		}
	}
}

