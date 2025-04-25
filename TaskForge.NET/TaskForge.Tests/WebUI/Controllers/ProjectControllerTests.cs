using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.CodeAnalysis;
using Moq;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using TaskForge.Application.Common.Model;
using TaskForge.Application.DTOs;
using TaskForge.Application.Interfaces.Services;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;
using TaskForge.WebUI.Models;
using TaskForge.WebUI.Controllers;
using Project = TaskForge.Domain.Entities.Project;
using Xunit;

namespace TaskForge.Tests.WebUI.Controllers
{
    public class ProjectControllerTests
    {
        private readonly Mock<IProjectService> _projectServiceMock = new();
        private readonly Mock<IProjectMemberService> _projectMemberServiceMock = new();
        private readonly Mock<ITaskService> _taskServiceMock = new();
        private readonly Mock<IProjectInvitationService> _invitationServiceMock = new();
        private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
        private readonly ProjectController _controller;

        public ProjectControllerTests()
        {
            var store = new Mock<IUserStore<IdentityUser>>();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            _userManagerMock = new Mock<UserManager<IdentityUser>>(store.Object,
                null, null, null, null, null, null, null, null);

            _controller = new ProjectController
            (
                _projectMemberServiceMock.Object,
                _projectServiceMock.Object,
                _taskServiceMock.Object,
                _invitationServiceMock.Object,
                _userManagerMock.Object
            );

            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            _controller.TempData = tempData;
        }


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

            var result = await _controller.Index(filter, 1, 10);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectListViewModel>(viewResult.Model);
            Assert.Equal(15, model.TotalItems);
            Assert.Equal(2, model?.TotalPages);
        }
        [Fact]
        public async Task Index_ModelStateInvalid_ReturnsRedirectToIndex()
        {
            _controller.ModelState.AddModelError("AnyKey", "Some error");

            var filter = new ProjectFilterDto();

            var result = await _controller.Index(filter);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }
        [Fact]
        public async Task Index_UserNull_ReturnsUnauthorized()
        {
            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                            .ReturnsAsync((IdentityUser?)null);

            var filter = new ProjectFilterDto();

            var result = await _controller.Index(filter);

            Assert.IsType<UnauthorizedResult>(result);
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

            var result = await _controller.Update(1);

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
        public async Task Update_ModelStateInvalid_ReturnsView()
        {
            _controller.ModelState.AddModelError("Id", "Invalid");

            var result = await _controller.Update(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
        }
        [Fact]
        public async Task Update_ProjectNotFound_ReturnsNotFound()
        {
            _projectServiceMock.Setup(p => p.GetProjectByIdAsync(1))
                .ReturnsAsync((Project?)null);

            var result = await _controller.Update(1);

            Assert.IsType<NotFoundResult>(result);
        }
        [Fact]
        public async Task Update_InvalidModelState_ReturnsPartialViewWithViewModel()
        {
            _controller.ModelState.AddModelError("Title", "Required");

            var viewModel = new ProjectUpdateViewModel { Title = "", Id = 1 };

            using var stringWriter = new StringWriter();
            var originalOut = Console.Out;
            Console.SetOut(stringWriter);

            try
            {
                var result = await _controller.Update(viewModel);

                var partialView = Assert.IsType<PartialViewResult>(result);
                Assert.Equal("_EditProjectForm", partialView.ViewName);
                var model = Assert.IsType<ProjectUpdateViewModel>(partialView.Model);
                Assert.Equal(viewModel, model);
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }
        [Fact]
        public async Task Update_UserIsNull_ReturnsUnauthorized()
        {
            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((IdentityUser?)null);

            var viewModel = new ProjectUpdateViewModel { Id = 1 };

            var result = await _controller.Update(viewModel);

            Assert.IsType<UnauthorizedResult>(result);
        }
        [Fact]
        public async Task Update_ProjectIsNull_ReturnsNotFound()
        {
            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new IdentityUser { Id = "user1" });

            _projectServiceMock.Setup(p => p.GetProjectByIdAsync(1))
                .ReturnsAsync((Project?)null);

            var viewModel = new ProjectUpdateViewModel { Id = 1 };

            var result = await _controller.Update(viewModel);

            Assert.IsType<NotFoundResult>(result);
        }
        [Fact]
        public async Task Update_ValidRequest_UpdatesProjectAndRedirects()
        {
            var user = new IdentityUser { Id = "user1" };

            var existingProject = new Project
            {
                Id = 1,
                Title = "Old Title",
                Description = "Old Desc",
                StartDate = DateTime.UtcNow.AddDays(-5),
                Status = ProjectStatus.NotStarted,
                UpdatedBy = "",
                UpdatedDate = DateTime.MinValue
            };

            var viewModel = new ProjectUpdateViewModel
            {
                Id = 1,
                Title = "Updated Title",
                Description = "Updated Desc",
                StartDate = DateTime.UtcNow,
                EndDateInput = DateTime.UtcNow.AddDays(3),
                Status = ProjectStatus.InProgress
            };

            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            _projectServiceMock.Setup(p => p.GetProjectByIdAsync(viewModel.Id))
                .ReturnsAsync(existingProject);

            _projectServiceMock.Setup(p => p.UpdateProjectAsync(It.IsAny<Project>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var result = await _controller.Update(viewModel);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Dashboard", redirect.ActionName);
            Assert.Equal("Project", redirect.ControllerName);
            Assert.NotNull(redirect.RouteValues);
            Assert.Equal(1, redirect.RouteValues["id"]);

            _projectServiceMock.Verify();
        }



        [Fact]
        public async Task Create_ReturnsViewWithDefaultModel()
        {
            var expectedStatusOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = "Not Started" },
                new SelectListItem { Value = "1", Text = "In Progress" },
                new SelectListItem { Value = "2", Text = "On Hold"},
                new SelectListItem { Value = "3", Text = "Completed" },
                new SelectListItem { Value = "4", Text = "Cancelled" },
            };

            _projectServiceMock.Setup(s => s.GetProjectStatusOptions())
                .ReturnsAsync(expectedStatusOptions);

            var result = await _controller.Create();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CreateProjectViewModel>(viewResult.Model);

            Assert.Equal("", model.Title);
            Assert.Equal(ProjectStatus.NotStarted, model.Status);
            Assert.Equal(expectedStatusOptions, model.StatusOptions);
            Assert.Equal(DateTime.Now.Date, model.StartDate.Date);
            Assert.Null(model.EndDate);
        }
        [Fact]
        public async Task Create_InvalidModelState_ReturnsViewWithStatusOptions()
        {
            // Arrange
            var viewModel = new CreateProjectViewModel
            {
                Title = "",
                Status = ProjectStatus.NotStarted,
                StartDate = DateTime.Now,
                EndDate = null
            };

            var expectedOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = "Not Started" },
                new SelectListItem { Value = "1", Text = "In Progress" },
                new SelectListItem { Value = "2", Text = "On Hold"},
                new SelectListItem { Value = "3", Text = "Completed" },
                new SelectListItem { Value = "4", Text = "Cancelled" },
            };

            _projectServiceMock.Setup(p => p.GetProjectStatusOptions())
                .ReturnsAsync(expectedOptions);

            _controller.ModelState.AddModelError("Title", "Required");

            // Act
            var result = await _controller.Create(viewModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CreateProjectViewModel>(viewResult.Model);
            Assert.Equal(expectedOptions, model.StatusOptions);
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

            // Act
            var result = await _controller.Create(model);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect?.ActionName);

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
        public void Validate_EndDateBeforeStartDate_ReturnsValidationError()
        {
            // Arrange
            var model = new CreateProjectViewModel
            {
                Title = "Test Project",
                Status = ProjectStatus.Completed,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddHours(-1) // Invalid: EndDate before StartDate
            };

            // Act
            var results = model.Validate(new ValidationContext(model)).ToList();

            // Assert
            Assert.Single(results); // Should return exactly 1 error
            Assert.Equal("EndDate must be greater than StartDate", results[0].ErrorMessage);
            Assert.Contains(nameof(CreateProjectViewModel.EndDate), results[0].MemberNames);
        }
        [Fact]
        public async Task Create_Post_UserIsNull_ReturnsUnauthorized()
        {
            // Arrange
            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((IdentityUser?)null);

            var model = new CreateProjectViewModel
            {
                Title = "Test Project",
                Status = ProjectStatus.Completed,
                StartDate = new DateTime(2023, 10, 24, 0, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2023, 10, 26, 0, 0, 0, DateTimeKind.Utc)
            };

            // Act
            var result = await _controller.Create(model);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }



        [Fact]
        public async Task Dashboard_UserIsNull_ReturnsUnauthorized()
        {
            // Arrange
            int projectId = 1;

            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync((IdentityUser?)null);

            // Act
            var result = await _controller.Dashboard(projectId);

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

            // Act
            var result = await _controller.Dashboard(projectId);

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

            // Act
            var result = await _controller.Dashboard(projectId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
        [Fact]
        public async Task Dashboard_ModelStateInvalid_ReturnsView()
        {
            // Arrange
            int projectId = 1;
            _controller.ModelState.AddModelError("Test", "Invalid");

            // Act
            var result = await _controller.Dashboard(projectId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.Model);
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

            var invitedUserProfile1 = new UserProfile
            {
                Id = 4,
                UserId = user.Id,
                User = user
            };

            var invitedUserProfile2 = new UserProfile
            {
                Id = 5,
                UserId = "user4",
                User = new IdentityUser
                {
                    UserName = "user4",
                    Email = "user4@email.com"
                }
            };

            var member1 = new ProjectMemberDto
            {
                Id = 2,
                Name = "Alice",
                Email = "alice@email.com",
                Role = ProjectRole.Admin
            };

            var member2 = new ProjectMemberDto
            {
                Id = 3,
                Name = "Bob",
                Email = "bob@email.com",
                Role = ProjectRole.Contributor
            };

            var project = new Project
            {
                Id = projectId,
                Title = "Sample Project",
                Description = "Sample description",
                Status = ProjectStatus.InProgress,
                StartDate = DateTime.UtcNow.AddDays(-10)
            };
            project.SetEndDate(null);

            var task1 = new TaskItem
            {
                Id = 10,
                Title = "Task A",
                Description = "Do A",
                ProjectId = projectId,
                Status = TaskWorkflowStatus.ToDo,
                Priority = TaskPriority.Low,
                AssignedUsers = new List<TaskAssignment>
                {
                    new TaskAssignment
                    {
                        UserProfile = new UserProfile
                        {
                            FullName = "Charlie",
                            User = new IdentityUser { UserName = "charlie", Email = "charlie@email.com" },
                            AvatarUrl = "avatar1.png"
                        }
                    }
                }
            };

            var task2 = new TaskItem
            {
                Id = 11,
                Title = "Task B",
                Description = "Do B",
                ProjectId = projectId,
                Status = TaskWorkflowStatus.InProgress,
                Priority = TaskPriority.High,
                AssignedUsers = new List<TaskAssignment>
                {
                    new TaskAssignment
                    {
                        UserProfile = new UserProfile
                        {
                            FullName = "Dana",
                            User = new IdentityUser { UserName = "dana", Email = "dana@email.com" },
                            AvatarUrl = "avatar2.png"
                        }
                    }
                }
            };

            var invitation1 = new ProjectInvitation
            {
                Id = 20,
                ProjectId = projectId,
                AssignedRole = ProjectRole.Admin,
                Status = InvitationStatus.Pending,
                InvitationSentDate = DateTime.UtcNow.AddDays(-3),
                InvitedUserProfileId = invitedUserProfile1.Id,
                InvitedUserProfile = invitedUserProfile1,
                Project = project
            };

            var invitation2 = new ProjectInvitation
            {
                Id = 21,
                ProjectId = projectId,
                AssignedRole = ProjectRole.Contributor,
                Status = InvitationStatus.Accepted,
                InvitationSentDate = DateTime.UtcNow.AddDays(-2),
                InvitedUserProfileId = invitedUserProfile2.Id,
                InvitedUserProfile = invitedUserProfile2,
                Project = project
            };

            var member = new ProjectMemberDto { Id = 1, Role = ProjectRole.Admin };

            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            _projectMemberServiceMock.Setup(x => x.GetUserProjectRoleAsync(user.Id, projectId))
                .ReturnsAsync(member);
            _projectServiceMock.Setup(x => x.GetProjectByIdAsync(projectId))
                .ReturnsAsync(project);
            _taskServiceMock.Setup(x => x.GetTaskListAsync(projectId))
                .ReturnsAsync(new List<TaskItem> { task1, task2 });
            _projectMemberServiceMock.Setup(x => x.GetProjectMembersAsync(projectId))
                .ReturnsAsync(new List<ProjectMemberDto> { member1, member2 });

            var invitations = new List<ProjectInvitation> { invitation1, invitation2 };

            var paginatedInvites = new PaginatedList<ProjectInvitation>(
                invitations, invitations.Count, 1, 10);
            _invitationServiceMock
                .Setup(x => x.GetInvitationListAsync(projectId, 1, 10))
                .ReturnsAsync(paginatedInvites);


            // Act
            var result = await _controller.Dashboard(projectId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectDashboardViewModel>(viewResult.Model);

            Assert.Equal(projectId, model.ProjectId);
            Assert.Equal("Sample Project", model.ProjectTitle);
            Assert.Equal("Sample description", model.ProjectDescription);
            Assert.Equal(ProjectStatus.InProgress, model.ProjectStatus);
            Assert.Equal(ProjectRole.Admin, model.UserRoleInThisProject);
            Assert.Equal(project.StartDate.Date, model.StartDate.Date);
            Assert.Null(model.EndDate);

            Assert.Equal(2, model.Members.Count);
            Assert.Contains(model.Members, m => m.Id == member1.Id && m.Name == member1.Name &&
                m.Email == member1.Email && m.Role == member1.Role);
            Assert.Contains(model.Members, m => m.Id == member2.Id && m.Name == member2.Name &&
                m.Email == member2.Email && m.Role == member2.Role);

            Assert.Equal(2, model.Invitations.Count);

            Assert.Contains(model.Invitations, i =>
                i.Id == invitation1.Id &&
                i.ProjectId == invitation1.ProjectId &&
                i.InvitedUserEmail == invitedUserProfile1.User.UserName &&
                i.Status == invitation1.Status &&
                i.AssignedRole == invitation1.AssignedRole &&
                i.InvitationSentDate.Date == invitation1.InvitationSentDate.Date);

            Assert.Contains(model.Invitations, i =>
                i.Id == invitation2.Id &&
                i.ProjectId == invitation2.ProjectId &&
                i.InvitedUserEmail == invitedUserProfile2.User.UserName &&
                i.Status == invitation2.Status &&
                i.AssignedRole == invitation2.AssignedRole &&
                i.InvitationSentDate.Date == invitation2.InvitationSentDate.Date);

            Assert.Equal(2, model.TaskItems.Count);
            Assert.Contains(model.TaskItems, t =>
                t.Id == task1.Id &&
                t.Title == task1.Title &&
                t.Description == task1.Description &&
                t.Status == task1.Status &&
                t.Priority == task1.Priority &&
                t.AssignedUsers.Any(u =>
                    u.FullName == "Charlie" &&
                    u.UserName == "charlie" &&
                    u.Email == "charlie@email.com" &&
                    u.AvatarUrl == "avatar1.png"));

            Assert.Contains(model.TaskItems, t =>
                t.Id == task2.Id &&
                t.Title == task2.Title &&
                t.Description == task2.Description &&
                t.Status == task2.Status &&
                t.Priority == task2.Priority &&
                t.AssignedUsers.Any(u =>
                    u.FullName == "Dana" &&
                    u.UserName == "dana" &&
                    u.Email == "dana@email.com" &&
                    u.AvatarUrl == "avatar2.png"));

            var update = model.UpdateViewModel;
            Assert.NotNull(update);
            Assert.Equal(projectId, update.Id);
            Assert.Equal("Sample Project", update.Title);
            Assert.Equal("Sample description", update.Description);
            Assert.Equal(ProjectStatus.InProgress, update.Status);
            Assert.Null(update.EndDateInput);
        }
        [Fact]
        public async Task Dashboard_MemberAndInvitation_NullValues_AreHandledGracefully()
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
                User = null
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
                Status = ProjectStatus.NotStarted,
                StartDate = DateTime.UtcNow
            };
            project.SetEndDate(null);

            var tasks = new List<TaskItem>();

            var members = new List<ProjectMemberDto>
            {
                new ProjectMemberDto { Id = 2, Name = null, Email = null, Role = ProjectRole.Admin }
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

            // Act
            var result = await _controller.Dashboard(projectId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectDashboardViewModel>(viewResult.Model);

            var memberVm = model.Members.FirstOrDefault();
            Assert.NotNull(memberVm);
            Assert.Equal("", memberVm.Name);
            Assert.Equal("", memberVm.Email);

            var inviteVm = model.Invitations.FirstOrDefault();
            Assert.NotNull(inviteVm);
            Assert.Equal("No User", inviteVm.InvitedUserEmail);
        }



        [Fact]
        public async Task ManageMembers_ValidAdminUser_ReturnsViewWithExpectedModel()
        {
            // Arrange
            var projectId = 1;
            var pageIndex = 1;
            var pageSize = 10;
            var time = new DateTime(2023, 10, 24, 0, 0, 0, DateTimeKind.Utc);

            var user = new IdentityUser
            {
                Id = "user3",
                Email = "test03@gmail.com",
                UserName = "testuser03"
            };
            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var member1 = new ProjectMemberDto
            {
                Id = 1,
                Name = "John Doe",
                Email = "john@example.com",
                Role = ProjectRole.Admin
            };
            var member2 = new ProjectMemberDto
            {
                Id = 2,
                Name = null,
                Email = null,
                Role = ProjectRole.Admin
            };
            _projectMemberServiceMock.Setup(x => x.GetUserProjectRoleAsync(user.Id, projectId))
                .ReturnsAsync(member1);

            var project = new Project
            {
                Id = projectId,
                Title = "Test Project",
                Description = "Desc",
            };
            _projectServiceMock.Setup(s => s.GetProjectByIdAsync(projectId))
                .ReturnsAsync(project);

            var projectMembers = new List<ProjectMemberDto> { member1, member2 };
            _projectMemberServiceMock.Setup(s => s.GetProjectMembersAsync(projectId))
                .ReturnsAsync(projectMembers);

            var invitation1 = new ProjectInvitation
            {
                Id = 1,
                ProjectId = projectId,
                Project = project,
                InvitedUserProfile = new UserProfile
                {
                    User = new IdentityUser { Id = "test01", Email = "test01@gmail" }
                },
                Status = InvitationStatus.Pending,
                InvitationSentDate = time,
                AssignedRole = ProjectRole.Admin
            };
            var invitation2 = new ProjectInvitation
            {
                Id = 2,
                ProjectId = projectId,
                Project = project,
                InvitedUserProfile = new UserProfile
                {
                    User = new IdentityUser { Id = "test02", Email = "test02@gmail" }
                },
                Status = InvitationStatus.Accepted,
                InvitationSentDate = time.AddHours(1),
                AssignedRole = ProjectRole.Viewer
            };
            var invitations = new List<ProjectInvitation> { invitation1, invitation2 };
            var paginatedInvitations = new PaginatedList<ProjectInvitation>(invitations, invitations.Count, pageIndex, pageSize);
            _invitationServiceMock.Setup(s => s.GetInvitationListAsync(projectId, pageIndex, pageSize))
                .ReturnsAsync(paginatedInvitations);

            // Act
            var result = await _controller.ManageMembers(projectId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ManageMembersViewModel>(viewResult.Model);

            Assert.Equal(projectId, model.ProjectId);
            Assert.Equal(project.Title, model.ProjectTitle);
            Assert.Equal(project.Description, model.ProjectDescription);

            Assert.NotNull(model.ProjectMembers);
            Assert.Collection(model.ProjectMembers,
                m =>
                {
                    Assert.Equal(member1.Id, m.Id);
                    Assert.Equal("John Doe", m.Name);
                    Assert.Equal("john@example.com", m.Email);
                    Assert.Equal(ProjectRole.Admin, m.Role);
                },
                m =>
                {
                    Assert.Equal(member2.Id, m.Id);
                    Assert.Equal("Unknown", m.Name); // Name was null
                    Assert.Equal("N/A", m.Email);    // Email was null
                    Assert.Equal(ProjectRole.Admin, m.Role);
                });


            var vmMember1 = model.ProjectMembers.FirstOrDefault(m => m.Id == member1.Id);
            Assert.NotNull(vmMember1);
            Assert.Equal(member1.Id, vmMember1!.Id);
            Assert.Equal(member1.Name, vmMember1.Name);
            Assert.Equal(member1.Email, vmMember1.Email);
            Assert.Equal(member1.Role, vmMember1.Role);

            var vmMember2 = model.ProjectMembers.FirstOrDefault(m => m.Id == member2.Id);
            Assert.NotNull(vmMember2);
            Assert.Equal(member2.Id, vmMember2!.Id);
            Assert.Equal("Unknown", vmMember2.Name);
            Assert.Equal("N/A", vmMember2.Email);
            Assert.Equal(member2.Role, vmMember2.Role);

            Assert.NotNull(model.ProjectInvitations);
            Assert.Collection(model.ProjectInvitations.Items,
                i =>
                {
                    Assert.Equal(invitation1.Id, i.Id);
                    Assert.Equal(projectId, i.ProjectId);
                    Assert.Equal("test01@gmail", i.InvitedUserEmail);
                    Assert.Equal(InvitationStatus.Pending, i.Status);
                    Assert.Equal(time, i.InvitationSentDate);
                    Assert.Equal(ProjectRole.Admin, i.AssignedRole);
                },
                i =>
                {
                    Assert.Equal(invitation2.Id, i.Id);
                    Assert.Equal(projectId, i.ProjectId);
                    Assert.Equal("test02@gmail", i.InvitedUserEmail);
                    Assert.Equal(InvitationStatus.Accepted, i.Status);
                    Assert.Equal(time.AddHours(1), i.InvitationSentDate);
                    Assert.Equal(ProjectRole.Viewer, i.AssignedRole);
                });
            Assert.Equal(invitations.Count, model.ProjectInvitations.TotalCount);
            Assert.Equal(pageIndex, model.ProjectInvitations.PageIndex);
            Assert.Equal(pageSize, model.ProjectInvitations.PageSize);

            var invite1 = model.ProjectInvitations.Items.FirstOrDefault(i => i.Id == invitation1.Id);
            Assert.NotNull(invite1);
            Assert.Equal(invitation1.Id, invite1!.Id);
            Assert.Equal(invitation1.ProjectId, invite1.ProjectId);
            Assert.Equal("test01@gmail", invite1.InvitedUserEmail);
            Assert.Equal(invitation1.Status, invite1.Status);
            Assert.Equal(invitation1.InvitationSentDate, invite1.InvitationSentDate);
            Assert.Equal(invitation1.AssignedRole, invite1.AssignedRole);

            var invite2 = model.ProjectInvitations.Items.FirstOrDefault(i => i.Id == invitation2.Id);
            Assert.NotNull(invite2);
            Assert.Equal(invitation2.Id, invite2!.Id);
            Assert.Equal(invitation2.ProjectId, invite2.ProjectId);
            Assert.Equal("test02@gmail", invite2.InvitedUserEmail);
            Assert.Equal(invitation2.Status, invite2.Status);
            Assert.Equal(invitation2.InvitationSentDate, invite2.InvitationSentDate);
            Assert.Equal(invitation2.AssignedRole, invite2.AssignedRole);
        }
        [Fact]
        public async Task ManageMembers_ModelStateInvalid_ReturnsView()
        {
            int projectId = 1;
            _controller.ModelState.AddModelError("Test", "Invalid");

            var result = await _controller.ManageMembers(projectId);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.Model);
        }
        [Fact]
        public async Task ManageMembers_UserIsNull_ReturnsUnauthorized()
        {
            int projectId = 1;
            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync((IdentityUser?)null);

            var result = await _controller.ManageMembers(projectId);

            Assert.IsType<UnauthorizedResult>(result);
        }
        [Fact]
        public async Task ManageMembers_UserNotAdmin_ReturnsForbid()
        {
            int projectId = 1;
            var user = new IdentityUser { Id = "user1" };

            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            _projectMemberServiceMock.Setup(x => x.GetUserProjectRoleAsync(user.Id, projectId)).ReturnsAsync(new ProjectMemberDto { Role = ProjectRole.Contributor });

            var result = await _controller.ManageMembers(projectId);

            Assert.IsType<ForbidResult>(result);
        }
        [Fact]
        public async Task ManageMembers_ProjectNotFound_ReturnsNotFound()
        {
            int projectId = 1;
            var user = new IdentityUser { Id = "user1" };
            var memberDto = new ProjectMemberDto { Id = 1, Role = ProjectRole.Admin };

            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            _projectMemberServiceMock.Setup(x => x.GetUserProjectRoleAsync(user.Id, projectId)).ReturnsAsync(memberDto);
            _projectServiceMock.Setup(x => x.GetProjectByIdAsync(projectId)).ReturnsAsync((Project?)null);

            var result = await _controller.ManageMembers(projectId);

            Assert.IsType<NotFoundResult>(result);
        }
        [Fact]
        public async Task ManageMembers_NoProjectMembers_ReturnsViewWithEmptyList()
        {
            int projectId = 1;
            var user = new IdentityUser { Id = "user1" };
            var memberDto = new ProjectMemberDto { Id = 1, Role = ProjectRole.Admin };
            var project = new Project { Id = projectId, Title = "Test Project", Description = "" };

            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            _projectMemberServiceMock.Setup(x => x.GetUserProjectRoleAsync(user.Id, projectId)).ReturnsAsync(memberDto);
            _projectServiceMock.Setup(x => x.GetProjectByIdAsync(projectId)).ReturnsAsync(project);
            _projectMemberServiceMock.Setup(x => x.GetProjectMembersAsync(projectId)).ReturnsAsync(new List<ProjectMemberDto>());
            _invitationServiceMock.Setup(x => x.GetInvitationListAsync(projectId, 1, 10)).ReturnsAsync(new PaginatedList<ProjectInvitation>(new List<ProjectInvitation>(), 0, 1, 10));

            var result = await _controller.ManageMembers(projectId) as ViewResult;

            var model = Assert.IsType<ManageMembersViewModel>(result?.Model);
            Assert.Empty(model.ProjectMembers);
            Assert.Empty(model.ProjectInvitations.Items);
        }
        [Theory]
        [InlineData(null)]
        [InlineData(ProjectRole.Contributor)]
        [InlineData(ProjectRole.Viewer)]
        public async Task ManageMembers_WhenUserNotAdminOrNotInProject_ReturnsForbid(ProjectRole? role)
        {
            int projectId = 1;
            var user = new IdentityUser { Id = "user1" };

            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            ProjectMemberDto? member = role.HasValue
                ? new ProjectMemberDto { Role = role.Value }
                : null;

            _projectMemberServiceMock.Setup(x => x.GetUserProjectRoleAsync(user.Id, projectId))
                .ReturnsAsync(member);

            var result = await _controller.ManageMembers(projectId);

            Assert.IsType<ForbidResult>(result);
        }



        [Fact]
        public async Task RemoveMember_AllBranchesCovered()
        {
            // Arrange
            int memberId = 1;
            int projectId = 100;
            string userId = "user1";

            var user = new IdentityUser { Id = userId, UserName = "admin@test.com" };

            var memberToRemove = new ProjectMember
            {
                Id = memberId,
                ProjectId = projectId,
                Role = ProjectRole.Contributor
            };

            var adminMember = new ProjectMemberDto
            {
                Id = 2,
                ProjectId = projectId,
                Role = ProjectRole.Admin
            };

            _projectMemberServiceMock.Setup(s => s.GetByIdAsync(memberId))
                .ReturnsAsync(memberToRemove);

            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            _projectMemberServiceMock.Setup(s => s.GetUserProjectRoleAsync(user.Id, projectId))
                .ReturnsAsync(adminMember);

            _projectMemberServiceMock.Setup(s => s.RemoveAsync(memberId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.RemoveMember(memberId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ManageMembers", redirectResult.ActionName);
            Assert.Equal("Project", redirectResult.ControllerName);
            Assert.NotNull(redirectResult.RouteValues);
            Assert.Equal(projectId, redirectResult.RouteValues["Id"]);
        }
        [Fact]
        public async Task RemoveMember_ModelStateInvalid_ReturnsView()
        {
            _controller.ModelState.AddModelError("FakeError", "Invalid");

            var result = await _controller.RemoveMember(1);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Null(view.Model);
        }
        [Fact]
        public async Task RemoveMember_MemberNotFound_ReturnsNotFound()
        {
            _projectMemberServiceMock.Setup(s => s.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((ProjectMember?)null);

            var result = await _controller.RemoveMember(999);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Member not found.", notFound.Value);
        }
        [Fact]
        public async Task RemoveMember_UserNull_ReturnsUnauthorized()
        {
            _projectMemberServiceMock.Setup(s => s.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new ProjectMember { Id = 1, ProjectId = 1 });

            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((IdentityUser?)null);

            var result = await _controller.RemoveMember(1);

            Assert.IsType<UnauthorizedResult>(result);
        }
        [Fact]
        public async Task RemoveMember_UserNotMember_ReturnsForbidden()
        {
            _projectMemberServiceMock.Setup(s => s.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new ProjectMember { Id = 1, ProjectId = 1 });

            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new IdentityUser { Id = "user1" });

            _projectMemberServiceMock.Setup(s => s.GetUserProjectRoleAsync("user1", 1))
                .ReturnsAsync((ProjectMemberDto?)null);

            var result = await _controller.RemoveMember(1);

            Assert.IsType<ForbidResult>(result);
        }
        [Fact]
        public async Task RemoveMember_UserNotAdmin_ReturnsForbidden()
        {
            _projectMemberServiceMock.Setup(s => s.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new ProjectMember { Id = 1, ProjectId = 1 });

            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new IdentityUser { Id = "user1" });

            _projectMemberServiceMock.Setup(s => s.GetUserProjectRoleAsync("user1", 1))
                .ReturnsAsync(new ProjectMemberDto { Id = 2, ProjectId = 1, Role = ProjectRole.Contributor });

            var result = await _controller.RemoveMember(1);

            Assert.IsType<ForbidResult>(result);
        }
        [Fact]
        public async Task RemoveMember_SelfRemoval_ReturnsBadRequest()
        {
            var member = new ProjectMember { Id = 1, ProjectId = 1, Role = ProjectRole.Admin };

            _projectMemberServiceMock.Setup(s => s.GetByIdAsync(1))
                .ReturnsAsync(member);

            var user = new IdentityUser { Id = "user1" };
            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            _projectMemberServiceMock.Setup(s => s.GetUserProjectRoleAsync(user.Id, member.ProjectId))
                .ReturnsAsync(new ProjectMemberDto
                {
                    Id = 1,
                    ProjectId = 1,
                    Role = ProjectRole.Admin
                });

            var result = await _controller.RemoveMember(1);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid request.", badRequest.Value);
        }



        [Fact]
        public async Task CancelInvitation_ModelStateInvalid_ReturnsBadRequest()
        {
            _controller.ModelState.AddModelError("Id", "Required");

            var result = await _controller.CancelInvitation(1);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsType<SerializableError>(badRequest.Value);
        }
        [Fact]
        public async Task CancelInvitation_InvitationNotFound_ReturnsNotFound()
        {
            _invitationServiceMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync((ProjectInvitation?)null);

            var result = await _controller.CancelInvitation(1);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Invitation not found.", notFound.Value);
        }
        [Fact]
        public async Task CancelInvitation_UserIsNull_ReturnsUnauthorized()
        {
            _invitationServiceMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(new ProjectInvitation { ProjectId = 99, Status = InvitationStatus.Pending });

            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((IdentityUser?)null);

            var result = await _controller.CancelInvitation(1);

            Assert.IsType<UnauthorizedResult>(result);
        }
        [Fact]
        public async Task CancelInvitation_UserNotAdmin_ReturnsForbid()
        {
            var invitation = new ProjectInvitation { ProjectId = 99, Status = InvitationStatus.Pending };

            _invitationServiceMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(invitation);

            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new IdentityUser { Id = "user1" });

            _projectMemberServiceMock.Setup(x => x.GetUserProjectRoleAsync("user1", 99))
                .ReturnsAsync(new ProjectMemberDto { Role = ProjectRole.Contributor });

            var result = await _controller.CancelInvitation(1);

            Assert.IsType<ForbidResult>(result);
        }
        [Theory]
        [InlineData(InvitationStatus.Accepted)]
        [InlineData(InvitationStatus.Declined)]
        [InlineData(InvitationStatus.Canceled)]
        public async Task CancelInvitation_AlreadyProcessed_ReturnsBadRequest(InvitationStatus status)
        {
            var invitation = new ProjectInvitation { Id = 1, ProjectId = 101, Status = status };

            _invitationServiceMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(invitation);

            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new IdentityUser { Id = "user1" });

            _projectMemberServiceMock.Setup(x => x.GetUserProjectRoleAsync("user1", 101))
                .ReturnsAsync(new ProjectMemberDto { Role = ProjectRole.Admin });

            var result = await _controller.CancelInvitation(1);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal($"Cannot cancel an invitation that is already {status.ToString().ToLower()}.", badRequest.Value);
        }
        [Fact]
        public async Task CancelInvitation_Valid_ReturnsRedirectToManageMembers()
        {
            var invitation = new ProjectInvitation
            {
                Id = 1,
                ProjectId = 101,
                Status = InvitationStatus.Pending
            };

            _invitationServiceMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(invitation);

            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new IdentityUser { Id = "user1" });

            _projectMemberServiceMock.Setup(x => x.GetUserProjectRoleAsync("user1", 101))
                .ReturnsAsync(new ProjectMemberDto { Role = ProjectRole.Admin });

            _invitationServiceMock.Setup(x => x.UpdateInvitationStatusAsync(1, InvitationStatus.Canceled))
                .Returns(Task.CompletedTask);

            var result = await _controller.CancelInvitation(1);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ManageMembers", redirect.ActionName);
            Assert.Equal("Project", redirect.ControllerName);
            Assert.NotNull(redirect.RouteValues);
            Assert.Equal(101, redirect.RouteValues["Id"]);
        }
        [Fact]
        public async Task CancelInvitation_UserNotMember_ReturnsForbidden()
        {
            int invitationId = 1;
            var user = new IdentityUser { Id = "user1" };
            var invitation = new ProjectInvitation
            {
                Id = invitationId,
                ProjectId = 1,
                Status = InvitationStatus.Pending
            };

            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            _invitationServiceMock.Setup(x => x.GetByIdAsync(invitationId)).ReturnsAsync(invitation);
            _projectMemberServiceMock.Setup(x => x.GetUserProjectRoleAsync(user.Id, invitation.ProjectId))
                .ReturnsAsync((ProjectMemberDto?)null);

            var result = await _controller.CancelInvitation(invitationId);

            Assert.IsType<ForbidResult>(result);
        }
        [Fact]
        public async Task CancelInvitation_UserNotAdmin_ReturnsForbidden()
        {
            int invitationId = 1;
            var user = new IdentityUser { Id = "user1" };
            var invitation = new ProjectInvitation
            {
                Id = invitationId,
                ProjectId = 1,
                Status = InvitationStatus.Pending
            };

            var nonAdminMember = new ProjectMemberDto
            {
                Id = 1,
                Role = ProjectRole.Contributor
            };

            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            _invitationServiceMock.Setup(x => x.GetByIdAsync(invitationId)).ReturnsAsync(invitation);
            _projectMemberServiceMock.Setup(x => x.GetUserProjectRoleAsync(user.Id, invitation.ProjectId))
                .ReturnsAsync(nonAdminMember);

            var result = await _controller.CancelInvitation(invitationId);

            Assert.IsType<ForbidResult>(result);
        }

    }
}
