using Microsoft.EntityFrameworkCore;
using Abp.Zero.EntityFrameworkCore;
using MyProject.Authorization.Roles;
using MyProject.Authorization.Users;
using MyProject.MultiTenancy;
using Abp.EntityFrameworkCore;
using MyProject.Tasks;
using MyProject.People;
using MyProject.Products;
using MyProject.Categories;
using MyProject.Carts;
using MyProject.Sliders;
using MyProject.Tours;
using MyProject.Orders;
using MyProject.Inventories;
using MyProject.InventoryTransactions;
using MyProject.CustomerProfiles;
using MyProject.FlashSales;
using MyProject.Suppliers;
using MyProject.Payments;
using MyProject.ImportSlips;
using MyProject.ExportSlips;
using MyProject.Stocktakings;


namespace MyProject.EntityFrameworkCore
{
	public class MyProjectDbContext : AbpZeroDbContext<Tenant, Role, User, MyProjectDbContext>
	{
		/* Define a DbSet for each entity of the application */
		public DbSet<Task> Tasks { get; set; }

		public DbSet<Person> People { get; set; }

		public DbSet<Product> Products { get; set; }
		public DbSet<ProductSpecification> ProductSpecifications { get; set; }

		public DbSet<Category> Categories { get; set; }
		public DbSet<Slider> Sliders { get; set; }

		public DbSet<Tour> Tours { get; set; }

		public DbSet<CartItem> CartItems { get; set; }
		public DbSet<Order> Orders { get; set; }
		public DbSet<OrderDetail> OrderDetail { get; set; }
		public DbSet<Inventory> Inventories { get; set; }
		public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
		public DbSet<CustomerProfile> CustomerProfiles { get; set; }
		public DbSet<FlashSale> FlashSales { get; set; }
		public DbSet<FlashSaleProduct> FlashSaleProducts { get; set; }
		public DbSet<Supplier> Suppliers { get; set; }
		public DbSet<PaymentTransaction> PaymentTransactions { get; set; }

		// Warehouse Management Entities
		public DbSet<ImportSlip> ImportSlips { get; set; }
		public DbSet<ImportDetail> ImportDetails { get; set; }
		public DbSet<ExportSlip> ExportSlips { get; set; }
		public DbSet<ExportDetail> ExportDetails { get; set; }
		public DbSet<Stocktaking> Stocktakings { get; set; }
		public DbSet<StocktakingDetail> StocktakingDetails { get; set; }

		public MyProjectDbContext(DbContextOptions<MyProjectDbContext> options)
						: base(options)
		{

		}
	}
}

