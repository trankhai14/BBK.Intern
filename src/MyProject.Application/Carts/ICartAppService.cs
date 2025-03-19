using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services;
using MyProject.Carts.Dto;
using MyProject.Products;

namespace MyProject.Carts
{
	public interface ICartAppService : IApplicationService
	{
		Task<List<CartsDto>> GetAllCart();
		Task AddToCart(int productId, int quantity, bool check);

		Task DeleteCart(int productId);

		Task UpdateCart(int productId, int quantity);

		//Task<int> CountCart();
	}
}
