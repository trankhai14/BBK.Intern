using System.Collections.Generic;
using MyProject.Tours.Dto;

namespace MyProject.Web.Models.Tours
{
	public class TourViewModel
	{
		public IReadOnlyList<TourListDto> Tours { get; set; }

		public TourViewModel(IReadOnlyList<TourListDto> tours)
		{
			Tours = tours;
		}
	}
}
