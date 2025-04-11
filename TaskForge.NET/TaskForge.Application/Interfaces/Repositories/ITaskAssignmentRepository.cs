using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskForge.Application.Interfaces.Repositories.Common;
using TaskForge.Domain.Entities;

namespace TaskForge.Application.Interfaces.Repositories
{
	public interface ITaskAssignmentRepository : IRepository<TaskAssignment>
	{
	}
}
