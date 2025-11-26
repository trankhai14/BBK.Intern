using System.ComponentModel.DataAnnotations;

namespace MyProject.Suppliers.Dto
{
	public class UpdateSupplierDto
	{
		[Required]
		public int Id { get; set; }

		[Required]
		[StringLength(256)]
		public string Name { get; set; }

		[StringLength(50)]
		public string Code { get; set; }

		[StringLength(20)]
		public string Phone { get; set; }

		[StringLength(256)]
		public string Email { get; set; }

		[StringLength(500)]
		public string Address { get; set; }

		[StringLength(256)]
		public string ContactPerson { get; set; }

		[StringLength(1000)]
		public string Notes { get; set; }

		public bool IsActive { get; set; }
	}
}

