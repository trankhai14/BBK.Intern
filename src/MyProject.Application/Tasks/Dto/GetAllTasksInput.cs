using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Abp.Domain.Entities.Auditing;
using System;
using static MyProject.Tasks.Task;

namespace MyProject.Tasks
{
    public class GetAllTasksInput
    {
        public TaskState? State { get; set; }
    }
}