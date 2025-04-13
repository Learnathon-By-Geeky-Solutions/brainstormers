using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskForge.Domain.Entities
{
    public class TaskDependency
    {
        public int TaskId { get; set; }
        public TaskItem Task { get; set; } = null!;

        public int DependsOnTaskId { get; set; }
        public TaskItem DependsOnTask { get; set; } = null!;
    }

}
