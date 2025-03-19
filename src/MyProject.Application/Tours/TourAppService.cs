using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Microsoft.EntityFrameworkCore;
using MyProject.Tours.Dto;

namespace MyProject.Tours
{
	public class TourAppService : ApplicationService, ITourAppService
	{
		private readonly IRepository<Tour, long> _tourRepository;
		public TourAppService(IRepository<Tour, long> tourRepository)
		{
			_tourRepository = tourRepository;
		}

		public async Task<PagedResultDto<TourListDto>> GetAllTours(GetAllToursInput input)
		{
			var tours = _tourRepository.GetAll();

			if (!string.IsNullOrEmpty(input.Keyword))
			{
				tours = tours.Where(t => t.TourName.Contains(input.Keyword));
			}

			if (!string.IsNullOrEmpty(input.isTransprotation))
			{
				tours = tours.Where(t => t.Transportation == input.isTransprotation);
			}

			if (input.isTourTypeId != null)
			{
				tours = tours.Where(t => t.TourTypeId == input.isTourTypeId);
			}

			var tourCount = await tours.CountAsync();
			
			

			var tourDtos =await tours.PageBy(input).Select(t => new TourListDto
			{
				Id = t.Id,
				TourName = t.TourName,
				MinGroupSize = t.MinGroupSize,
				MaxGroupSize = t.MaxGroupSize,
				StartDate = t.StartDate,
				EndDate = t.EndDate,
				TourTypeId = t.TourTypeId,
				Transportation = t.Transportation,
				TourPrice = t.TourPrice,
				PhoneNumber = t.PhoneNumber,
				Description = t.Description,
				Attachment = t.Attachment
			}
			).ToListAsync();
			
			return new PagedResultDto<TourListDto>(tourCount, tourDtos);
		}

		public async Task CreateTour(CreateTourDto input)
		{
			var tour = new Tour
			{
				TourName = input.TourName,
				MinGroupSize = input.MinGroupSize,
				MaxGroupSize = input.MaxGroupSize,
				StartDate = input.StartDate,
				EndDate = input.EndDate,
				TourTypeId = input.TourTypeId,
				Transportation = input.Transportation,
				TourPrice = input.TourPrice,
				PhoneNumber = input.PhoneNumber,
				Description = input.Description,
				Attachment = input.Attachment
			};

			await _tourRepository.InsertAsync(tour);

		}

		public async Task<TourListDto> UpdateTour(UpdateTourDto input)
		{
			var tour = await _tourRepository.FirstOrDefaultAsync(x => x.Id == input.Id);
			if (tour == null)
			{
				throw new Exception("Tour không tồn tại hoặc đã bị xóa."); // Hoặc return null
			}

			// Cập nhật thông tin tour
			tour.TourName = input.TourName;
			tour.MinGroupSize = input.MinGroupSize;
			tour.MaxGroupSize = input.MaxGroupSize;
			tour.StartDate = input.StartDate;
			tour.EndDate = input.EndDate;
			tour.TourTypeId = input.TourTypeId;
			tour.Transportation = input.Transportation;
			tour.TourPrice = input.TourPrice;
			tour.PhoneNumber = input.PhoneNumber;
			tour.Description = input.Description;
			tour.Attachment = input.Attachment;

			await _tourRepository.UpdateAsync(tour);

			return new TourListDto
			{
				Id = tour.Id,
				TourName = tour.TourName,
				MinGroupSize = tour.MinGroupSize,
				MaxGroupSize = tour.MaxGroupSize,
				StartDate = tour.StartDate,
				EndDate = tour.EndDate,
				TourTypeId = tour.TourTypeId,
				Transportation = tour.Transportation,
				TourPrice = tour.TourPrice,
				PhoneNumber = tour.PhoneNumber,
				Description = tour.Description,
				Attachment = tour.Attachment
			};
		}

		public async Task DeleteTour(EntityDto<long> input)
		{
			var tour = await _tourRepository.FirstOrDefaultAsync(x => x.Id == input.Id);
			if (tour == null)
			{
				throw new Exception("Tour không tồn tại hoặc đã bị xóa."); // Hoặc return null
			}
			await _tourRepository.DeleteAsync(tour);
		}


		public async Task<TourListDto> GetTourById(EntityDto<long> input)
		{
			if(input.Id == 0)
			{
				throw new ArgumentException("ID không hợp lệ.");
			}
			var tour = await _tourRepository.FirstOrDefaultAsync(x => x.Id == input.Id);
			return new TourListDto
			{
				Id = tour.Id,
				TourName = tour.TourName,
				MinGroupSize = tour.MinGroupSize,
				MaxGroupSize = tour.MaxGroupSize,
				StartDate = tour.StartDate,
				EndDate = tour.EndDate,
				TourTypeId = tour.TourTypeId,
				Transportation = tour.Transportation,
				TourPrice = tour.TourPrice,
				PhoneNumber = tour.PhoneNumber,
				Description = tour.Description,
				Attachment = tour.Attachment
			};
		}
	}



	
}
