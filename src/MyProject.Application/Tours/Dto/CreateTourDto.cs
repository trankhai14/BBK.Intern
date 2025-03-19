using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Domain.Entities.Auditing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;

namespace MyProject.Tours.Dto
{
	public class CreateTourDto: FullAuditedEntity<long>
	{
		public string TourName { get; set; }
		public int MinGroupSize { get; set; }
		public int MaxGroupSize { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public long TourTypeId { get; set; }
		public string Transportation { get; set; }
		public decimal TourPrice { get; set; }
		public string PhoneNumber { get; set; }
		public string Description { get; set; }
		public string Attachment { get; set; }
		public IFormFile AttachmentFile { get; set; }
	}
}
