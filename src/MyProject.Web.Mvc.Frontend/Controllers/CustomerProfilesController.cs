using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyProject.Controllers;
using MyProject.CustomerProfiles;
using MyProject.CustomerProfiles.Dto;

namespace MyProject.Web.Controllers
{
	[AbpMvcAuthorize]
	public class CustomerProfilesController : MyProjectControllerBase
	{
		private readonly ICustomerProfileAppService _customerProfileAppService;

		public CustomerProfilesController(ICustomerProfileAppService customerProfileAppService)
		{
			_customerProfileAppService = customerProfileAppService;
		}

		private string UploadAvatar(IFormFile avatarFile)
		{
			if (avatarFile != null && avatarFile.Length > 0)
			{
				// Kiểm tra định dạng ảnh
				string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
				string fileExtension = Path.GetExtension(avatarFile.FileName).ToLower();
				if (!allowedExtensions.Contains(fileExtension))
				{
					throw new ArgumentException("Định dạng ảnh không hợp lệ. Vui lòng chọn ảnh có định dạng .jpg, .jpeg, .png hoặc .gif");
				}

				// Kiểm tra kích thước file (tối đa 5MB)
				if (avatarFile.Length > 5 * 1024 * 1024)
				{
					throw new ArgumentException("Kích thước ảnh không được vượt quá 5MB");
				}

				string uploadsFolder = @"E:\Uploads\avatars\";
				Directory.CreateDirectory(uploadsFolder); // Tạo thư mục nếu chưa có

				string uniqueFileName = DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + Guid.NewGuid().ToString("N") + fileExtension;
				string filePath = Path.Combine(uploadsFolder, uniqueFileName);

				using (var fileStream = new FileStream(filePath, FileMode.Create))
				{
					avatarFile.CopyTo(fileStream);
				}

				return "/avatars/" + uniqueFileName;
			}

			return null; // Trả về null nếu không có ảnh upload
		}

		private void DeleteAvatarFile(string avatarPath)
		{
			if (string.IsNullOrEmpty(avatarPath)) return;

			string fileName = Path.GetFileName(avatarPath);
			if (string.IsNullOrEmpty(fileName)) return;

			string folderPath = @"E:\Uploads\avatars\";
			string fullPath = Path.Combine(folderPath, fileName);

			if (System.IO.File.Exists(fullPath))
			{
				System.IO.File.Delete(fullPath);
			}
		}

		public async Task<ActionResult> Index()
		{
			var profiles = await _customerProfileAppService.GetAllByCurrentUser();
			return View(profiles);
		}

		public ActionResult Create()
		{
			return View();
		}

		[HttpPost]
		public async Task<ActionResult> Create(CreateCustomerProfileDto input)
		{
			if (ModelState.IsValid)
			{
				try
				{
					// Upload avatar nếu có
					if (input.AvatarFile != null)
					{
						input.Avatar = UploadAvatar(input.AvatarFile);
					}

					await _customerProfileAppService.Create(input);
					return RedirectToAction("Index");
				}
				catch (Exception ex)
				{
					ModelState.AddModelError("", ex.Message);
				}
			}
			return View(input);
		}

		public async Task<ActionResult> Edit(int id)
		{
			var profile = await _customerProfileAppService.GetById(id);
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
			return View(updateDto);
		}

		[HttpPost]
		public async Task<ActionResult> Edit(UpdateCustomerProfileDto input)
		{
			if (ModelState.IsValid)
			{
				try
				{
					// Lấy thông tin profile hiện tại để xóa avatar cũ nếu có
					var existingProfile = await _customerProfileAppService.GetById(input.Id);
					
					// Upload avatar mới nếu có
					if (input.AvatarFile != null && input.AvatarFile.Length > 0)
					{
						// Xóa avatar cũ nếu có
						if (!string.IsNullOrEmpty(existingProfile.Avatar))
						{
							DeleteAvatarFile(existingProfile.Avatar);
						}
						input.Avatar = UploadAvatar(input.AvatarFile);
					}
					else
					{
						// Giữ nguyên avatar cũ nếu không upload mới
						input.Avatar = existingProfile.Avatar;
					}

					await _customerProfileAppService.Update(input);
					return RedirectToAction("Index");
				}
				catch (Exception ex)
				{
					ModelState.AddModelError("", ex.Message);
				}
			}
			return View(input);
		}

		[HttpPost]
		public async Task<ActionResult> Delete(int id)
		{
			// Lấy thông tin profile để xóa avatar file
			var profile = await _customerProfileAppService.GetById(id);
			if (!string.IsNullOrEmpty(profile.Avatar))
			{
				DeleteAvatarFile(profile.Avatar);
			}

			await _customerProfileAppService.Delete(id);
			return RedirectToAction("Index");
		}

		[HttpPost]
		public async Task<ActionResult> SetAsDefault(int id)
		{
			await _customerProfileAppService.SetAsDefault(id);
			return RedirectToAction("Index");
		}
	}
}

