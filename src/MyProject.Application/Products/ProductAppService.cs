using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using MyProject.Product;
using MyProject.Product.Dtos;
using MyProject.Products;
using MyProject.Products.Dtos;
using MyProject.TaskAppService.Dto;
using Microsoft.EntityFrameworkCore;
using Abp.Linq.Extensions;
using MyProject.Categories;
using MyProject.Categories.Dto;
using Microsoft.AspNetCore.Http;
using Abp.UI;
using System.IO;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Abp.Authorization;
using MyProject.Authorization;
using static MyProject.Products.Product;
using MyProject.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using Abp.Application.Services;


namespace MyProject.Products
{
	//[AbpAuthorize(PermissionNames.Pages_Products)]

	public class ProductAppService : ApplicationService, IProductAppService
	{
		private readonly IRepository<Product> _productRepository;
		private readonly IRepository<Category> _categoryRepository;


		public ProductAppService(IRepository<Product> productRepository, IRepository<Category> categoryRepository)
		{
			_productRepository = productRepository;
			_categoryRepository = categoryRepository;
		}

		public async Task<bool> CheckProductsByCategoryId(int categoryId)
		{
			return await _productRepository.GetAll().AnyAsync(p => p.CategoryId == categoryId);
		}

		public async Task<List<ProductListDto>> GetAllProducts()
		{
			var products = await _productRepository.GetAllListAsync();

			return products.Select(p => new ProductListDto
			{
				Id = p.Id,
				Name = p.Name,
				Description = p.Description,
				Price = p.Price,
				State = p.State,
				CreationTime = p.CreationTime,
				Image = p.Image,
				CategoryId = p.CategoryId
			}).ToList();
		}


		public async Task<PagedResultDto<ProductListDto>> GetAll(GetAllProductsInput input)
		{
			var products = _productRepository.GetAll();

			var Count = await products.CountAsync();
			
			input.Sorting = "CreationTime DESC"; // Sắp xếp theo thời gian tạo mới nhất dùng sorting của PagedAndSortedResultRequestDto

			var productDtos = products.PageBy(input).Select(p => new ProductListDto
			{
				Id = p.Id,
				Name = p.Name,
				Description = p.Description,
				Price = p.Price,
				State = p.State,
				CreationTime = p.CreationTime,
				Image = p.Image,
			}).ToList();

			return new PagedResultDto<ProductListDto>(Count, productDtos);
		}


		// lấy sản phẩm để hiển thị ra trang chủ theo từng danh mục
		public async Task<List<ProductListDto>> GetAllProduct()
		{
			var products = await _productRepository.GetAllListAsync();
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


		public async Task<ProductListDto> Update(UpdateProductDto input)
		{
			// Lấy sản phẩm từ CSDL theo ID
			var product = await _productRepository.GetAsync(input.Id);

			// Cập nhật các thuộc tính của sản phẩm
			product.Name = input.Name;
			product.Description = input.Description;
			product.Price = input.Price;
			product.State = input.State;
			product.Image = input.Image;
			product.CategoryId = input.CategoryId;

			// Lưu thay đổi vào CSDL``
			await _productRepository.UpdateAsync(product);
			await CurrentUnitOfWork.SaveChangesAsync();

			return new ProductListDto
			{
				Id = product.Id,
				Name = product.Name,
				Description = product.Description,
				Price = product.Price,
				State = product.State,
				Image = product.Image,
				CategoryId = product.CategoryId,
			};
		}

		public async Task<ProductListDto> Create(CreateProductDto input)
		{

			var product = new Product
			{
				Name = input.Name,
				Description = input.Description,
				Price = input.Price,
				State = input.State,
				CategoryId = input.CategoryId,
				Image = input.Image
			};

			await _productRepository.InsertAsync(product);
			await CurrentUnitOfWork.SaveChangesAsync();

			return new ProductListDto
			{
				Id = product.Id,
				Name = product.Name,
				Description = product.Description,
				Price = product.Price,
				State = product.State,
				Image = product.Image
			};
		}

		public async Task<ProductListDto> GetAsync(EntityDto<int> input)
		{
			var product = await _productRepository.GetAsync(input.Id);

			return new ProductListDto
			{
				Id = product.Id,
				Name = product.Name,
				Description = product.Description,
				Price = product.Price,
				State = product.State,
				CreationTime = product.CreationTime, 
				Image = product.Image,
				CategoryId = product.CategoryId
			};
		}

		public async Task Delete(EntityDto<int> input)
		{
			var product = await _productRepository.FirstOrDefaultAsync(x => x.Id == input.Id);
			await _productRepository.DeleteAsync(product);
		}


		public async Task<ProductDetailDto> Detail(EntityDto<int> input)
		{
			var product = await _productRepository.FirstOrDefaultAsync(x => x.Id == input.Id);

			return new ProductDetailDto
			{
				Id = product.Id,
				Name = product.Name,
				Description = product.Description,
				Price = product.Price,
				State = product.State,
				CreationTime = product.CreationTime
			};
		}

		public async Task<PagedResultDto<ProductListDto>> Search(GetAllProductsInput input)
		{
			var productQuery = _productRepository.GetAll();


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
				CreationTime = products.CreationTime,
			}).ToList();

			return new PagedResultDto<ProductListDto>(Count, productDtos);
		}

		// join (left join , ...)

	
	}
}
