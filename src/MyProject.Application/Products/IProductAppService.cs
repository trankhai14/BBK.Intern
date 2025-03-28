using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyProject.Categories.Dto;
using MyProject.Product.Dtos;
using MyProject.Products.Dtos;

namespace MyProject.Products
{
	public interface IProductAppService : IApplicationService
	{
		Task<PagedResultDto<ProductListDto>> GetAll(GetAllProductsInput input);
		Task<ProductListDto> Create(CreateProductDto input);
		Task<ProductListDto> GetAsync(EntityDto<int> input);

		Task Delete(EntityDto<int> input);
		Task<ProductDetailDto> Detail(EntityDto<int> input);
		Task<List<ProductListDto>> GetProductByIds(List<int> productIds);
		Task<List<CategoryListDto>> GetAllCategory();
		Task<ProductListDto> Update(UpdateProductDto input);

		Task<List<ProductListDto>> GetAllProducts();
		Task<PagedResultDto<ProductListDto>> Search(GetAllProductsInput input);

		Task<bool> CheckProductsByCategoryId(int categoryId);

	}
}
