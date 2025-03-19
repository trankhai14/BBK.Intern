using System;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Abp.Domain.Entities.Auditing;
using MyProject.People;
using static MyProject.Tasks.Task;

namespace MyProject.TaskAppService.Dto
{
    //[AutoMapFrom(typeof(Task))]
    public class TaskListDto : EntityDto, IHasCreationTime
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime CreationTime { get; set; }

        public TaskState State { get; set; }

        public int AssignedPersonId { get; set; }

        public string AssignedPersonName { get; set; }
    }
}

