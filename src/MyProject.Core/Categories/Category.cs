using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using MyProject.Products;

namespace MyProject.Categories
{
	[Table("AppCategory")]
	public class Category : Entity
	{
		[Required]
		[MaxLength(256)]
		public string CategoryName { get; set; }
		public string CategoryDescription { get; set; }

		public Category()
		{

		}
	
		public Category(string name)
		{
			CategoryName = name;
		}

	}
}
