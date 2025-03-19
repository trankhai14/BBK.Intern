using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using MyProject.Tours.Dto;

namespace MyProject.Tours
{
	public interface ITourAppService : IApplicationService
	{
		Task<PagedResultDto<TourListDto>> GetAllTours(GetAllToursInput input);
		Task CreateTour(CreateTourDto input);
		Task<TourListDto> GetTourById(EntityDto<long> input);
		Task<TourListDto> UpdateTour(UpdateTourDto input);

		Task DeleteTour(EntityDto<long> input);
	}
}
