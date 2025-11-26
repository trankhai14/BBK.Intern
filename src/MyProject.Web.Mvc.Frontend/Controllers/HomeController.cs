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
using MyProject.FlashSales;
using MyProject.FlashSales.Dto;
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
		private readonly IFlashSaleAppService _flashSaleAppService;

		
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
			ICustomerProfileAppService customerProfileAppService,
			IFlashSaleAppService flashSaleAppService
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
			_flashSaleAppService = flashSaleAppService;
		}

		public async Task<IActionResult> Index(int page = 1, int pageSize = 5)
		{
			// Lấy danh sách sản phẩm theo phân trang (chỉ lấy sản phẩm có tồn kho)
			var productsResult = await _productAppService.GetAll(new GetAllProductsInput
			{
				MaxResultCount = 20,
				SkipCount = 0,
				Sorting = "CreationTime DESC",
				OnlyWithInventory = true // Chỉ lấy sản phẩm có tồn kho (cho frontend)
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
						CategoryId = item.Id,
						OnlyWithInventory = true // Chỉ lấy sản phẩm có tồn kho (cho frontend)
					});

					categoryProductViewModels.Add(new CategoryProductViewModel
					{
						CategoryId = item.Id,
						CategoryName = item.CategoryName,
						Products = productsOfCategory.Items.Take(10).ToList()
					});
				}
			}

			// Lấy FlashSale đang diễn ra
			var ongoingFlashSales = await _flashSaleAppService.GetOngoingFlashSales();

			var homePageViewModel = new HomePageViewModel
			{
				ProductData = productViewModel,
				CategoryProducts = categoryProductViewModels,
				FlashSales = ongoingFlashSales ?? new List<MyProject.FlashSales.Dto.FlashSaleDto>()
			};

			return View(homePageViewModel);
		}



		public async Task<IActionResult> SearchProductsWeb(GetAllProductWeb input)
		{
			//ProductViewModel webViewModel = new ProductViewModel();
			var result = await _productAppService.Search(new GetAllProductsInput
			{
				Keyword = input.Keyword,
				OnlyWithInventory = true // Chỉ hiển thị sản phẩm có trong kho khi tìm kiếm
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
				SkipCount = 0,
				OnlyWithInventory = true // Chỉ lấy sản phẩm có tồn kho (cho frontend)
			});

			var relatedProducts = relatedProductsResult.Items
				.Where(p => p.Id != product.Id)
				.Take(5)
				.ToList();

			// Kiểm tra sản phẩm có trong FlashSale đang diễn ra không
			FlashSaleProductDto flashSaleProduct = null;
			try
			{
				flashSaleProduct = await _flashSaleAppService.GetFlashSaleProductByProductId(product.Id);
			}
			catch
			{
				// Nếu không có FlashSale, bỏ qua
			}

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
				// Thông tin kỹ thuật điện thoại
				//Sku = product.Sku,
				//ModelNumber = product.ModelNumber,
				//Chipset = product.Chipset,
				//Ram = product.Ram,
				//Storage = product.Storage,
				//Screen = product.Screen,
				//OperatingSystem = product.OperatingSystem,
				//Battery = product.Battery,
				//Camera = product.Camera,
				//FrontCamera = product.FrontCamera,
				//Sim = product.Sim,
				//Connectivity = product.Connectivity,
				//Security = product.Security,
				//Charging = product.Charging,
				//ChargingPort = product.ChargingPort,
				//Color = product.Color,
				//Warranty = product.Warranty,
				//TechnicalSpecifications = product.TechnicalSpecifications,
				RelatedProducts = relatedProducts,
				FlashSaleProduct = flashSaleProduct
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

		public async Task<IActionResult> FilterStatus(int? orderStatus = 5, int page = 1, int pageSize = 10)
		{
			// Lấy danh sách đơn hàng với phân trang
			var orderPagedResult = await _orderAppService.GetStatusOrderPaged(orderStatus, page, pageSize);
			var orderOutputs = orderPagedResult.Items;
			var orderIds = orderOutputs.Select(o => o.OrderId).ToList(); // Lấy danh sách ID
			
			// Nếu không có đơn hàng, trả về view rỗng
			if (!orderIds.Any())
			{
				var emptyModel = new FilterStatusOrderViewModel
				{
					ListOrder = new List<OrderDetails.Dto.OrderDetailDto>(),
					OrderStatus = orderStatus,
					Products = new List<Product.Dtos.ProductListDto>(),
					CurrentPage = page,
					PageSize = pageSize,
					TotalCount = 0
				};
				return PartialView("FilterOrder", emptyModel);
			}

			var orderList = await _orderDetailAppService.GetOrderByIdAndStatus(orderIds);
			var orderStatusDict = orderOutputs.ToDictionary(o => o.OrderId, o => o.OrderStatus);
			// Duy trì thứ tự đơn hàng theo kết quả phân trang (mới -> cũ)
			var orderIndexMap = orderOutputs
				.Select((o, index) => new { o.OrderId, index })
				.ToDictionary(x => x.OrderId, x => x.index);

			// Gán OrderStatus cho từng đơn hàng trong orderList
			foreach (var order in orderList)
			{
				if (orderStatusDict.TryGetValue(order.OrderId, out var status))
				{
					order.OrderStatus = status;
				}
			}
			// Sắp xếp lại danh sách chi tiết theo thứ tự đơn hàng mới nhất đến cũ nhất
			orderList = orderList
				.OrderBy(od => orderIndexMap.TryGetValue(od.OrderId, out var idx) ? idx : int.MaxValue)
				.ToList();

			// Lấy danh sách ID sản phẩm cần lấy thông tin
			var productIds = orderList.Select(o => o.ProductId).Distinct().ToList();
			var productList = await _productAppService.GetProductByIds(productIds);
			var model = new FilterStatusOrderViewModel
			{
				ListOrder = orderList,
				OrderStatus = orderStatus,
				Products = productList,
				CurrentPage = page,
				PageSize = pageSize,
				TotalCount = orderPagedResult.TotalCount
			};
			return PartialView("FilterOrder", model);
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