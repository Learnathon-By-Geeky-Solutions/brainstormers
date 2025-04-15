using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskForge.Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskForge.Application.Interfaces.Services;
using TaskForge.Domain.Entities;
using TaskForge.Infrastructure.Data;
using TaskForge.Infrastructure.Repositories.Common;

namespace TaskForge.Infrastructure.Repositories
{
	public class TaskAssignmentRepository : Repository<TaskAssignment>, ITaskAssignmentRepository
	{
		public TaskAssignmentRepository(ApplicationDbContext context, IUserContextService userContextService)
			: base(context, userContextService)
		{
		}
	}
}