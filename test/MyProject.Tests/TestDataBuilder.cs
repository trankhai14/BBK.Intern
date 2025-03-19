using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyProject.EntityFrameworkCore;
using static MyProject.Tasks.Task;

namespace MyProject.Tests
{
    public class TestDataBuilder
    {
        private readonly MyProjectDbContext _context;

        public TestDataBuilder(MyProjectDbContext context)
        {
            _context = context;
        }

        public void Build()
        {
            _context.Tasks.AddRange(
                new Task("Follow the white rabbit", "Follow the white rabbit in order to know the reality."),
                new Task("Clean your room") { State = TaskState.Completed }
                );
        }
    }
}
