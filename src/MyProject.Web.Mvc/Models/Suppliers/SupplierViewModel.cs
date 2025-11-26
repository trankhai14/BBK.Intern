using System.Collections.Generic;
using MyProject.Suppliers.Dto;

namespace MyProject.Web.Models.Suppliers
{
	public class SupplierViewModel
	{
		public IReadOnlyList<SupplierDto> Suppliers;

		public SupplierViewModel(IReadOnlyList<SupplierDto> suppliers)
		{
			this.Suppliers = suppliers;
		}
	}
}

