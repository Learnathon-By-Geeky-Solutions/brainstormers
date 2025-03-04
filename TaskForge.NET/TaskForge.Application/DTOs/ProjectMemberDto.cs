using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskForge.Domain.Enums;

namespace TaskForge.Application.DTOs
{
    public class ProjectMemberDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public ProjectRole Role { get; set; }
    }

}
