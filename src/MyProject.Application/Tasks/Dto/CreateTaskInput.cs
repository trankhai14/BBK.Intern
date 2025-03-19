using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MyProject.Tasks.Task;

namespace MyProject.Tasks.Dto
{
    public class CreateTaskInput
    {
        [Required]
        [StringLength(Task.MaxTitleLength)]
        public string Title { get; set; }

        [StringLength(Task.MaxDescriptionLength)]
        public string Description { get; set; }
        public TaskState State { get; set; }

        public int AssignedPersonId { get; set; }
    }
}
