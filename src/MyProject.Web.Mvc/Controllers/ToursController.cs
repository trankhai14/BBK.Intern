using System.IO;
using System.Linq;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyProject.Controllers;
using MyProject.Tours;
using MyProject.Tours.Dto;
using MyProject.Web.Models.Tours;
using Microsoft.AspNetCore.Hosting;
using Abp.Application.Services.Dto;
using MyProject.Products.Dtos;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;

namespace MyProject.Web.Controllers
{
	public class ToursController : MyProjectControllerBase
	{
		private readonly ITourAppService _tourAppService;
		private readonly IWebHostEnvironment webHostEnvironment;

		public ToursController(ITourAppService tourAppService, IWebHostEnvironment webHostEnvironment)
		{
			_tourAppService = tourAppService;
			this.webHostEnvironment = webHostEnvironment;
		}
		public async Task<IActionResult> Index(GetAllToursInput input)
		{
			var output = await _tourAppService.GetAllTours(input);
			var model = new TourViewModel(output.Items);
			return View(model);
		}

	
		public async Task<IActionResult> Create(CreateTourDto model)
		{
			try
			{
				if (ModelState.IsValid)
				{
					// Upload ảnh và lấy tên file duy nhất
					string uniqueFileName = UploadImage(model.AttachmentFile);

					// Gán đường dẫn file vào model
					model.Attachment = uniqueFileName;

					// Gọi service để tạo mới sản phẩm
					await _tourAppService.CreateTour(model);

					TempData["SuccessMessage"] = "Thêm sản phẩm thành công";
					return RedirectToAction("Index");
				}
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = ex.Message });
			}

			var errors = ModelState.Values.SelectMany(v => v.Errors)
																		 .Select(e => e.ErrorMessage)
																		 .ToList();
			return Json(new { success = false, errors });
		}

		private string UploadImage(IFormFile AttachmentFile)
		{
			if (AttachmentFile != null && AttachmentFile.Length > 0)
			{
				// Kiểm tra định dạng ảnh
				string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
				string fileExtension = Path.GetExtension(AttachmentFile.FileName).ToLower();
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
					AttachmentFile.CopyTo(fileStream);
				}

				return "/tours/" + uniqueFileName;
			}

			return null; // Trả về ảnh mặc định nếu không có ảnh upload
		}


		private void DeleteFileFromUploads(string filePath)
		{
			string fileName = Path.GetFileName(filePath);
			if (string.IsNullOrEmpty(fileName)) return; // Kiểm tra tên file hợp lệ

			string folderPath = @"E:\Uploads"; // Thư mục chứa ảnh
			string fullPath = Path.Combine(folderPath, fileName); // Đường dẫn đầy đủ

			if (System.IO.File.Exists(fullPath)) // Kiểm tra file có tồn tại không
			{
				System.IO.File.Delete(fullPath); // Xóa file
				Console.WriteLine($"Đã xóa file: {fullPath}");
			}
			else
			{
				Console.WriteLine("File không tồn tại!");
			}
		}


		public async Task<IActionResult> EditTourModal(long tourId)
		{
			var tour = await _tourAppService.GetTourById(new EntityDto<long>(tourId));
			if (tour == null)
			{
				return Content("Không tìm thấy tour");
			}
			var model = new EditTourModel
			{
				Tour = tour
			};
			return PartialView("EditModal", model);
		}


		public async Task<IActionResult> EditAndUpdateTour(UpdateTourDto model)
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
				var existingTour = await _tourAppService.GetTourById(new EntityDto<long>(model.Id));
				if (existingTour == null)
				{
					return Json(new { success = false, message = "Tour không tồn tại hoặc đã bị xóa." });
				}

				// Kiểm tra xem người dùng có tải lên ảnh mới không
				if (model.AttachmentFile != null && model.AttachmentFile.Length > 0)
				{
					// Danh sách các định dạng ảnh được phép tải lên
					string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".jfif" };
					string fileExtension = Path.GetExtension(model.AttachmentFile.FileName).ToLower();

					// Kiểm tra xem ảnh có thuộc định dạng hợp lệ không
					if (!allowedExtensions.Contains(fileExtension))
					{
						return Json(new { success = false, message = "Định dạng ảnh không hợp lệ. Vui lòng chọn file .jpg, .png, .gif." });
					}

					// Nếu sản phẩm đã có ảnh trước đó, xóa ảnh cũ trước khi cập nhật ảnh mới
					if (!string.IsNullOrEmpty(existingTour.Attachment))
					{
						DeleteFileFromUploads(existingTour.Attachment); // Gọi hàm xóa ảnh cũ
					}

					// Upload ảnh mới và cập nhật đường dẫn vào model
					model.Attachment = UploadImage(model.AttachmentFile);
				}
				else
				{
					// Nếu người dùng không chọn ảnh mới, giữ nguyên ảnh cũ
					model.Attachment = existingTour.Attachment;
				}

				// Gọi service để cập nhật thông tin sản phẩm trong database
				await _tourAppService.UpdateTour(model);

				// Trả về kết quả thành công kèm theo đường dẫn ảnh mới (nếu có thay đổi)
				return Json(new { success = true, message = "Cập nhật sản phẩm thành công", imagePath = model.Attachment });
			}
			catch (Exception ex)
			{
				// Xử lý ngoại lệ nếu có lỗi xảy ra trong quá trình cập nhật
				return Json(new { success = false, message = ex.Message });
			}
		}

		public async Task<IActionResult> DeleteImage(long tourId)
		{
			var existingTour = await _tourAppService.GetTourById(new EntityDto<long>(tourId));

			if (existingTour == null)
			{
				return Json(new { success = false, message = "Sản phẩm không tồn tại." });
			}

			if (string.IsNullOrEmpty(existingTour.Attachment))
			{
				return Json(new { success = false, message = "Sản phẩm này không có ảnh để xóa." });
			}

			try
			{
				// Xóa file ảnh trên server
				DeleteFileFromUploads(existingTour.Attachment);

				// Cập nhật lại sản phẩm trong DB (xóa đường dẫn ảnh)
				var updateTourDto = new UpdateTourDto()
				{
					Id = existingTour.Id,
					TourName = existingTour.TourName,
					MinGroupSize = existingTour.MinGroupSize,
					MaxGroupSize = existingTour.MaxGroupSize,
					TourTypeId = existingTour.TourTypeId,
					StartDate = existingTour.StartDate,
					EndDate = existingTour.EndDate,
					Transportation = existingTour.Transportation,
					TourPrice = existingTour.TourPrice,
					PhoneNumber = existingTour.PhoneNumber,
					Description = existingTour.Description,
					Attachment = null,

				};
				await _tourAppService.UpdateTour(updateTourDto);

				return Json(new { success = true, message = "Ảnh đã được xóa thành công." });
			}
			catch (Exception)
			{
				return Json(new { success = false, message = "Đã xảy ra lỗi khi xóa ảnh. Vui lòng thử lại." });
			}
		}

		public async Task<IActionResult> DeleteTourAndImg(EntityDto<long> input)
		{
			var tour = await _tourAppService.GetTourById(input);

			if (tour != null) {
				DeleteFileFromUploads(tour.Attachment);
				await _tourAppService.DeleteTour(input);

				return Json(new { success = true, Message = "Xóa tour và ảnh thành công" });
			}

			return Json(new { success = false, Message = "Tour không tồn tại" });
		}
	}
}
