using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using MyProject.Carts.Dto;
using MyProject.Categories;

namespace MyProject.Carts
{
	/// <summary>
	/// Interface for Cart Frontend Application Service
	/// </summary>
	public interface ICartFrontendAppService
	{
		/// <summary>
		/// Counts the number of items in the cart.
		/// </summary>
		/// <returns>The total count of items in the cart.</returns>
		Task<int> CountCart();
	}

	/// <summary>
	/// Implementation of Cart Frontend Application Service
	/// </summary>
	public class CartFrontendAppService : MyProjectAppServiceBase, ICartFrontendAppService
	{
		private readonly IRepository<CartItem> _cartRepository;

		/// <summary>
		/// Initializes a new instance of the <see cref="CartFrontendAppService"/> class.
		/// </summary>
		/// <param name="cartRepository">The cart repository.</param>
		public CartFrontendAppService(IRepository<CartItem> cartRepository)
		{
			_cartRepository = cartRepository;
		}

		/// <summary>
		/// Counts the number of items in the cart.
		/// </summary>
		/// <returns>The total count of items in the cart.</returns>
		public async Task<int> CountCart()
		{
			// Get the current user ID
			var userId = AbpSession.UserId; 
			using var uow = UnitOfWorkManager.Begin();
			using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
			{
				try
				{
					var cartItems = _cartRepository.GetAll().Where(x => x.UserId == userId);
					// Count the total number of items in the cart
					var totalCount = await cartItems.CountAsync();
					return totalCount;
				}
				catch (Exception)
				{
					return 0;
				}
				finally
				{
					await uow.CompleteAsync();

				}
			}
		}
	}
}
