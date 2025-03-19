using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;
using Abp.Timing;
using MyProject.Categories;

namespace MyProject.Products
{
    [Table("AppProducts")]
    public class Product : Entity, IHasCreationTime
    {
        public const int MaxNameLength = 256;
        public const int MaxDescriptionLength = 64 * 1024; // 64KB

        [Required]
        [StringLength(MaxNameLength)]
        public string Name { get; set; }

        [StringLength(MaxDescriptionLength)]
        public string Description { get; set; }

        public DateTime CreationTime { get; set; }

        public ProductState State { get; set; }

        public decimal Price { get; set; }

        public string Image {  get; set; }

		    public int CategoryId { get; set; }
		    [ForeignKey("CategoryId")]
		    public Category Category { get; set; }
         
    

		public Product()
        {
            CreationTime = Clock.Now;
            State = ProductState.Available;
        }

        public Product(string name, string description = null, decimal price = 0, int categoryId = 0 )
            : this()
        {
            Name = name;
            Description = description;
            Price = price;
            CategoryId = categoryId;
        }

        public enum ProductState : byte
        {
            Available = 0,
            OutOfStock = 1,
            Discontinued = 2
        }
    }
}
