using System.IO;
using System.Linq;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyProject.Controllers;
using MyProject.Sliders;
using MyProject.Sliders.Dto;
using MyProject.Web.Models.Sliders;
using Abp.Application.Services.Dto;
using MyProject.Products.Dtos;

namespace MyProject.Web.Controllers
{
	public class SlidersController: MyProjectControllerBase
	{
		private readonly ISliderAppService _sliderAppService;
		private readonly IWebHostEnvironment webHostEnvironment;

		public SlidersController(ISliderAppService sliderAppService, IWebHostEnvironment webHostEnvironment)
		{
			_sliderAppService = sliderAppService;
			this.webHostEnvironment = webHostEnvironment;
		}
		public async Task<ActionResult> Index(GetAllSlidersInput input)
		{
			var output = await _sliderAppService.GetAllSlider(input);
			var model = new SliderViewModel(output.Items);
			return View(model);
		}

		public async Task<IActionResult> Create(CreateSliderDto model)
		{
			try
			{
				if (ModelState.IsValid)
				{
					string uniqueFileName = UploadImage(model.ImageFile);

					model.Image = uniqueFileName;

					await _sliderAppService.CreateSlider(model);

					TempData["SuccessMessage"] = "Thêm slider thành công";
					return RedirectToAction("Index");
				}
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = ex.Message });
			}

			var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
			return Json(new { success = false, errors });
		}

		public async Task<IActionResult> EditModal(int sliderId)
		{
			var slider = await _sliderAppService.GetSlider(new EntityDto<int>(sliderId));

			var model = new EditSliderViewModel
			{
				Slider = slider
			};
			return PartialView("_EditModal", model);
		}

		public async Task<IActionResult> Detail(int sliderId)
		{
			var slider = await _sliderAppService.GetSlider(new EntityDto<int>(sliderId));
			var model = new EditSliderViewModel
			{
				Slider = slider
			};
			return View(model);
		}



		public async Task<IActionResult> EditAndUploadDeleteImage(UpdateSliderDto model)
		{
			try
			{
				// Kiểm tra xem dữ liệu đầu vào có hợp lệ không
				if (!ModelState.IsValid)
				{
					// Lấy danh sách lỗi nếu có
					var errors = ModelState.Values.SelectMany(v => v.Errors)
																				.Select(e => e.ErrorMessage)
																				.ToList();
					return Json(new { success = false, errors }); // Trả về lỗi dưới dạng JSON
				}

				// Kiểm tra xem sản phẩm có tồn tại trong hệ thống không
				var existingSlider = await _sliderAppService.GetSlider(new EntityDto<int>(model.Id));
				if (existingSlider == null)
				{
					return Json(new { success = false, message = "Không tìm thấy sản phẩm." }); // Trả về lỗi nếu không tìm thấy
				}

				// Kiểm tra xem người dùng có tải lên ảnh mới không
				if (model.ImageFile != null && model.ImageFile.Length > 0)
				{
					// Danh sách các định dạng ảnh được phép tải lên
					string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".jfif" };
					string fileExtension = Path.GetExtension(model.ImageFile.FileName).ToLower();

					// Kiểm tra xem ảnh có thuộc định dạng hợp lệ không
					if (!allowedExtensions.Contains(fileExtension))
					{
						return Json(new { success = false, message = "Định dạng ảnh không hợp lệ. Vui lòng chọn file .jpg, .png, .gif." });
					}

					// Nếu sản phẩm đã có ảnh trước đó, xóa ảnh cũ trước khi cập nhật ảnh mới
					if (!string.IsNullOrEmpty(existingSlider.Image))
					{
						DeleteFile(existingSlider.Image); // Gọi hàm xóa ảnh cũ
					}

					// Upload ảnh mới và cập nhật đường dẫn vào model
					model.Image = UploadImage(model.ImageFile);
				}
				else
				{
					// Nếu người dùng không chọn ảnh mới, giữ nguyên ảnh cũ
					model.Image = existingSlider.Image;
				}

				// Gọi service để cập nhật thông tin sản phẩm trong database
				await _sliderAppService.UpdateSlider(model);

				// Trả về kết quả thành công kèm theo đường dẫn ảnh mới (nếu có thay đổi)
				return Json(new { success = true, message = "Cập nhật sản phẩm thành công", imagePath = model.Image });
			}
			catch (Exception ex)
			{
				// Xử lý ngoại lệ nếu có lỗi xảy ra trong quá trình cập nhật
				return Json(new { success = false, message = ex.Message });
			}
		}

		// delete image
		private void DeleteFile(string imagePath)
		{
			if (string.IsNullOrEmpty(imagePath)) return;

			string fullPath = Path.Combine(webHostEnvironment.WebRootPath, imagePath.TrimStart('/'));
			if (System.IO.File.Exists(fullPath))
			{
				System.IO.File.Delete(fullPath);
			}
		}


		//upload image
		private string UploadImage(IFormFile ImageFile)
		{
			if (ImageFile != null && ImageFile.Length > 0)
			{
				// Kiểm tra định dạng ảnh
				string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
				string fileExtension = Path.GetExtension(ImageFile.FileName).ToLower();
				if (!allowedExtensions.Contains(fileExtension))
				{
					throw new ArgumentException("Định dạng ảnh không hợp lệ. Vui lòng chọn ảnh có định dạng hợp lệ.");
				}

				string uploadsFolder = @"E:\Uploads\";
				Directory.CreateDirectory(uploadsFolder); // Tạo thư mục nếu chưa có

				string uniqueFileName = DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + Guid.NewGuid().ToString("N") + fileExtension;
				string filePath = Path.Combine(uploadsFolder, uniqueFileName);

				using (var fileStream = new FileStream(filePath, FileMode.Create))
				{
					ImageFile.CopyTo(fileStream);
				}

				return "/sliders/" + uniqueFileName;
			}

			return "/sliders/default.png"; // Trả về ảnh mặc định nếu không có ảnh upload
		}

		public async Task<IActionResult> DeleteImage(int sliderId)
		{
			var existingSlider = await _sliderAppService.GetSlider(new EntityDto<int>(sliderId));

			if (existingSlider == null)
			{
				return Json(new { success = false, message = "slider không tồn tại." });
			}

			if (string.IsNullOrEmpty(existingSlider.Image))
			{
				return Json(new { success = false, message = "slider này không có ảnh để xóa." });
			}

			try
			{
				// Xóa file ảnh trên server
				DeleteFile(existingSlider.Image);

				// Cập nhật lại sản phẩm trong DB (xóa đường dẫn ảnh)
				var updateSliderDto = new UpdateSliderDto()
				{
					Id = existingSlider.Id,
					Title = existingSlider.Title,
					Description = existingSlider.Description,
					Image = null,
					IsActive = existingSlider.IsActive,
				};
				await _sliderAppService.UpdateSlider(updateSliderDto);

				return Json(new { success = true, message = "Ảnh đã được xóa thành công." });
			}
			catch (Exception)
			{
				return Json(new { success = false, message = "Đã xảy ra lỗi khi xóa ảnh. Vui lòng thử lại." });
			}
		}


	}
}


