using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Microsoft.EntityFrameworkCore;
using MyProject.TaskAppService;
using MyProject.TaskAppService.Dto;
using MyProject.Tasks.Dto;

namespace MyProject.Tasks
{
    public class TaskAppService : MyProjectAppServiceBase, ITaskAppService
    {
        private readonly IRepository<Task> _taskRepository;

        public TaskAppService(IRepository<Task> taskRepository)
        {
            _taskRepository = taskRepository;
        }

        public async Task<ListResultDto<TaskListDto>> GetAll(GetAllTasksInput input)
        {
            var tasks = await _taskRepository
                .GetAll()
                .Include(t => t.AssignedPerson)
                .WhereIf(input.State.HasValue, t => t.State == input.State.Value)
                .OrderByDescending(t => t.CreationTime)
                .ToListAsync();

            var taskDtos = tasks.Select(t => new TaskListDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                CreationTime = t.CreationTime,
                State = t.State,
                AssignedPersonName = t.AssignedPerson.Name,
            }).ToList();

            return new ListResultDto<TaskListDto>(taskDtos);
        }

        public async System.Threading.Tasks.Task Create(CreateTaskInput input)
        {
            //var task = ObjectMapper.Map<Task>(input);
             Task task = new Task();
            task.AssignedPersonId = input.AssignedPersonId;
            //task.State = input.State
            task.Title = input.Title;
            task.Description = input.Description;


            await _taskRepository.InsertAsync(task);
        }

        
    }
}
