using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using MyProject.Controllers;
using MyProject.ImportSlips;
using MyProject.ImportSlips.Dto;
using MyProject.Web.Models.ImportSlips;
using Abp.Application.Services.Dto;

namespace MyProject.Web.Controllers
{
	/// <summary>
	/// Controller quản lý phiếu nhập kho
	/// </summary>
	public class ImportSlipsController : MyProjectControllerBase
	{
		private readonly IImportSlipAppService _importSlipAppService;

		public ImportSlipsController(IImportSlipAppService importSlipAppService)
		{
			_importSlipAppService = importSlipAppService;
		}

		/// <summary>
		/// Trang danh sách phiếu nhập kho
		/// </summary>
		public async Task<ActionResult> Index(GetAllImportSlipsInput input)
		{
			if (input == null)
			{
				input = new GetAllImportSlipsInput();
			}

			var output = await _importSlipAppService.GetAllImportSlips(input);
			var model = new ImportSlipViewModel
			{
				ImportSlips = output.Items.ToList()
			};

			return View(model);
		}

		/// <summary>
		/// Modal sửa phiếu nhập kho (chỉ khi Status = Draft)
		/// </summary>
		public async Task<ActionResult> EditModal(int importSlipId)
		{
			var importSlip = await _importSlipAppService.GetImportSlipById(importSlipId);
			var model = new EditImportSlipViewModel
			{
				ImportSlip = importSlip
			};
			return PartialView("_EditModal", model);
		}

		/// <summary>
		/// Trang chi tiết phiếu nhập kho
		/// </summary>
		public async Task<ActionResult> Detail(int importSlipId)
		{
			var importSlip = await _importSlipAppService.GetImportSlipById(importSlipId);
			var model = new EditImportSlipViewModel
			{
				ImportSlip = importSlip
			};
			return View(model);
		}
	}
}

