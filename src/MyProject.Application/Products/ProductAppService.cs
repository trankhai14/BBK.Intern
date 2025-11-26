using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Timing;
using Microsoft.EntityFrameworkCore;
using MyProject.Authorization;
using MyProject.Categories;
using MyProject.Categories.Dto;
using MyProject.Inventories;
using MyProject.Product.Dtos;
using MyProject.Products.Dtos;
using static MyProject.Products.Product;


namespace MyProject.Products
{
	//[AbpAuthorize(PermissionNames.Pages_Products)]
	public class ProductAppService : ApplicationService, IProductAppService
	{
		private readonly IRepository<Product> _productRepository;
		private readonly IRepository<Category> _categoryRepository;
		private readonly IRepository<Inventory> _inventoryRepository;
		private readonly IRepository<ProductSpecification> _specificationRepository;


		public ProductAppService(
			IRepository<Product> productRepository, 
			IRepository<Category> categoryRepository, 
			IRepository<Inventory> inventoryRepository,
			IRepository<ProductSpecification> specificationRepository)
		{
			_productRepository = productRepository;
			_categoryRepository = categoryRepository;
			_inventoryRepository = inventoryRepository;
			_specificationRepository = specificationRepository;
		}

		public async Task<bool> CheckProductsByCategoryId(int categoryId)
		{
			return await _productRepository.GetAll().AnyAsync(p => p.CategoryId == categoryId);
		}

		public async Task<List<ProductListDto>> GetAllProducts()
		{
			var products = await _productRepository.GetAll()
				.Include(p => p.Specification)
				.ToListAsync();

			return products.Select(p => new ProductListDto
			{
				Id = p.Id,
				Name = p.Name,
				Description = p.Description,
				Price = p.Price,
				State = p.State,
				CreationTime = p.CreationTime,
				Image = p.Image,
				Brand = p.Brand,
				WeightInGrams = p.WeightInGrams,
				WidthCm = p.WidthCm,
				HeightCm = p.HeightCm,
				LengthCm = p.LengthCm,
				CategoryId = p.CategoryId,
				SupplierId = p.SupplierId,
				// Thông tin kỹ thuật (từ Specification)
				Specification = p.Specification != null ? new ProductSpecificationDto
				{
					Id = p.Specification.Id,
					ProductId = p.Specification.ProductId,
					Sku = p.Specification.Sku,
					ModelNumber = p.Specification.ModelNumber,
					Chipset = p.Specification.Chipset,
					Ram = p.Specification.Ram,
					Storage = p.Specification.Storage,
					Screen = p.Specification.Screen,
					OperatingSystem = p.Specification.OperatingSystem,
					Battery = p.Specification.Battery,
					Camera = p.Specification.Camera,
					FrontCamera = p.Specification.FrontCamera,
					Sim = p.Specification.Sim,
					Connectivity = p.Specification.Connectivity,
					Security = p.Specification.Security,
					Charging = p.Specification.Charging,
					ChargingPort = p.Specification.ChargingPort,
					Color = p.Specification.Color,
					Warranty = p.Specification.Warranty,
					TechnicalSpecifications = p.Specification.TechnicalSpecifications,
					CreationTime = p.Specification.CreationTime,
					LastModificationTime = p.Specification.LastModificationTime
				} : null
			}).ToList();
		}


		public async Task<PagedResultDto<ProductListDto>> GetAll(GetAllProductsInput input)
		{
			var products = _productRepository.GetAll();

			// Nếu OnlyWithInventory = true, chỉ lấy sản phẩm có tồn kho (cho frontend)
			// Nếu false (mặc định), lấy tất cả sản phẩm (cho admin)
			if (input.OnlyWithInventory)
			{
				// Lấy danh sách ProductId có tồn kho (có record trong Inventory)
				var productsWithInventory = await _inventoryRepository.GetAll()
					.Select(i => i.ProductId)
					.Distinct()
					.ToListAsync();

				// Chỉ lấy sản phẩm có tồn kho
				products = products.Where(p => productsWithInventory.Contains(p.Id));
			}

			var Count = await products.CountAsync();

			input.Sorting = "CreationTime DESC"; // Sắp xếp theo thời gian tạo mới nhất dùng sorting của PagedAndSortedResultRequestDto

			var productDtos = products
				.Include(p => p.Specification)
				.PageBy(input)
				.Select(p => new ProductListDto
				{
					Id = p.Id,
					Name = p.Name,
					Description = p.Description,
					Price = p.Price,
					State = p.State,
					CreationTime = p.CreationTime,
					Image = p.Image,
					Brand = p.Brand,
					WeightInGrams = p.WeightInGrams,
					WidthCm = p.WidthCm,
					HeightCm = p.HeightCm,
					LengthCm = p.LengthCm,
					CategoryId = p.CategoryId,
					SupplierId = p.SupplierId,
					// Thông tin kỹ thuật (chỉ load khi cần - có thể null)
					Specification = p.Specification != null ? new ProductSpecificationDto
					{
						Id = p.Specification.Id,
						ProductId = p.Specification.ProductId,
						Sku = p.Specification.Sku,
						ModelNumber = p.Specification.ModelNumber,
						Chipset = p.Specification.Chipset,
						Ram = p.Specification.Ram,
						Storage = p.Specification.Storage,
						Color = p.Specification.Color
					} : null
				}).ToList();

			return new PagedResultDto<ProductListDto>(Count, productDtos);
		}


		// lấy sản phẩm để hiển thị ra trang chủ theo từng danh mục
		public async Task<List<ProductListDto>> GetAllProduct()
		{
			// Lấy danh sách ProductId có tồn kho (có record trong Inventory)
			var productsWithInventory = await _inventoryRepository.GetAll()
				.Select(i => i.ProductId)
				.Distinct()
				.ToListAsync();

			// Chỉ lấy sản phẩm có tồn kho
			var products = await _productRepository.GetAll()
				.Where(p => productsWithInventory.Contains(p.Id))
				.ToListAsync();

			return products.Select(product => new ProductListDto
			{
				Id = product.Id,
				Name = product.Name,
				Description = product.Description,
				Price = product.Price,
				State = product.State,
				Image = product.Image,
				CreationTime = product.CreationTime,
				CategoryId = product.CategoryId
			}).ToList();
		}

		/// <summary>
		/// Lấy danh sách tất cả danh mục từ cơ sở dữ liệu.
		/// </summary>
		/// <returns>Danh sách các danh mục dưới dạng DTO.</returns>
		/// //lấy danh mục để hiển thị vào droplist => tạo product
		public async Task<List<CategoryListDto>> GetAllCategory()
		{
			// Lấy toàn bộ danh mục từ cơ sở dữ liệu
			var categories = await _categoryRepository.GetAll().ToListAsync();

			// Chuyển đổi danh sách category sang DTO để trả về
			return categories.Select(category => new CategoryListDto
			{
				Id = category.Id,
				CategoryName = category.CategoryName,
				CategoryDescription = category.CategoryDescription
			}).ToList();
		}


		[AbpAuthorize(PermissionNames.Pages_Products_Edit)]
		public async Task<ProductListDto> Update(UpdateProductDto input)
		{
			// Lấy sản phẩm từ CSDL theo ID (include Specification)
			var product = await _productRepository.GetAll()
				.Include(p => p.Specification)
				.FirstOrDefaultAsync(p => p.Id == input.Id);

			if (product == null)
			{
				throw new Abp.UI.UserFriendlyException("Không tìm thấy sản phẩm");
			}

			// Cập nhật các thuộc tính cơ bản của sản phẩm
			product.Name = input.Name;
			product.Description = input.Description;
			product.Price = input.Price;
			product.State = input.State;
			product.Image = input.Image;
			product.CategoryId = input.CategoryId;
			product.SupplierId = input.SupplierId;
			product.Brand = input.Brand;
			product.WeightInGrams = input.WeightInGrams;
			product.WidthCm = input.WidthCm;
			product.HeightCm = input.HeightCm;
			product.LengthCm = input.LengthCm;

			// Cập nhật hoặc tạo mới thông tin kỹ thuật
			if (input.Specification != null)
			{
				if (product.Specification != null)
				{
					// Cập nhật Specification hiện có
					var spec = product.Specification;
					spec.Sku = input.Specification.Sku;
					spec.ModelNumber = input.Specification.ModelNumber;
					spec.Chipset = input.Specification.Chipset;
					spec.Ram = input.Specification.Ram;
					spec.Storage = input.Specification.Storage;
					spec.Screen = input.Specification.Screen;
					spec.OperatingSystem = input.Specification.OperatingSystem;
					spec.Battery = input.Specification.Battery;
					spec.Camera = input.Specification.Camera;
					spec.FrontCamera = input.Specification.FrontCamera;
					spec.Sim = input.Specification.Sim;
					spec.Connectivity = input.Specification.Connectivity;
					spec.Security = input.Specification.Security;
					spec.Charging = input.Specification.Charging;
					spec.ChargingPort = input.Specification.ChargingPort;
					spec.Color = input.Specification.Color;
					spec.Warranty = input.Specification.Warranty;
					spec.TechnicalSpecifications = input.Specification.TechnicalSpecifications;
					spec.LastModificationTime = Clock.Now;
					await _specificationRepository.UpdateAsync(spec);
				}
				else
				{
					// Tạo mới Specification
					var spec = new ProductSpecification
					{
						ProductId = product.Id,
						Sku = input.Specification.Sku,
						ModelNumber = input.Specification.ModelNumber,
						Chipset = input.Specification.Chipset,
						Ram = input.Specification.Ram,
						Storage = input.Specification.Storage,
						Screen = input.Specification.Screen,
						OperatingSystem = input.Specification.OperatingSystem,
						Battery = input.Specification.Battery,
						Camera = input.Specification.Camera,
						FrontCamera = input.Specification.FrontCamera,
						Sim = input.Specification.Sim,
						Connectivity = input.Specification.Connectivity,
						Security = input.Specification.Security,
						Charging = input.Specification.Charging,
						ChargingPort = input.Specification.ChargingPort,
						Color = input.Specification.Color,
						Warranty = input.Specification.Warranty,
						TechnicalSpecifications = input.Specification.TechnicalSpecifications,
						CreationTime = Clock.Now
					};
					await _specificationRepository.InsertAsync(spec);
				}
			}

			// Lưu thay đổi vào CSDL
			await _productRepository.UpdateAsync(product);
			await CurrentUnitOfWork.SaveChangesAsync();

			// Reload để lấy Specification mới nhất
			product = await _productRepository.GetAll()
				.Include(p => p.Specification)
				.FirstOrDefaultAsync(p => p.Id == product.Id);

			return MapToProductListDto(product);
		}

		[AbpAuthorize(PermissionNames.Pages_Products_Create)]
		public async Task<ProductListDto> Create(CreateProductDto input)
		{
			// Tạo mới sản phẩm với thông tin cơ bản
			var product = new Product
			{
				Name = input.Name,
				Description = input.Description,
				Price = input.Price,
				State = input.State,
				CategoryId = input.CategoryId,
				SupplierId = input.SupplierId,
				Image = input.Image,
				Brand = input.Brand,
				WeightInGrams = input.WeightInGrams,
				WidthCm = input.WidthCm,
				HeightCm = input.HeightCm,
				LengthCm = input.LengthCm
			};

			await _productRepository.InsertAsync(product);
			await CurrentUnitOfWork.SaveChangesAsync();

			// Tạo thông tin kỹ thuật nếu có
			if (input.Specification != null)
			{
				var specification = new ProductSpecification
				{
					ProductId = product.Id,
					Sku = input.Specification.Sku,
					ModelNumber = input.Specification.ModelNumber,
					Chipset = input.Specification.Chipset,
					Ram = input.Specification.Ram,
					Storage = input.Specification.Storage,
					Screen = input.Specification.Screen,
					OperatingSystem = input.Specification.OperatingSystem,
					Battery = input.Specification.Battery,
					Camera = input.Specification.Camera,
					FrontCamera = input.Specification.FrontCamera,
					Sim = input.Specification.Sim,
					Connectivity = input.Specification.Connectivity,
					Security = input.Specification.Security,
					Charging = input.Specification.Charging,
					ChargingPort = input.Specification.ChargingPort,
					Color = input.Specification.Color,
					Warranty = input.Specification.Warranty,
					TechnicalSpecifications = input.Specification.TechnicalSpecifications,
					CreationTime = DateTime.Now
				};

				await _specificationRepository.InsertAsync(specification);
				await CurrentUnitOfWork.SaveChangesAsync();
			}

			// Reload để lấy Specification
			product = await _productRepository.GetAll()
				.Include(p => p.Specification)
				.FirstOrDefaultAsync(p => p.Id == product.Id);

			return MapToProductListDto(product);
		}

		public async Task<ProductListDto> GetAsync(EntityDto<int> input)
		{
			var product = await _productRepository.GetAll()
				.Include(p => p.Specification)
				.FirstOrDefaultAsync(p => p.Id == input.Id);

			if (product == null)
			{
				throw new Abp.UI.UserFriendlyException("Không tìm thấy sản phẩm");
			}

			return MapToProductListDto(product);
		}

		[AbpAuthorize(PermissionNames.Pages_Products_Delete)]
		public async Task Delete(EntityDto<int> input)
		{
			var product = await _productRepository.GetAll()
				.Include(p => p.Specification)
				.FirstOrDefaultAsync(x => x.Id == input.Id);

			if (product == null)
			{
				throw new Abp.UI.UserFriendlyException("Không tìm thấy sản phẩm");
			}

			// Xóa Specification trước (nếu có)
			// Kiểm tra cả từ product.Specification và từ repository để đảm bảo xóa được
			if (product.Specification != null)
			{
				await _specificationRepository.DeleteAsync(product.Specification);
			}
			else
			{
				// Nếu Include không load được, tìm trực tiếp từ repository
				var specification = await _specificationRepository.FirstOrDefaultAsync(s => s.ProductId == input.Id);
				if (specification != null)
				{
					await _specificationRepository.DeleteAsync(specification);
				}
			}

			// Xóa Product (sau khi đã xóa Specification)
			await _productRepository.DeleteAsync(product);
			await CurrentUnitOfWork.SaveChangesAsync();
		}


		public async Task<ProductDetailDto> Detail(EntityDto<int> input)
		{
			var product = await _productRepository.GetAll()
				.Include(p => p.Specification)
				.Include(p => p.Category)
				.Include(p => p.Supplier)
				.FirstOrDefaultAsync(x => x.Id == input.Id);

			if (product == null)
			{
				return null;
			}

			return new ProductDetailDto
			{
				Id = product.Id,
				Name = product.Name,
				Description = product.Description,
				Price = product.Price,
				State = product.State,
				CreationTime = product.CreationTime,
				Image = product.Image,
				Brand = product.Brand,
				CategoryId = product.CategoryId,
				CategoryName = product.Category?.CategoryName,
				SupplierId = product.SupplierId,
				SupplierName = product.Supplier?.Name,
				// Thông tin kích thước và trọng lượng
				WeightInGrams = product.WeightInGrams,
				WidthCm = product.WidthCm,
				HeightCm = product.HeightCm,
				LengthCm = product.LengthCm,
				// Thông tin kỹ thuật (từ Specification)
				Specification = product.Specification != null ? new ProductSpecificationDto
				{
					Id = product.Specification.Id,
					ProductId = product.Specification.ProductId,
					Sku = product.Specification.Sku,
					ModelNumber = product.Specification.ModelNumber,
					Chipset = product.Specification.Chipset,
					Ram = product.Specification.Ram,
					Storage = product.Specification.Storage,
					Screen = product.Specification.Screen,
					OperatingSystem = product.Specification.OperatingSystem,
					Battery = product.Specification.Battery,
					Camera = product.Specification.Camera,
					FrontCamera = product.Specification.FrontCamera,
					Sim = product.Specification.Sim,
					Connectivity = product.Specification.Connectivity,
					Security = product.Specification.Security,
					Charging = product.Specification.Charging,
					ChargingPort = product.Specification.ChargingPort,
					Color = product.Specification.Color,
					Warranty = product.Specification.Warranty,
					TechnicalSpecifications = product.Specification.TechnicalSpecifications,
					CreationTime = product.Specification.CreationTime,
					LastModificationTime = product.Specification.LastModificationTime
				} : null
			};
		}

		public async Task<List<ProductListDto>> GetProductByIds(List<int> productIds)
		{
			return await _productRepository.GetAll()
				.Where(p => productIds.Contains(p.Id))
				.Select(p => new ProductListDto
				{
					Id = p.Id,
					Name = p.Name,
					Description = p.Description,
					Price = p.Price,
					Image = p.Image
				})
				.ToListAsync();
		}

		public async Task<PagedResultDto<ProductListDto>> Search(GetAllProductsInput input)
		{
			var productQuery = _productRepository.GetAll();

			// Nếu OnlyWithInventory = true, chỉ lấy sản phẩm có tồn kho (cho frontend)
			// Nếu false (mặc định), lấy tất cả sản phẩm (cho admin)
			if (input.OnlyWithInventory)
			{
				// Lấy danh sách ProductId có tồn kho (có record trong Inventory)
				var productsWithInventory = await _inventoryRepository.GetAll()
					.Select(i => i.ProductId)
					.Distinct()
					.ToListAsync();

				// Chỉ lấy sản phẩm có tồn kho
				productQuery = productQuery.Where(p => productsWithInventory.Contains(p.Id));
			}

			if (input.CategoryId.HasValue && input.CategoryId > 0)
			{
				productQuery = productQuery.Where(x => x.CategoryId == input.CategoryId.Value);
			}


			if (!string.IsNullOrWhiteSpace(input.Keyword))
			{
				string keywordLower = input.Keyword.ToLower();
				productQuery = productQuery.Where(x => x.Name.ToLower().Contains(keywordLower));
			}

			if (!string.IsNullOrWhiteSpace(input.CategoryInput))
			{
				int categoryId = Convert.ToInt32(input.CategoryInput);
				productQuery = productQuery.Where(x => x.CategoryId == categoryId);
			}

			if (!string.IsNullOrWhiteSpace(input.StateInput) && Enum.TryParse<ProductState>(input.StateInput, out var state))
			{
				productQuery = productQuery.Where(x => x.State == state);
			}

			// Lọc theo SupplierId nếu có
			if (input.SupplierId.HasValue && input.SupplierId > 0)
			{
				productQuery = productQuery.Where(x => x.SupplierId == input.SupplierId.Value);
			}

			var Count = await productQuery.CountAsync();

			//input.Sorting = "CreationTime DESC"; // Sắp xếp theo thời gian tạo mới nhất dùng sorting của PagedAndSortedResultRequestDto

			var productDtos = productQuery.OrderByDescending(x => x.CreationTime).PageBy(input).Select(products => new ProductListDto
			{
				Id = products.Id,
				Name = products.Name,
				Description = products.Description,
				Price = products.Price,
				State = products.State,
				Image = products.Image,
				Brand = products.Brand,
				WeightInGrams = products.WeightInGrams,
				WidthCm = products.WidthCm,
				HeightCm = products.HeightCm,
				LengthCm = products.LengthCm,
				CreationTime = products.CreationTime,
				CategoryId = products.CategoryId,
			}).ToList();

			return new PagedResultDto<ProductListDto>(Count, productDtos);
		}

		/// <summary>
		/// Helper method để map Product sang ProductListDto
		/// </summary>
		private ProductListDto MapToProductListDto(Product product)
		{
			return new ProductListDto
			{
				Id = product.Id,
				Name = product.Name,
				Description = product.Description,
				Price = product.Price,
				State = product.State,
				CreationTime = product.CreationTime,
				Image = product.Image,
				Brand = product.Brand,
				WeightInGrams = product.WeightInGrams,
				WidthCm = product.WidthCm,
				HeightCm = product.HeightCm,
				LengthCm = product.LengthCm,
				CategoryId = product.CategoryId,
				SupplierId = product.SupplierId,
				// Thông tin kỹ thuật (từ Specification)
				Specification = product.Specification != null ? new ProductSpecificationDto
				{
					Id = product.Specification.Id,
					ProductId = product.Specification.ProductId,
					Sku = product.Specification.Sku,
					ModelNumber = product.Specification.ModelNumber,
					Chipset = product.Specification.Chipset,
					Ram = product.Specification.Ram,
					Storage = product.Specification.Storage,
					Screen = product.Specification.Screen,
					OperatingSystem = product.Specification.OperatingSystem,
					Battery = product.Specification.Battery,
					Camera = product.Specification.Camera,
					FrontCamera = product.Specification.FrontCamera,
					Sim = product.Specification.Sim,
					Connectivity = product.Specification.Connectivity,
					Security = product.Specification.Security,
					Charging = product.Specification.Charging,
					ChargingPort = product.Specification.ChargingPort,
					Color = product.Specification.Color,
					Warranty = product.Specification.Warranty,
					TechnicalSpecifications = product.Specification.TechnicalSpecifications,
					CreationTime = product.Specification.CreationTime,
					LastModificationTime = product.Specification.LastModificationTime
				} : null
			};
		}
	}
}
