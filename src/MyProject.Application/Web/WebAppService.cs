using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using MyProject.Product.Dtos;
using MyProject.Products;
using MyProject.Web.Dto;


namespace MyProject.Websites
{
	public class WebAppService : MyProjectAppServiceBase, IWebAppService
	{
		private readonly IRepository<Products.Product> _productRepository;

		public WebAppService(IRepository<Products.Product> productRepository)
		{
			_productRepository = productRepository;
		}

		//public async Task<List<WebProductDto>> SearchProducts(string keyword)
		//{
		//	var productQuery = _productRepository.GetAll();

		//	if (!string.IsNullOrWhiteSpace(keyword))
		//	{
		//		string keywordLower = keyword.ToLower();
		//		productQuery = productQuery
		//				.Where(x => x.Name.ToLower().Contains(keywordLower))
		//				.OrderByDescending(x => x.CreationTime);
		//	}

		//	var webProductDtos = await productQuery.Select(p => new WebProductDto
		//	{
		//		Id = p.Id,
		//		Name = p.Name,
		//		Description = p.Description,
		//		Price = p.Price,
		//		Image = p.Image,
		//		CreationTime = p.CreationTime,
		//	}).ToListAsync();

		//	return webProductDtos;
		//}
	}

}
