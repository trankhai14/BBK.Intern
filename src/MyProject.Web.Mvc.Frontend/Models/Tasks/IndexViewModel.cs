using MyProject.TaskAppService.Dto;
using static MyProject.Tasks.Task;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Abp.Localization;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using MyProject.Product.Dtos;

namespace MyProject.Web.Models.Tasks
{
	public class IndexViewModel
	{
		private IReadOnlyList<ProductListDto> items;

		public IReadOnlyList<TaskListDto> Tasks { get; }

		public IndexViewModel(IReadOnlyList<TaskListDto> tasks)
		{
			Tasks = tasks;
		}

		public IndexViewModel(IReadOnlyList<ProductListDto> items)
		{
			this.items = items;
		}

		public string GetTaskLabel(TaskListDto tasks)
		{
			switch (tasks.State)
			{
				case TaskState.Open:
					return "label-success";
				default:
					return "label-default";
			}
		}

		public TaskState? SelectedTaskState { get; set; }

		public List<SelectListItem> GetTasksStateSelectListItems(ILocalizationManager localizationManager)
		{
			var list = new List<SelectListItem>
				{
						new SelectListItem
						{
								Text = localizationManager.GetString(MyProjectConsts.LocalizationSourceName, "AllTasks"),
								Value = "",
								Selected = SelectedTaskState == null
						}
				};

			list.AddRange(Enum.GetValues(typeof(TaskState))
							.Cast<TaskState>()
							.Select(state =>
									new SelectListItem
									{
										Text = localizationManager.GetString(MyProjectConsts.LocalizationSourceName, $"Tasks_{state}"),
										Value = state.ToString(),
										Selected = state == SelectedTaskState
									})
			);

			return list;
		}
	}
}
