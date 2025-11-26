using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MyProject.Authorization;
using MyProject.Sliders.Dto;

namespace MyProject.Sliders
{
	//[AbpAuthorize(PermissionNames.Pages_Sliders)]
	public class SliderAppService :MyProjectAppServiceBase, ISliderAppService
	{
		private readonly IRepository<Slider> _sliderRepository;
		//private readonly IHttpContextAccessor _httpContextAccessor;

		public SliderAppService
			(
			IRepository<Slider> sliderRepository
			//IHttpContextAccessor httpContextAccessor
			)
		{
			_sliderRepository = sliderRepository;
			//_httpContextAccessor = httpContextAccessor;
		}
		public async Task<PagedResultDto<SliderListDto>> GetAllSlider(GetAllSlidersInput input)
		{
			var query =  _sliderRepository.GetAll();

			if(!string.IsNullOrWhiteSpace(input.Keyword))
			{
				string keywordLower = input.Keyword.ToLower();
				query = query.Where(x => x.Title.ToLower().Contains(keywordLower));
			}

			if(input.IsActive != null)
			{
			query = query.Where(x => x.IsActive == input.IsActive.Value);
			}

			var sliderCount = await query.CountAsync();

			query = query.OrderByDescending(s => s.CreationTime);

			var sliderDtos = await query.PageBy(input).Select(slider => new SliderListDto
			{
				Id = slider.Id,
				Title = slider.Title,
				Description = slider.Description,
				Image = slider.Image,
				IsActive = slider.IsActive,
				CreationTime = slider.CreationTime
			}).ToListAsync();

			return new PagedResultDto<SliderListDto>(sliderCount, sliderDtos);
		}

		[AbpAuthorize(PermissionNames.Pages_Sliders_Create)]
		public async Task CreateSlider(CreateSliderDto input)
		{
			var slider = new Slider
			{
				Title = input.Title,
				Description = input.Description,
				Image = input.Image,
				IsActive = input.IsActive,
				CreationTime = DateTime.Now
			};

			await _sliderRepository.InsertAsync(slider);
			//await CurrentUnitOfWork.SaveChangesAsync();

		}

		[AbpAuthorize(PermissionNames.Pages_Sliders_Edit)]
		public async Task UpdateSlider(UpdateSliderDto input)
		{
			var slider = await _sliderRepository.GetAsync(input.Id);
			slider.Title = input.Title;
			slider.Description = input.Description;
			slider.Image = input.Image;
			slider.IsActive = input.IsActive;
			await _sliderRepository.UpdateAsync(slider);
			//await CurrentUnitOfWork.SaveChangesAsync();
		}

		public async Task UpdateActive(EntityDto<int> input)
		{
			var slider = await _sliderRepository.GetAsync(input.Id);
			slider.IsActive = !slider.IsActive;
			await _sliderRepository.UpdateAsync(slider);
		}

		[AbpAuthorize(PermissionNames.Pages_Sliders_Delete)]
		public async Task DeleteSlider(EntityDto<int> input)
		{
			var slider = await _sliderRepository.GetAsync(input.Id);
			if(slider == null)
			{
				throw new Exception("Slider not found");
			}
			await _sliderRepository.DeleteAsync(slider);

		}

		public async Task<SliderListDto> GetSlider(EntityDto<int> input)
		{
			var slider = await _sliderRepository.GetAsync(input.Id);

			return new SliderListDto
			{
				Id = slider.Id,
				Title = slider.Title,
				Description = slider.Description,
				Image = slider.Image,
				IsActive = slider.IsActive,
				CreationTime = slider.CreationTime
			};
		}

		public async Task<List<SliderListDto>> GetSliderByActive()
		{
			var slider = await _sliderRepository.GetAllListAsync(x => x.IsActive);

			//var request = _httpContextAccessor.HttpContext.Request;
			//var baseUrl = $"{request.Scheme}://{request.Host}"; // Lấy URL đầy đủ của Backend  

			return slider.Select(x => new SliderListDto
			{
				Id = x.Id,
				Title = x.Title,
				Description = x.Description,
				Image = x.Image,
				IsActive = x.IsActive,
				CreationTime = x.CreationTime
			}).ToList();
		}

	
	}
}
