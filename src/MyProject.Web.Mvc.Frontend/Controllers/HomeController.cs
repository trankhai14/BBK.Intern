using Microsoft.AspNetCore.Mvc;
using Abp.AspNetCore.Mvc.Authorization;
using MyProject.Controllers;
using MyProject.Products;
using MyProject.Websites;
using Abp.Application.Services.Dto;
using MyProject.Product.Dtos;
using MyProject.Products.Dtos;
using MyProject.Web.Dto;
using MyProject.Web.Models.Web;
using System.Threading.Tasks;
using System;
using MyProject.Web.Models.Products;
using System.Linq;
using System.Collections.Generic;
using System.Drawing.Design;
using MyProject.Categories;
using MyProject.Sliders;
using MyProject.Carts;
using MyProject.Web.Models.Home;
using MyProject.Web.Models.Orders;
using MyProject.OrderDetails;
using MyProject.Orders;
using MyProject.Users;
using MyProject.CustomerProfiles;
using MyProject.CustomerProfiles.Dto;
using System.IO;
using Microsoft.AspNetCore.Http;
namespace MyProject.Web.Controllers
{
	//[AbpMvcAuthorize]
	public class HomeController : MyProjectControllerBase
	{
		private readonly IProductAppService _productAppService;
		private readonly ICategoryAppService _categoryAppService;
		private readonly IWebAppService _webAppService;
		private readonly ISliderAppService _sliderAppService;
		private readonly ICartAppService _cartAppService;
		private readonly IOrderAppService _orderAppService;
		private readonly IOrderDetailAppService _orderDetailAppService;
		private readonly IUserAppService _userAppService;
		private readonly ICustomerProfileAppService _customerProfileAppService;

		
		public HomeController
			(
			IProductAppService productAppService,
			IWebAppService webAppService,
			ICategoryAppService categoryAppService,
			ISliderAppService sliderAppService,
			ICartAppService cartAppService,
			IOrderAppService orderAppService,
			IOrderDetailAppService orderDetailAppService,
			IUserAppService userAppService,
			ICustomerProfileAppService customerProfileAppService
			)
		{
			_productAppService = productAppService;
			_webAppService = webAppService;
			_categoryAppService = categoryAppService;
			_sliderAppService = sliderAppService;
			_cartAppService = cartAppService;
			_orderAppService = orderAppService;
			_orderDetailAppService = orderDetailAppService;
			_userAppService = userAppService;
			_customerProfileAppService = customerProfileAppService;
		}

		public async Task<IActionResult> Index(int page = 1, int pageSize = 5)
		{
			// Lấy danh sách sản phẩm theo phân trang
			var productsResult = await _productAppService.GetAll(new GetAllProductsInput
			{
				MaxResultCount = 20,
				SkipCount = 0,
				Sorting = "CreationTime DESC",
			});

			// Lấy slider
			var sliders = await _sliderAppService.GetSliderByActive();

			// Chia danh sách sản phẩm theo phân trang (hiển thị 5 sản phẩm một lần)
			var paginatedProducts = productsResult.Items.Skip((page - 1) * pageSize).Take(pageSize).ToList();

			// Khởi tạo ProductViewModel
			var productViewModel = new ProductViewModel(productsResult.Items)
			{
				CurrentPage = page,
				TotalPages = (int)Math.Ceiling((double)productsResult.Items.Count / pageSize),
				SliderList = sliders
			};


			// Lấy danh sách chuyên mục + sản phẩm
			List<CategoryProductViewModel> categoryProductViewModels = new List<CategoryProductViewModel>();

			var categories = await _categoryAppService.GetAllCategory();
			if (categories != null)
			{
				foreach (var item in categories)
				{
					var productsOfCategory = await _productAppService.Search(new GetAllProductsInput
					{
						CategoryId = item.Id
					});

					categoryProductViewModels.Add(new CategoryProductViewModel
					{
						CategoryId = item.Id,
						CategoryName = item.CategoryName,
						Products = productsOfCategory.Items.Take(10).ToList()
					});
				}
			}

			var homePageViewModel = new HomePageViewModel
			{
				ProductData = productViewModel,
				CategoryProducts = categoryProductViewModels
			};

			return View(homePageViewModel);
		}



		public async Task<IActionResult> SearchProductsWeb(GetAllProductWeb input)
		{
			//ProductViewModel webViewModel = new ProductViewModel();
			var result = await _productAppService.Search(new GetAllProductsInput
			{
				Keyword = input.Keyword
			});

			var count = result.Items.Count();

			if (result != null)
			{
				var model = new ProductViewModel(result.Items)
				{
					count = count
				};
				return View("_ProducResultSearch", model);
			}
			return View("_ProducResultSearch", new ProductViewModel(new List<ProductListDto>()));
		}

	

		public async Task<IActionResult> GetDetailProduct(EntityDto<int> productId)
		{
			var product = await _productAppService.GetAsync(productId);
			var category = await _categoryAppService.GetCategoryById(product.CategoryId);

			// Lấy sản phẩm tương tự (cùng category, loại trừ sản phẩm hiện tại)
			var relatedProductsResult = await _productAppService.Search(new GetAllProductsInput
			{
				CategoryId = product.CategoryId,
				MaxResultCount = 8,
				SkipCount = 0
			});

			var relatedProducts = relatedProductsResult.Items
				.Where(p => p.Id != product.Id)
				.Take(5)
				.ToList();

			var model = new DetailProductModel()
			{
				Id = product.Id,
				Name = product.Name,
				Description = product.Description,
				Price = product.Price,
				Image = product.Image,
				CategoryId = product.CategoryId,
				CategoryName = category?.CategoryName ?? "",
				Brand = product.Brand,
				State = product.State,
				CreationTime = product.CreationTime,
				WeightInGrams = product.WeightInGrams,
				WidthCm = product.WidthCm,
				HeightCm = product.HeightCm,
				LengthCm = product.LengthCm,
				RelatedProducts = relatedProducts
			};

			return View("_DetailProductWeb", model);
		}

		public async Task<IActionResult> PageAllProduct(int? categoryId)
		{
			var allProducts = await _productAppService.GetAllProducts();
			string NameBreakCrum = "Tất cả sản phẩm";

			//lấy name của category
			if (categoryId != null)
			{
				var category = await _categoryAppService.GetCategoryById(categoryId);
				if (category != null)
				{
					NameBreakCrum = category.CategoryName;
					allProducts = allProducts.Where(p => p.CategoryId == categoryId.Value).ToList();
				}
			}


			var selectedProducts = allProducts.Take(10).ToList();
			var model = new ProductViewModel(selectedProducts)
			{
				CategoryId = categoryId,
				CategoryName = NameBreakCrum,
			};
			return View("_AllProducts", model); // Trả về View
		}

		public async Task<IActionResult> LoadMoreProducts(int? categoryId, string? sortOrder, int page, int pageSize = 10)
		{
			var allProducts = await _productAppService.GetAllProducts();

			switch (sortOrder)
			{
				case "price_asc":
					allProducts = allProducts.OrderBy(p => p.Price).ToList();
					break;
				case "price_desc":
					allProducts = allProducts.OrderByDescending(p => p.Price).ToList();
					break;
				default:
					break;
			}

			if (categoryId.HasValue)
			{
				allProducts = allProducts.Where(p => p.CategoryId == categoryId.Value).ToList();
			}

			// sắp xếp theo giá

			var products = allProducts
					.Skip((page - 1) * pageSize)
					.Take(pageSize)
					.ToList();

			if (products.Any())
			{
				return PartialView("_GetProductPage", new ProductViewModel(products)); // Trả về PartialView
			}

			return NoContent();  // Trả về rỗng nếu không còn sản phẩm
		}


		public IActionResult UserProfile()
		{
			return View("ProfileUser");
		}

		public async Task<IActionResult> FilterStatus(int? orderStatus = 5)
		{

			var orderOutputs = await _orderAppService.GetStatusOrder(orderStatus);
			var orderIds = orderOutputs.Select(o => o.OrderId).ToList(); // Lấy danh sách ID
			var orderList = await _orderDetailAppService.GetOrderByIdAndStatus(orderIds);
			var orderStatusDict = orderOutputs.ToDictionary(o => o.OrderId, o => o.OrderStatus);

			// Gán OrderStatus cho từng đơn hàng trong orderList
			foreach (var order in orderList)
			{
				if (orderStatusDict.TryGetValue(order.OrderId, out var status))
				{
					order.OrderStatus = status;
				}
			}

			// Lấy danh sách ID sản phẩm cần lấy thông tin
			var productIds = orderList.Select(o => o.ProductId).Distinct().ToList();
			var productList = await _productAppService.GetProductByIds(productIds);
			var model = new FilterStatusOrderViewModel
			{
				ListOrder = orderList,
				OrderStatus = orderStatus,
				Products = productList
			};
			return PartialView("FilterStatus", model);
		}

		public async Task<IActionResult> LoadPartialView(string nameView)
		{
			if (nameView == "_UserInfos")
			{
				var userId = AbpSession.UserId ?? 0; // 0 là giá trị mặc định
				var user = await _userAppService.GetUserById(userId);
				var customerProfiles = new List<CustomerProfiles.Dto.CustomerProfileDto>();
				
				if (AbpSession.UserId != null)
				{
					customerProfiles = await _customerProfileAppService.GetAllByCurrentUser();
				}

				var model = new ProfileUser
				{
					User = user,
					CustomerProfiles = customerProfiles
				};
				return PartialView("_UserInfos", model);
			}
				return PartialView("_OrderList");
		}

		// ========== CustomerProfile Actions ==========
		private string UploadAvatar(IFormFile avatarFile)
		{
			if (avatarFile != null && avatarFile.Length > 0)
			{
				string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
				string fileExtension = Path.GetExtension(avatarFile.FileName).ToLower();
				if (!allowedExtensions.Contains(fileExtension))
				{
					throw new ArgumentException("Định dạng ảnh không hợp lệ. Vui lòng chọn ảnh có định dạng .jpg, .jpeg, .png hoặc .gif");
				}

				if (avatarFile.Length > 5 * 1024 * 1024)
				{
					throw new ArgumentException("Kích thước ảnh không được vượt quá 5MB");
				}

				string uploadsFolder = @"E:\Uploads\avatars\";
				Directory.CreateDirectory(uploadsFolder);

				string uniqueFileName = DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + Guid.NewGuid().ToString("N") + fileExtension;
				string filePath = Path.Combine(uploadsFolder, uniqueFileName);

				using (var fileStream = new FileStream(filePath, FileMode.Create))
				{
					avatarFile.CopyTo(fileStream);
				}

				return "/avatars/" + uniqueFileName;
			}
			return null;
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

		[HttpPost]
		public async Task<IActionResult> CreateCustomerProfile(CreateCustomerProfileDto input)
		{
			try
			{
				if (input.AvatarFile != null)
				{
					input.Avatar = UploadAvatar(input.AvatarFile);
				}
				await _customerProfileAppService.Create(input);
				return Json(new { success = true, message = "Thêm thông tin thành công" });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = ex.Message });
			}
		}

		[HttpPost]
		public async Task<IActionResult> UpdateCustomerProfile(UpdateCustomerProfileDto input)
		{
			try
			{
				var existingProfile = await _customerProfileAppService.GetById(input.Id);
				
				if (input.AvatarFile != null && input.AvatarFile.Length > 0)
				{
					if (!string.IsNullOrEmpty(existingProfile.Avatar))
					{
						DeleteAvatarFile(existingProfile.Avatar);
					}
					input.Avatar = UploadAvatar(input.AvatarFile);
				}
				else
				{
					input.Avatar = existingProfile.Avatar;
				}

				await _customerProfileAppService.Update(input);
				return Json(new { success = true, message = "Cập nhật thông tin thành công" });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = ex.Message });
			}
		}

		[HttpPost]
		public async Task<IActionResult> DeleteCustomerProfile(int id)
		{
			try
			{
				var profile = await _customerProfileAppService.GetById(id);
				if (!string.IsNullOrEmpty(profile.Avatar))
				{
					DeleteAvatarFile(profile.Avatar);
				}
				await _customerProfileAppService.Delete(id);
				return Json(new { success = true, message = "Xóa thông tin thành công" });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = ex.Message });
			}
		}

		[HttpPost]
		public async Task<IActionResult> SetDefaultCustomerProfile(int id)
		{
			try
			{
				await _customerProfileAppService.SetAsDefault(id);
				return Json(new { success = true, message = "Đặt làm mặc định thành công" });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = ex.Message });
			}
		}

		public async Task<IActionResult> GetCustomerProfileForm(int? id)
		{
			if (id.HasValue && id.Value > 0)
			{
				var profile = await _customerProfileAppService.GetById(id.Value);
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
				return PartialView("_CustomerProfileForm", updateDto);
			}
			return PartialView("_CustomerProfileForm", new CreateCustomerProfileDto());
		}

		public async Task<IActionResult> GetInforDetailOrder(int orderId)
		{
			var order = await _orderAppService.GetOrderById(orderId);
			var orderDetail = await _orderDetailAppService.GetOrderListById(orderId);

			var productIds = orderDetail.Select(od => od.ProductId).ToList();
			var products = await _productAppService.GetProductByIds(productIds);

			// Lấy thông tin CustomerProfile từ UserId của đơn hàng
			CustomerProfiles.Dto.CustomerProfileDto customerProfile = null;
			if (order.UserId > 0)
			{
				var profiles = await _customerProfileAppService.GetAllByCurrentUser();
				// Lấy profile mặc định hoặc profile đầu tiên
				customerProfile = profiles.FirstOrDefault(p => p.IsDefault) ?? profiles.FirstOrDefault();
			}

			var model = new OrderViewSuccess
			{
				Order = order,
				OrderListDetail = orderDetail,
				ProductList = products,
				CustomerProfile = customerProfile
			};

			return PartialView("GetInforDetailOrder", model);
		}
	}
}