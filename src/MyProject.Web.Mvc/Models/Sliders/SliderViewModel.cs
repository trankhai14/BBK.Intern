using System.Collections.Generic;
using MyProject.Sliders.Dto;

namespace MyProject.Web.Models.Sliders
{
	public class SliderViewModel
	{
		public IReadOnlyList<SliderListDto> Sliders;
		public SliderViewModel(IReadOnlyList<SliderListDto> sliders) 
		{
			Sliders = sliders;
		}
	}
}
