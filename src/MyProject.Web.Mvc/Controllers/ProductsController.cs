using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyProject.Authorization;
using MyProject.Categories;
using MyProject.Controllers;
using MyProject.Product;
using MyProject.Product.Dtos;
using MyProject.Products;
using MyProject.Products.Dtos;
using MyProject.Suppliers;
using MyProject.Suppliers.Dto;
using MyProject.Tasks;
using MyProject.Web.Models.Products;



namespace MyProject.Web.Controllers
{
	[AbpMvcAuthorize(PermissionNames.Pages_Products)]
	public class ProductsController : MyProjectControllerBase
	{
		private readonly IProductAppService _productAppService;
		private readonly IWebHostEnvironment webHostEnvironment;
		private readonly ICategoryAppService _categoryAppService;
		private readonly ISupplierAppService _supplierAppService;
		private readonly IPermissionChecker _permissionChecker;


		public ProductsController(IProductAppService productService, IWebHostEnvironment webHostEnvironment, ICategoryAppService categoryAppService, ISupplierAppService supplierAppService, IPermissionChecker permissionChecker)
		{
			_productAppService = productService;
			this.webHostEnvironment = webHostEnvironment;
			_categoryAppService = categoryAppService;
			_supplierAppService = supplierAppService;
			_permissionChecker = permissionChecker;
		}

		public async Task<ActionResult> Index(GetAllProductsInput input)
		{
			var output = await _productAppService.GetAll(input);
			var Categories = await _productAppService.GetAllCategory();
			
			// Chỉ lấy danh sách suppliers nếu user có permission
			List<SupplierDto> Suppliers = new List<SupplierDto>();
			if (_permissionChecker.IsGranted(PermissionNames.Pages_Suppliers))
			{
				try
				{
					Suppliers = await _supplierAppService.GetAllList();
				}
				catch (Abp.Authorization.AbpAuthorizationException)
				{
					// Nếu không có permission, để danh sách rỗng
					Suppliers = new List<SupplierDto>();
				}
			}
			
			var model = new ProductViewModel(output.Items);
			model.CategoryLists = Categories;
			model.SupplierLists = Suppliers;
			return View(model);
		}

		public async Task<ActionResult> EditModal(int productId)
		{
			var product = await _productAppService.GetAsync(new EntityDto<int>(productId));
			var categories = await _categoryAppService.GetAllCategory();
			
			// Chỉ lấy danh sách suppliers nếu user có permission
			List<SupplierDto> suppliers = new List<SupplierDto>();
			if (_permissionChecker.IsGranted(PermissionNames.Pages_Suppliers))
			{
				try
				{
					suppliers = await _supplierAppService.GetAllList();
				}
				catch (Abp.Authorization.AbpAuthorizationException)
				{
					// Nếu không có permission, để danh sách rỗng
					suppliers = new List<SupplierDto>();
				}
			}

			var model = new EditProductViewModel
			{
				Product = product,
				Categories = categories,
				Suppliers = suppliers
			};
			return PartialView("_EditModal", model);
		}


		public async Task<ActionResult> Detail(int productId)
		{
			var product = await _productAppService.Detail(new EntityDto<int>(productId));

			if (product == null)
			{
				return NotFound();
			}

			var model = new ProductDetailViewModel
			{
				Product = product
			};
			return View(model);
		}

		public async Task<IActionResult> Create(CreateProductDto model)
		{
			try
			{
				if (ModelState.IsValid)
				{
					// Upload ảnh và lấy tên file duy nhất
					string uniqueFileName = UploadImage(model.ImageFile);

					// Gán đường dẫn file vào model
					model.Image = uniqueFileName;

					// Gọi service để tạo mới sản phẩm
					await _productAppService.Create(model);

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

				return "/products/" + uniqueFileName;
			}

			return "/products/default.png"; // Trả về ảnh mặc định nếu không có ảnh upload
		}

		public async Task<IActionResult> EditAndUploadDeleteImage(UpdateProductDto model)
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
				var existingProduct = await _productAppService.GetAsync(new EntityDto<int>(model.Id));
				if (existingProduct == null)
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
					if (!string.IsNullOrEmpty(existingProduct.Image))
					{
						DeleteFile(existingProduct.Image); // Gọi hàm xóa ảnh cũ
					}

					// Upload ảnh mới và cập nhật đường dẫn vào model
					model.Image = UploadImage(model.ImageFile);
				}
				else
				{
					// Nếu người dùng không chọn ảnh mới, giữ nguyên ảnh cũ
					model.Image = existingProduct.Image;
				}

				// Gọi service để cập nhật thông tin sản phẩm trong database
				await _productAppService.Update(model);

				// Trả về kết quả thành công kèm theo đường dẫn ảnh mới (nếu có thay đổi)
				return Json(new { success = true, message = "Cập nhật sản phẩm thành công", imagePath = model.Image });
			}
			catch (Exception ex)
			{
				// Xử lý ngoại lệ nếu có lỗi xảy ra trong quá trình cập nhật
				return Json(new { success = false, message = ex.Message });
			}
		}


		private void DeleteFile(string fileName)
		{
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


		public async Task<IActionResult> DeleteImage(int productId)
		{
			var existingProduct = await _productAppService.GetAsync(new EntityDto<int>(productId));

			if (existingProduct == null)
			{
				return Json(new { success = false, message = "Sản phẩm không tồn tại." });
			}

			if (string.IsNullOrEmpty(existingProduct.Image))
			{
				return Json(new { success = false, message = "Sản phẩm này không có ảnh để xóa." });
			}

			try
			{
				// Xóa file ảnh trên server
				DeleteFile(existingProduct.Image);

				// Cập nhật lại sản phẩm trong DB (xóa đường dẫn ảnh)
				var updateProductDto = new UpdateProductDto()
				{
					Id = existingProduct.Id,
					Name = existingProduct.Name,
					Description = existingProduct.Description,
					Price = existingProduct.Price,
					State = existingProduct.State,
					CategoryId = existingProduct.CategoryId,
					SupplierId = existingProduct.SupplierId,
					Image = null,
				};
				await _productAppService.Update(updateProductDto);

				return Json(new { success = true, message = "Ảnh đã được xóa thành công." });
			}
			catch (Exception)
			{
				return Json(new { success = false, message = "Đã xảy ra lỗi khi xóa ảnh. Vui lòng thử lại." });
			}
		}

		/// <summary>
		/// Hiển thị modal thông tin kỹ thuật sản phẩm
		/// </summary>
		public async Task<ActionResult> SpecificationModal(int productId)
		{
			var product = await _productAppService.GetAsync(new EntityDto<int>(productId));
			if (product == null)
			{
				return Json(new { success = false, message = "Không tìm thấy sản phẩm" });
			}

			var model = new EditProductViewModel
			{
				Product = product
			};

			return PartialView("_ProductSpecificationModal", model);
		}

		/// <summary>
		/// Lưu hoặc cập nhật thông tin kỹ thuật sản phẩm
		/// </summary>
		[HttpPost]
		public async Task<IActionResult> SaveSpecification([FromQuery] int productId, [FromBody] UpdateProductSpecificationDto specification)
		{
			try
			{
				// Kiểm tra nếu specification null hoặc không có dữ liệu
				if (specification == null)
				{
					return Json(new { success = false, message = "Dữ liệu thông tin kỹ thuật không hợp lệ" });
				}

				var product = await _productAppService.GetAsync(new EntityDto<int>(productId));
				if (product == null)
				{
					return Json(new { success = false, message = "Không tìm thấy sản phẩm" });
				}

				// Đảm bảo ProductId trong specification khớp với productId từ query
				specification.ProductId = productId;

				var updateProductDto = new UpdateProductDto
				{
					Id = productId,
					Name = product.Name,
					Description = product.Description,
					Price = product.Price,
					State = product.State,
					CategoryId = product.CategoryId,
					SupplierId = product.SupplierId,
					Image = product.Image,
					Brand = product.Brand,
					WeightInGrams = product.WeightInGrams,
					WidthCm = product.WidthCm,
					HeightCm = product.HeightCm,
					LengthCm = product.LengthCm,
					Specification = specification
				};

				await _productAppService.Update(updateProductDto);

				return Json(new { success = true, message = "Lưu thông tin kỹ thuật thành công" });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = ex.Message });
			}
		}
	}
}
