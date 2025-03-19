using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using static MyProject.Tasks.Task;

namespace MyProject.Web.Models.People
{
	public class CreateTaskViewModel
	{
		public List<SelectListItem> People { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public int AssignedPersonId { get; set; }
		public TaskState TaskState { get; set; }

		public CreateTaskViewModel(List<SelectListItem> people)
		{
			People = people;
		}
	}
}
