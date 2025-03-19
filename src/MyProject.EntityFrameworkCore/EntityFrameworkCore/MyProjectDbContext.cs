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


namespace MyProject.EntityFrameworkCore
{
    public class MyProjectDbContext : AbpZeroDbContext<Tenant, Role, User, MyProjectDbContext>
    {
        /* Define a DbSet for each entity of the application */
        public DbSet<Task> Tasks { get; set; }

        public DbSet<Person> People { get; set; }

        public DbSet<Product> Products { get; set; }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Slider> Sliders { get; set; }

		    public DbSet<Tour> Tours { get; set; }

		public DbSet<CartItem> CartItems { get; set; }

		public MyProjectDbContext(DbContextOptions<MyProjectDbContext> options)
            : base(options)
        {

        }
    }
}

