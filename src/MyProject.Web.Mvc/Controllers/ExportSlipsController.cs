using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using MyProject.Controllers;
using MyProject.ExportSlips;
using MyProject.ExportSlips.Dto;
using MyProject.Web.Models.ExportSlips;
using Abp.Application.Services.Dto;

namespace MyProject.Web.Controllers
{
	/// <summary>
	/// Controller quản lý phiếu xuất kho
	/// </summary>
	public class ExportSlipsController : MyProjectControllerBase
	{
		private readonly IExportSlipAppService _exportSlipAppService;

		public ExportSlipsController(IExportSlipAppService exportSlipAppService)
		{
			_exportSlipAppService = exportSlipAppService;
		}

		/// <summary>
		/// Trang danh sách phiếu xuất kho
		/// </summary>
		public async Task<ActionResult> Index(GetAllExportSlipsInput input)
		{
			if (input == null)
			{
				input = new GetAllExportSlipsInput();
			}

			var output = await _exportSlipAppService.GetAllExportSlips(input);
			var model = new ExportSlipViewModel
			{
				ExportSlips = output.Items.ToList()
			};

			return View(model);
		}

		/// <summary>
		/// Modal sửa phiếu xuất kho (chỉ khi Status = Draft)
		/// </summary>
		public async Task<ActionResult> EditModal(int exportSlipId)
		{
			var exportSlip = await _exportSlipAppService.GetExportSlipById(exportSlipId);
			var model = new EditExportSlipViewModel
			{
				ExportSlip = exportSlip
			};
			return PartialView("_EditModal", model);
		}

		/// <summary>
		/// Trang chi tiết phiếu xuất kho
		/// </summary>
		public async Task<ActionResult> Detail(int exportSlipId)
		{
			var exportSlip = await _exportSlipAppService.GetExportSlipById(exportSlipId);
			var model = new EditExportSlipViewModel
			{
				ExportSlip = exportSlip
			};
			return View(model);
		}
	}
}

