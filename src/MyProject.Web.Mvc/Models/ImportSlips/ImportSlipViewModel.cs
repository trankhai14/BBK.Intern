using System.Collections.Generic;
using MyProject.ImportSlips.Dto;

namespace MyProject.Web.Models.ImportSlips
{
	public class ImportSlipViewModel
	{
		public List<ImportSlipDto> ImportSlips { get; set; }

		public ImportSlipViewModel()
		{
			ImportSlips = new List<ImportSlipDto>();
		}
	}

	public class EditImportSlipViewModel
	{
		public ImportSlipDto ImportSlip { get; set; }
	}
}

