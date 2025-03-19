using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Microsoft.EntityFrameworkCore;
using MyProject.Categories.Dto;

namespace MyProject.Categories
{
	public interface ICategoryFontendAppService
	{
		Task<PagedResultDto<CategoryListDto>> GetCategory(GetAllCategoriesInput input);
	}

	public class CategoryFontendAppService : MyProjectAppServiceBase, ICategoryFontendAppService
	{
		private readonly IRepository<Category> _categoryRepository;
		private readonly IRepository<MyProject.Products.Product> _productRepository;

		public CategoryFontendAppService(IRepository<Category> categoryRepository, IRepository<Products.Product> productRepository)
		{
			_categoryRepository = categoryRepository;
			_productRepository = productRepository;
		}

		public async Task<PagedResultDto<CategoryListDto>> GetCategory(GetAllCategoriesInput input)
		{
			using var uow = UnitOfWorkManager.Begin();
			using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
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
					return null;
				}
				finally
				{
					await uow.CompleteAsync();
				}

			}
		}

	}
}
