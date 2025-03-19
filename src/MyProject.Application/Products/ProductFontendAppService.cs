using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Microsoft.EntityFrameworkCore;
using MyProject.Categories;
using MyProject.Product.Dtos;
using MyProject.Products.Dtos;

namespace MyProject.Products
{
	public interface IProductFontendAppService
	{
		Task<PagedResultDto<ProductListDto>> GetAllProduct(GetAllProductsInput input);
	}
	public class ProductFontendAppService : MyProjectAppServiceBase, IProductFontendAppService
	{
		private readonly IRepository<Product> _productRepository;

		public ProductFontendAppService(IRepository<Product> productRepository)
		{
			_productRepository = productRepository;
		}

		public async Task<PagedResultDto<ProductListDto>> GetAllProduct(GetAllProductsInput input)
		{
			using var uow = UnitOfWorkManager.Begin();
			using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
			{
				try
				{
					var products = _productRepository.GetAll();
					// Tính tổng số lượng sản phẩm phù hợp với điều kiện tìm kiếm
					var totalCount = await products.CountAsync();
					// Lấy danh sách các sản phẩm phù hợp, phân trang và chuyển đổi thành DTO
					var productDtos = await products.OrderBy(x => x.Name)  // 
															 .PageBy(input)               // Phân trang theo input
															 .Select(p => new ProductListDto
															 {
																 Id = p.Id,
																 Name = p.Name,
																 Description = p.Description,
																 Price = p.Price,
																 CreationTime = p.CreationTime,
																 State = p.State,
																 Image = p.Image,
																 CategoryId = p.CategoryId,
															 }).ToListAsync();  // Thực thi truy vấn và chuyển thành danh sách DTO
																									// Trả về kết quả phân trang
					return new PagedResultDto<ProductListDto>(totalCount, productDtos);
				}
				catch (Exception)
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

