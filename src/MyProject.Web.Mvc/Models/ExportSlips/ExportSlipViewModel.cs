using System.Collections.Generic;
using MyProject.ExportSlips.Dto;

namespace MyProject.Web.Models.ExportSlips
{
	public class ExportSlipViewModel
	{
		public List<ExportSlipDto> ExportSlips { get; set; }

		public ExportSlipViewModel()
		{
			ExportSlips = new List<ExportSlipDto>();
		}
	}

	public class EditExportSlipViewModel
	{
		public ExportSlipDto ExportSlip { get; set; }
	}
}

