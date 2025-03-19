using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services;
using MyProject.Product.Dtos;
using MyProject.Web.Dto;

namespace MyProject.Websites
{
	public interface IWebAppService : IApplicationService
	{
		//Task<List<WebProductDto>> SearchProducts(String keyword);
	}
}
