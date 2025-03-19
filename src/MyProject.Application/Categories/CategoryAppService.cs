using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Microsoft.EntityFrameworkCore;
using MyProject.Categories.Dto;
using MyProject.Categories;
using Abp.UI;
using Abp.Authorization;
using MyProject.Authorization;

namespace MyProject.Categories
{
	//[AbpAuthorize(PermissionNames.Pages_Categories)]
	public class CategoryAppService : MyProjectAppServiceBase, ICategoryAppService
	{
		private readonly IRepository<Category> _categoryRepository;
		private readonly IRepository<MyProject.Products.Product> _productRepository;

		public CategoryAppService(IRepository<Category> categoryRepository, IRepository<Products.Product> productRepository)
		{
			_categoryRepository = categoryRepository;
			_productRepository = productRepository;
		}
		public async Task<PagedResultDto<CategoryListDto>> GetCategory(GetAllCategoriesInput input)
		{
			try
			{
				var categories = _categoryRepository.GetAll();


				// Tính tổng số lượng sản phẩm phù hợp với điều kiện tìm kiếm
				var totalCount = await categories.CountAsync();

				// Lấy danh sách các sản phẩm phù hợp, phân trang và chuyển đổi thành DTO
				var categoryDtos = await categories.OrderBy(x => x.CategoryName)  // 
																						 .PageBy(input)               // Phân trang theo input
																						 .Select(p => new CategoryListDto
																						 {
																							 Id = p.Id,
																							 CategoryName = p.CategoryName,
																							 CategoryDescription = p.CategoryDescription
																						 }).ToListAsync();  // Thực thi truy vấn và chuyển thành danh sách DTO

				// Trả về kết quả phân trang
				return new PagedResultDto<CategoryListDto>(totalCount, categoryDtos);
			}
			catch (Exception ex)
			{

			}

			return null;
		}

		public async Task<CategoryListDto> CreateCategory(CreateCategoryDto input)
		{
			var category = new Category
			{
				Id = input.Id,
				CategoryName = input.CategoryName,
				CategoryDescription = input.CategoryDescription
			};

			await _categoryRepository.InsertAsync(category);
			await CurrentUnitOfWork.SaveChangesAsync();

			return new CategoryListDto
			{
				Id = input.Id,
				CategoryName = category.CategoryName,
				CategoryDescription = category.CategoryDescription
			};
		}


		/// <summary>
		/// Lấy thông tin chi tiết của một danh mục dựa trên ID truyền vào.
		/// </summary>
		/// <param name="input">Đối tượng chứa ID của danh mục cần lấy.</param>
		/// <returns>
		/// Trả về một đối tượng `CategoryListDto` chứa thông tin của danh mục,
		/// bao gồm ID, tên danh mục và mô tả danh mục.
		/// </returns>
		public async Task<CategoryListDto> GetEdit(EntityDto<int> input)
		{
			// Tìm danh mục trong cơ sở dữ liệu theo ID được truyền vào
			var category = await _categoryRepository.GetAsync(input.Id);

			// Trả về dữ liệu danh mục dưới dạng DTO (Data Transfer Object)
			return new CategoryListDto
			{
				Id = category.Id,  // Gán ID của danh mục
				CategoryName = category.CategoryName,  // Gán tên danh mục
				CategoryDescription = category.CategoryDescription // Gán mô tả danh mục
			};
		}

		//Lấy dữ liệu từ input để update
		public async Task<CategoryListDto> UpdateCategory(UpdateCategoryDto input)
		{
			var category = await _categoryRepository.GetAsync(input.Id);

			category.CategoryName = input.CategoryName;
			category.CategoryDescription = input.CategoryDescription;

			await _categoryRepository.UpdateAsync(category);
			await CurrentUnitOfWork?.SaveChangesAsync();

			return new CategoryListDto
			{
				Id = category.Id,
				CategoryName = category.CategoryName,
				CategoryDescription = category.CategoryDescription
			};
		}

		/// <summary>
		/// Xóa một danh mục dựa trên ID được truyền vào.
		/// </summary>
		/// <param name="input">Đối tượng chứa ID của danh mục cần xóa.</param>
		/// <returns>
		/// Task bất đồng bộ thực hiện việc xóa danh mục khỏi cơ sở dữ liệu.
		/// </returns>
		public async Task DeleteCategory(EntityDto<int> input)
		{
			// Tìm danh mục trong cơ sở dữ liệu theo ID, nếu không tìm thấy thì trả về null
			var category = await _categoryRepository.FirstOrDefaultAsync(x => x.Id == input.Id);

			// Kiểm tra nếu category không tồn tại
			if (category == null)
			{
				throw new UserFriendlyException("Danh mục không tồn tại hoặc đã bị xóa.");
			}

			var hasProducts = await _productRepository.FirstOrDefaultAsync(x => x.CategoryId == input.Id);

			if (hasProducts != null)
			{
				throw new UserFriendlyException("Không thể xóa danh mục vì vẫn còn sản phẩm thuộc danh mục này.");
			}

			// Xóa danh mục khỏi cơ sở dữ liệu
			await _categoryRepository.DeleteAsync(category);
		}


		public async Task<CategoryListDto> DetailCategory(EntityDto<int> input)
		{
			var category = await _categoryRepository.FirstOrDefaultAsync(x => x.Id == input.Id);

			return new CategoryListDto
			{
				Id = category.Id,
				CategoryName = category.CategoryName,
				CategoryDescription = category.CategoryDescription
			};
		}

		public async Task<CategoryListDto> GetCategoryById (int? categoryId)
		{
			var category =  _categoryRepository.FirstOrDefault(x => x.Id == categoryId);

			if (category == null) { 
				return null;
			}

			return new CategoryListDto
			{
				Id = category.Id,
				CategoryName = category.CategoryName,
				CategoryDescription = category.CategoryDescription
			};
		}

		// Tim kiem danh muc san pham
		public async Task<PagedResultDto<CategoryListDto>> SearchCategory(GetAllCategoriesInput input)
		{
			var categoryQuery = _categoryRepository.GetAll();

			if (!string.IsNullOrEmpty(input.CategoryName))
			{
				string nameLower = input.CategoryName.ToLower();
				categoryQuery = categoryQuery.Where(x => x.CategoryName.ToLower().Contains(nameLower));
			}

			var Count = await _categoryRepository.CountAsync();

			var categoryDtos = categoryQuery.OrderBy(x => x.CategoryName).PageBy(input).Select(Categories => new CategoryListDto
			{
				Id = Categories.Id,
				CategoryName = Categories.CategoryName,
				CategoryDescription = Categories.CategoryDescription,
			}).ToList();

			return new PagedResultDto<CategoryListDto>(Count, categoryDtos);
		}

		public async Task<List<CategoryListDto>> GetAllCategory()
		{
			var category = await _categoryRepository.GetAllListAsync();

			var categoryDtos = category.Select(c => new CategoryListDto
			{
				Id = c.Id,
				CategoryName = c.CategoryName,
				CategoryDescription = c.CategoryDescription,
			}).ToList();

			return categoryDtos;
		}

	}
}
