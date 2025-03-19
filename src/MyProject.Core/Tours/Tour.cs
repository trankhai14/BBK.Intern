using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Domain.Entities.Auditing;

namespace MyProject.Tours
{
	[Table("AppTours")]
	public class Tour:FullAuditedEntity<long>
	{
		[Required]
		public string TourName { get; set; } // tên tour
		public int MinGroupSize { get; set; } // số khách tối thiểu
		public int MaxGroupSize { get; set; } // số khách tối đa
		public long TourTypeId { get; set; } // loại tour

		public DateTime? StartDate { get; set; } // ngày bắt đầu
		public DateTime? EndDate { get; set; } // ngày kết thúc

		[Required]
		public string Transportation { get; set; } // phương tiện
		public decimal? TourPrice { get; set; } // giá tour

		[Required]
		public string PhoneNumber { get; set; } // số điện thoại

		[Required]
		public string Description { get; set; } // mô tả
		public string Attachment { get; set; } // file đính kèm
	}
}
