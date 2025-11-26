using System;
using Abp.Application.Services.Dto;

namespace MyProject.Suppliers.Dto
{
	public class SupplierDto : EntityDto<int>
	{
		public string Name { get; set; }
		public string Code { get; set; }
		public string Phone { get; set; }
		public string Email { get; set; }
		public string Address { get; set; }
		public string ContactPerson { get; set; }
		public string Notes { get; set; }
		public bool IsActive { get; set; }
		public DateTime CreationTime { get; set; }
		public long? CreatorUserId { get; set; }
	}
}

