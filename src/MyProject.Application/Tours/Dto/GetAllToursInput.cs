using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;

namespace MyProject.Tours.Dto
{
	public class GetAllToursInput : PagedAndSortedResultRequestDto
	{
		public string TourName { get; set; }
		public int? MinGroupSize { get; set; }
		public int? MaxGroupSize { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public string Transportation { get; set; }
		public decimal? TourPrice { get; set; }
		public string PhoneNumber { get; set; }
		public string Description { get; set; }
		public string Attachment { get; set; }

		public string? Keyword { get; set; }
		public string? isTransprotation { get; set; }
		public long? isTourTypeId { get; set; }

	}
}
