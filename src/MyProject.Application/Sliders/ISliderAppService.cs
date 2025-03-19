using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Microsoft.AspNetCore.Mvc;
using MyProject.Sliders.Dto;

namespace MyProject.Sliders
{
	public interface ISliderAppService : IApplicationService
	{
		Task<PagedResultDto<SliderListDto>> GetAllSlider(GetAllSlidersInput input);
		Task CreateSlider(CreateSliderDto input);
		Task<SliderListDto> GetSlider(EntityDto<int> input);
		Task UpdateSlider(UpdateSliderDto input);
		Task UpdateActive(EntityDto<int> input);
		Task DeleteSlider(EntityDto<int> input);
		Task<List<SliderListDto>> GetSliderByActive();
	}
}
