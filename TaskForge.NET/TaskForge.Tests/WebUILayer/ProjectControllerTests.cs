using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Moq;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using TaskForge.Application.Common.Model;
using TaskForge.Application.DTOs;
using TaskForge.Application.Interfaces.Services;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;
using TaskForge.Web.Models;
using TaskForge.WebUI.Controllers;
using TaskForge.WebUI.Models;
using Xunit;

namespace TaskForge.Tests.Controllers
{
    public class ProjectControllerTests
    {
        private readonly Mock<IProjectService> _projectServiceMock = new();
        private readonly Mock<IProjectMemberService> _projectMemberServiceMock = new();
        private readonly Mock<ITaskService> _taskServiceMock = new();
        private readonly Mock<IProjectInvitationService> _invitationServiceMock = new();
        private readonly Mock<UserManager<IdentityUser>> _userManagerMock;

        public ProjectControllerTests()
        {

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            _userManagerMock = new Mock<UserManager<IdentityUser>>(
                Mock.Of<IUserStore<IdentityUser>>(),
                null, null, null, null, null, null, null, null);
        }

        private ProjectController CreateController() =>
            new(_projectMemberServiceMock.Object,
                _projectServiceMock.Object,
                _taskServiceMock.Object,
                _invitationServiceMock.Object,
                _userManagerMock.Object);

        [Fact]
        public async Task Index_ValidRequest_ReturnsViewWithProjectList()
        {
            var userId = "test-123";
            var user = new IdentityUser { Id = userId };

            var filter = new ProjectFilterDto
            {
                Title = "Test Project",
                Status = ProjectStatus.NotStarted,
                StartDateFrom = DateTime.Now.AddDays(-7),
                StartDateTo = DateTime.Now.AddDays(7),
                EndDateFrom = DateTime.Now.AddDays(-7),
                EndDateTo = DateTime.Now.AddDays(7),
                SortBy = "StartDate",
                SortOrder = "asc"
            };

            var projectList = new List<ProjectWithRoleDto>
            {
                new() {
                    ProjectId = 1,
                    ProjectTitle = "Test Project 1",
                    ProjectStatus = ProjectStatus.NotStarted,
                    ProjectStartDate = DateTime.Now,
                    ProjectEndDate = DateTime.Now.AddDays(30),
                    UserRoleInThisProject = ProjectRole.Admin
                },
                new() {
                    ProjectId = 2,
                    ProjectTitle = "Test Project 2",
                    ProjectStatus = ProjectStatus.InProgress,
                    ProjectStartDate = DateTime.Now.AddDays(-10),
                    ProjectEndDate = DateTime.Now.AddDays(20),
                    UserRoleInThisProject = ProjectRole.Contributor
                }
            };

            var paginatedList = new PaginatedList<ProjectWithRoleDto>(projectList, 15, 1, 10);

            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                            .ReturnsAsync(user);

            _projectServiceMock.Setup(x => x.GetFilteredProjectsAsync(
                    It.Is<ProjectFilterDto>(f => f.UserId == userId), 1, 10))
                .ReturnsAsync(paginatedList);

            var controller = CreateController();

            var result = await controller.Index(filter, 1, 10);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectListViewModel>(viewResult.Model);
            Assert.Equal(15, model.TotalItems);
            Assert.Equal(2, model.TotalPages);
        }

        [Fact]
        public async Task Update_Get_ReturnsPartialView_WhenProjectExists()
        {
            var project = new Project
            {
                Id = 1,
                Title = "Test Project",
                Description = "Test Description",
                StartDate = DateTime.UtcNow,
                Status = ProjectStatus.NotStarted,
                Members = new List<ProjectMember> {
                    new() {
                        Id = 1,
                        ProjectId = 1,
                        UserProfileId = 1,
                        Role = ProjectRole.Admin,
                        UserProfile = new UserProfile {
                            Id = 1,
                            UserId = "Tester01",
                            FullName = "Test User",
                            AvatarUrl = "test-avatar-url"
                        }
                    }
                },
                Invitations = new List<ProjectInvitation> {
                    new() {
                        Id = 10,
                        ProjectId = 1,
                        InvitedUserProfileId = 1,
                        Status = InvitationStatus.Pending,
                        InvitedUserProfile = new UserProfile { Id = 1 }
                    }
                },
                TaskItems = new List<TaskItem> {
                    new() {
                        Id = 1,
                        ProjectId = 1,
                        Title = "Test Task",
                        Status = TaskWorkflowStatus.ToDo,
                        Priority = TaskPriority.Medium,
                        StartDate = DateTime.UtcNow,
                    }
                }
            };
            project.SetEndDate(DateTime.UtcNow.AddDays(30));

            _projectServiceMock.Setup(s => s.GetProjectByIdAsync(1)).ReturnsAsync(project);

            var result = await CreateController().Update(1);

            var viewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_EditProjectForm", viewResult.ViewName);

            var model = Assert.IsType<ProjectUpdateViewModel>(viewResult.Model);
            Assert.Equal(project.Id, model.Id);
            Assert.Equal(project.Title, model.Title);
            Assert.Equal(project.Description, model.Description);
            Assert.Equal(project.Status, model.Status);
            Assert.InRange(model.StartDate, project.StartDate.AddSeconds(-1), project.StartDate.AddSeconds(1));
        }

        [Fact]
        public async Task Create_Post_ValidModel_CreatesProject_AndRedirects()
        {
            // Arrange
            var model = new CreateProjectViewModel
            {
                Title = "Test Project",
                Description = "Some desc",
                Status = ProjectStatus.Completed,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(2)
            };
            model.StatusOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = ProjectStatus.NotStarted.ToString(), Text = "Not Started" },
                new SelectListItem { Value = ProjectStatus.InProgress.ToString(), Text = "In Progress" },
                new SelectListItem { Value = ProjectStatus.Completed.ToString(), Text = "Completed" }
            };

            var validationResults = model.Validate(new ValidationContext(model)).ToList();
            Assert.Empty(validationResults);

            var user = new IdentityUser { Id = "Test01" };
            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            _projectServiceMock.Setup(s => s.CreateProjectAsync(It.IsAny<CreateProjectDto>()))
                .Returns(Task.CompletedTask);

            var controller = CreateController();

            // Act
            var result = await controller.Create(model);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);

            _projectServiceMock.Verify(s => s.CreateProjectAsync(It.Is<CreateProjectDto>(dto =>
                dto.Title == model.Title &&
                dto.Description == model.Description &&
                dto.Status == model.Status &&
                dto.CreatedBy == user.Id &&
                dto.StartDate == model.StartDate &&
                dto.EndDate == model.EndDate
            )), Times.Once);
        }

        [Fact]
        public async Task Details_ValidRequest_ReturnsViewWithProjectDetails()
        {
            // Arrange
            var projectId = 1;
            var userId = "user-123";
            var user = new IdentityUser { Id = userId };

            var projectMemberDto = new ProjectMemberDto
            {
                Id = 1,
                Role = ProjectRole.Admin,
                Name = "Test User",
                Email = "test@example.com"
            };

            var taskList = new List<TaskItem>
            {
                new TaskItem { Id = 1, Title = "Task 1", ProjectId = projectId }
            };

            var project = new Project
            {
                Id = projectId,
                Title = "Test Project",
                TaskItems = taskList
            };

            _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            _projectMemberServiceMock.Setup(p => p.GetUserProjectRoleAsync(userId, projectId))
                .ReturnsAsync(projectMemberDto);

            _projectServiceMock.Setup(p => p.GetProjectByIdAsync(projectId))
                .ReturnsAsync(project);

            _taskServiceMock.Setup(t => t.GetTaskListAsync(projectId))
                .ReturnsAsync(taskList);

            var controller = CreateController();

            // Act
            var result = await controller.Details(projectId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectDetailsViewModel>(viewResult.Model);
            Assert.Equal(projectId, model.Project!.Id);
            Assert.Single(model.Project.TaskItems);
        }

        [Fact]
        public async Task Details_UserIsNull_ReturnsUnauthorized()
        {
            // Arrange
            _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((IdentityUser?)null);

            var controller = CreateController();

            // Act
            var result = await controller.Details(1);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task Details_UserNotInProject_ReturnsForbid()
        {
            var user = new IdentityUser { Id = "user-123" };

            _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            _projectMemberServiceMock.Setup(p => p.GetUserProjectRoleAsync(user.Id, 1))
                .ReturnsAsync((ProjectMemberDto?)null);

            var controller = CreateController();

            // Act
            var result = await controller.Details(1);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task Details_ProjectNotFound_ReturnsNotFound()
        {
            var user = new IdentityUser { Id = "user-123" };

            _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            _projectMemberServiceMock.Setup(p => p.GetUserProjectRoleAsync(user.Id, 1))
                .ReturnsAsync(new ProjectMemberDto { Id = 1, Role = ProjectRole.Viewer });

            _projectServiceMock.Setup(p => p.GetProjectByIdAsync(1))
                .ReturnsAsync((Project?)null);

            var controller = CreateController();

            // Act
            var result = await controller.Details(1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Dashboard_UserIsNull_ReturnsUnauthorized()
        {
            // Arrange
            int projectId = 1;

            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync((IdentityUser?)null);

            var controller = CreateController();

            // Act
            var result = await controller.Dashboard(projectId);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task Dashboard_UserNotInProject_ReturnsForbid()
        {
            // Arrange
            int projectId = 1;
            var user = new IdentityUser { Id = "user1" };

            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            _projectMemberServiceMock.Setup(x => x.GetUserProjectRoleAsync(user.Id, projectId)).ReturnsAsync((ProjectMemberDto?)null);

            var controller = CreateController();

            // Act
            var result = await controller.Dashboard(projectId);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task Dashboard_ProjectNotFound_ReturnsNotFound()
        {
            // Arrange
            int projectId = 1;
            var user = new IdentityUser { Id = "user1" };
            var member = new ProjectMemberDto { Id = 1, Role = ProjectRole.Admin };

            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            _projectMemberServiceMock.Setup(x => x.GetUserProjectRoleAsync(user.Id, projectId)).ReturnsAsync(member);
            _projectServiceMock.Setup(x => x.GetProjectByIdAsync(projectId)).ReturnsAsync((Project?)null);

            var controller = CreateController();

            // Act
            var result = await controller.Dashboard(projectId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Dashboard_ModelStateInvalid_ReturnsView()
        {
            // Arrange
            int projectId = 1;
            var controller = CreateController();
            controller.ModelState.AddModelError("Test", "Invalid");

            // Act
            var result = await controller.Dashboard(projectId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.Model); // No model should be returned
        }

        [Fact]
        public async Task Dashboard_ValidRequest_ReturnsViewWithExpectedModel()
        {
            // Arrange
            int projectId = 1;
            var user = new IdentityUser
            {
                Id = "user3",
                Email = "test03@gmail.com",
                UserName = "testuser03"
            };

            var invitedUserProfile = new UserProfile
            {
                Id = 4,
                UserId = user.Id,
                User = user
            };

            var member = new ProjectMemberDto 
            { 
                Id = 1, 
                Role = ProjectRole.Admin 
            };

            var project = new Project
            {
                Id = projectId,
                Title = "Test Project",
                Description = "Desc",
                Status = ProjectStatus.NotStarted,
                StartDate = DateTime.UtcNow
            };
            project.SetEndDate(null);

            var tasks = new List<TaskItem>
            {
                new TaskItem
                {
                    Id = 1,
                    Title = "Task1",
                    Status = TaskWorkflowStatus.ToDo,
                    Priority = TaskPriority.Medium,
                    AssignedUsers = new List<TaskAssignment>
                    {
                        new TaskAssignment
                        {
                            UserProfile = new UserProfile
                            {
                                FullName = "Tester 02",
                                User = new IdentityUser
                                {
                                    UserName = "Name2",
                                    Email = "Email"
                                },
                                AvatarUrl = "Hello"
                            }
                        }
                    }
                },
                new TaskItem
                {
                    Id = 2,
                    Title = "Title 02",
                    Description = "Description",
                    Status = TaskWorkflowStatus.Done,
                    Priority = TaskPriority.Medium,
                    AssignedUsers = new List<TaskAssignment>
                    {
                        new TaskAssignment
                        {
                            UserProfile = new UserProfile
                            {
                                FullName = "Tester 01",
                                User = new IdentityUser
                                {
                                    UserName = "Name",
                                    Email = "Email"
                                },
                                AvatarUrl = "Hello"
                            }
                        }
                    }
                }
            };


            var members = new List<ProjectMemberDto>
            {
                new ProjectMemberDto { Id = 2, Name = "Test 01", Email = "test@gmail.com", Role = ProjectRole.Admin },
                new ProjectMemberDto { Id = 3, Name = "Test 02", Email = "test02@gmail.com", Role = ProjectRole.Contributor }
            };

            var invites = new PaginatedList<ProjectInvitation>(new List<ProjectInvitation>
            {
                new ProjectInvitation
                {
                    Id = 5,
                    ProjectId = projectId,
                    InvitedUserProfileId = invitedUserProfile.Id,
                    Project = project,
                    AssignedRole = ProjectRole.Admin,
                    Status = InvitationStatus.Pending,
                    InvitationSentDate = DateTime.UtcNow,
                    InvitedUserProfile = invitedUserProfile
                }
            }, 1, 1, 10);

            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            _projectMemberServiceMock.Setup(x => x.GetUserProjectRoleAsync(user.Id, projectId)).ReturnsAsync(member);
            _projectServiceMock.Setup(x => x.GetProjectByIdAsync(projectId)).ReturnsAsync(project);
            _taskServiceMock.Setup(x => x.GetTaskListAsync(projectId)).ReturnsAsync(tasks);
            _projectMemberServiceMock.Setup(x => x.GetProjectMembersAsync(projectId)).ReturnsAsync(members);
            _invitationServiceMock.Setup(x => x.GetInvitationListAsync(projectId, 1, 10)).ReturnsAsync(invites);

            var controller = CreateController();

            // Act
            var result = await controller.Dashboard(projectId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectDashboardViewModel>(viewResult.Model);

            Assert.Equal(projectId, model.ProjectId);
            Assert.Equal(project.Title, model.ProjectTitle);
            Assert.Equal(project.Description, model.ProjectDescription);
            Assert.Equal(ProjectStatus.NotStarted, model.ProjectStatus);
            Assert.Equal(project.StartDate.Date, model.StartDate.Date);
            Assert.Null(model.EndDate);
            Assert.Equal(ProjectRole.Admin, model.UserRoleInThisProject);

            Assert.Equal(2, model.TotalTasks);
            Assert.Equal(1, model.PendingTasks);
            Assert.Equal(1, model.CompletedTasks);

            Assert.Equal(2, model.Members.Count);
            Assert.Contains(model.Members, m => m.Email == "test@gmail.com");
            Assert.Contains(model.Members, m => m.Email == "test02@gmail.com");

            Assert.Single(model.Invitations);
            Assert.Equal("testuser03", model.Invitations.First().InvitedUserEmail);
            Assert.Equal(InvitationStatus.Pending, model.Invitations.First().Status);

            Assert.Equal(2, model.TaskItems.Count);
            var task = model.TaskItems.First();
            Assert.Equal("Task1", task.Title);
            Assert.Equal(TaskWorkflowStatus.ToDo, task.Status);
            Assert.Equal(TaskPriority.Medium, task.Priority);
            Assert.Single(task.AssignedUsers);
            Assert.Equal("Tester 02", task.AssignedUsers.First().FullName);

            var update = model.UpdateViewModel;
            Assert.Equal(1, update.Id);
            Assert.Equal("Test Project", update.Title);
            Assert.Equal("Desc", update.Description);
            Assert.Equal(ProjectStatus.NotStarted, update.Status);
        }

    }
}
