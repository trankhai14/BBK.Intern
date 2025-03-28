using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using MyProject.Carts.Dto;
using MyProject.Products;

namespace MyProject.Carts
{
	public class CartAppService : MyProjectAppServiceBase, ICartAppService
	{
		//private readonly IRepository<Cart> _cartRepository;
		private readonly IRepository<CartItem> _cartItemRepository;
		private readonly IProductAppService _productAppService;

		public CartAppService(
			//IRepository<Cart> cartRepository, 
			IProductAppService productAppService,
			IRepository<CartItem> cartItemRepository)
		{
		//	_cartRepository = cartRepository;
			_productAppService = productAppService;
			_cartItemRepository = cartItemRepository;
		}
		public async Task<List<CartsDto>> GetAllCart()
		{
			if(AbpSession.UserId != null)
			{
				var cart = await _cartItemRepository.GetAllListAsync(c => c.UserId == AbpSession.UserId);
				if (cart != null)	
				{
					
					return cart.Select(c => new CartsDto
					{
						Id = c.Id,
						ProductId = c.ProductId,
						Quantity = c.Quantity,
						UserId = c.UserId,
					}).ToList();
				}
				else
				{
					return new List<CartsDto>();
				}
			}
			else
			{
				return null;
			}
		}

		public async Task AddToCart(int productId, int quantity, bool check)
		{

			if (AbpSession.UserId != null)
			{
				var cartItem = await _cartItemRepository.FirstOrDefaultAsync(c => c.UserId == AbpSession.UserId && c.ProductId == productId);
				if (cartItem != null)
				{
					if (check == true)
					{
						cartItem.Quantity += quantity;
					}
					else
					{
						cartItem.Quantity -= quantity;
					}
				}
				else
				{
					cartItem = new CartItem
					{
						UserId = AbpSession.UserId.Value,
						ProductId = productId,
						Quantity = quantity
					};
				}
				await _cartItemRepository.InsertOrUpdateAsync(cartItem);
			}
		}

		public async Task DeleteCart(int productId)
		{
			if (AbpSession.UserId != null)
			{
				var cartItem = await _cartItemRepository.FirstOrDefaultAsync(c => c.UserId == AbpSession.UserId && c.ProductId == productId);
				if (cartItem != null)
				{
					await _cartItemRepository.DeleteAsync(cartItem);
				}
			}
		}

		public async Task ClearProduct(int productId)
		{
			if (AbpSession.UserId != null)
			{
				var cartItem = await _cartItemRepository.GetAllListAsync(c => c.UserId == AbpSession.UserId && c.ProductId == productId);
				if (cartItem != null)
				{
					foreach (var item in cartItem)
					{
						await _cartItemRepository.DeleteAsync(item);
					}
				}
			}
		}

		public async Task UpdateCart(int productId, int quantity)
		{
			if (AbpSession.UserId != null)
			{
				var cartItem = await _cartItemRepository.FirstOrDefaultAsync(c => c.UserId == AbpSession.UserId && c.ProductId == productId);
				if (cartItem != null)
				{
					cartItem.Quantity = quantity;
					await _cartItemRepository.UpdateAsync(cartItem);
				}
			}
		}

		public async Task ClearCart(long userId)
		{
			var userCartItems = await _cartItemRepository.GetAllListAsync(c => c.UserId == userId);

			foreach (var cartItem in userCartItems)
			{
				await _cartItemRepository.DeleteAsync(cartItem);
			}
		}
	}
}

