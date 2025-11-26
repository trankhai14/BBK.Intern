using System;

namespace MyProject.Products.Dtos
{
	public class ProductSpecificationDto
	{
		public int Id { get; set; }
		public int ProductId { get; set; }

		// Thông tin kỹ thuật điện thoại
		public string Sku { get; set; }
		public string ModelNumber { get; set; }
		public string Chipset { get; set; }
		public string Ram { get; set; }
		public string Storage { get; set; }
		public string Screen { get; set; }
		public string OperatingSystem { get; set; }
		public string Battery { get; set; }
		public string Camera { get; set; }
		public string FrontCamera { get; set; }
		public string Sim { get; set; }
		public string Connectivity { get; set; }
		public string Security { get; set; }
		public string Charging { get; set; }
		public string ChargingPort { get; set; }
		public string Color { get; set; }
		public string Warranty { get; set; }
		public string TechnicalSpecifications { get; set; }

		public DateTime CreationTime { get; set; }
		public DateTime? LastModificationTime { get; set; }
	}

	public class CreateProductSpecificationDto
	{
		public int ProductId { get; set; }

		// Thông tin kỹ thuật điện thoại
		public string Sku { get; set; }
		public string ModelNumber { get; set; }
		public string Chipset { get; set; }
		public string Ram { get; set; }
		public string Storage { get; set; }
		public string Screen { get; set; }
		public string OperatingSystem { get; set; }
		public string Battery { get; set; }
		public string Camera { get; set; }
		public string FrontCamera { get; set; }
		public string Sim { get; set; }
		public string Connectivity { get; set; }
		public string Security { get; set; }
		public string Charging { get; set; }
		public string ChargingPort { get; set; }
		public string Color { get; set; }
		public string Warranty { get; set; }
		public string TechnicalSpecifications { get; set; }
	}

	public class UpdateProductSpecificationDto
	{
		public int Id { get; set; }
		public int ProductId { get; set; }

		// Thông tin kỹ thuật điện thoại
		public string Sku { get; set; }
		public string ModelNumber { get; set; }
		public string Chipset { get; set; }
		public string Ram { get; set; }
		public string Storage { get; set; }
		public string Screen { get; set; }
		public string OperatingSystem { get; set; }
		public string Battery { get; set; }
		public string Camera { get; set; }
		public string FrontCamera { get; set; }
		public string Sim { get; set; }
		public string Connectivity { get; set; }
		public string Security { get; set; }
		public string Charging { get; set; }
		public string ChargingPort { get; set; }
		public string Color { get; set; }
		public string Warranty { get; set; }
		public string TechnicalSpecifications { get; set; }
	}
}

