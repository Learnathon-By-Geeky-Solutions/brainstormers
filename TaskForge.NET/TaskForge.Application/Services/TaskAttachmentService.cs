using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskForge.Application.Interfaces.Repositories.Common;
using TaskForge.Application.Interfaces.Services;

namespace TaskForge.Application.Services
{
    public class TaskAttachmentService : ITaskAttachmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        public TaskAttachmentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
    }
}
