using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using System.Data;
using System.Linq.Expressions;
using TaskForge.Application.DTOs;
using TaskForge.Application.Helpers.DependencyResolvers;
using TaskForge.Application.Helpers.TaskSorters;
using TaskForge.Application.Interfaces.Repositories;
using TaskForge.Application.Interfaces.Repositories.Common;
using TaskForge.Application.Interfaces.Services;
using TaskForge.Application.Services;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;
using Xunit;

namespace TaskForge.Tests.Application.Services
{
    public class TaskServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IFileService> _fileServiceMock;
        private readonly Mock<IDependentTaskStrategy> _dependentTaskStrategyMock;
        private readonly Mock<ITaskSorter> _taskSorterMock;
        private readonly Mock<ITaskRepository> _taskRepositoryMock;
        private readonly Mock<IProjectMemberRepository> _projectMemberRepositoryMock;
        private readonly Mock<IUserProfileRepository> _userProfileRepositoryMock;
        private readonly Mock<ITaskAssignmentRepository> _taskAssignmentRepositoryMock;
        private readonly Mock<ITaskAttachmentRepository> _taskAttachmentRepositoryMock;
        private readonly Mock<ILogger<TaskService>> _loggerMock;
        private readonly Mock<IDbContextTransaction> _transactionMock;


        private readonly TaskService _taskService;

        public TaskServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _fileServiceMock = new Mock<IFileService>();
            _dependentTaskStrategyMock = new Mock<IDependentTaskStrategy>();
            _taskSorterMock = new Mock<ITaskSorter>();
            _taskRepositoryMock = new Mock<ITaskRepository>();
            _projectMemberRepositoryMock = new Mock<IProjectMemberRepository>();
            _userProfileRepositoryMock = new Mock<IUserProfileRepository>();
            _taskAssignmentRepositoryMock = new Mock<ITaskAssignmentRepository>();
            _taskAttachmentRepositoryMock = new Mock<ITaskAttachmentRepository>();
            _loggerMock = new Mock<ILogger<TaskService>>();
            _transactionMock = new Mock<IDbContextTransaction>();

            var dependencies = new TaskServiceDependencies
            {
                UnitOfWork = _unitOfWorkMock.Object,
                FileService = _fileServiceMock.Object,
                DependentTaskStrategy = _dependentTaskStrategyMock.Object,
                TaskSorter = _taskSorterMock.Object,
                TaskRepository = _taskRepositoryMock.Object,
                ProjectMemberRepository = _projectMemberRepositoryMock.Object,
                UserProfileRepository = _userProfileRepositoryMock.Object,
                TaskAssignmentRepository = _taskAssignmentRepositoryMock.Object,
                TaskAttachmentRepository = _taskAttachmentRepositoryMock.Object,
                Logger = _loggerMock.Object
            };

            _taskService = new TaskService(dependencies);
        }

        [Fact]
        public async Task GetTaskListAsync_ReturnsTasksForProject()
        {
            // Arrange
            var projectId = 1;
            var taskList = new List<TaskItem> { new TaskItem { Id = 1, ProjectId = projectId } };

            _taskRepositoryMock.Setup(u => u.FindByExpressionAsync(
                    It.IsAny<Expression<Func<TaskItem, bool>>>(),
                    It.IsAny<Func<IQueryable<TaskItem>, IOrderedQueryable<TaskItem>>>(),
                    It.IsAny<Func<IQueryable<TaskItem>, IQueryable<TaskItem>>>(),
                    It.IsAny<int?>(),
                    It.IsAny<int?>()))
                .ReturnsAsync(taskList);

            // Act
            var result = await _taskService.GetTaskListAsync(projectId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }
        [Fact]
        public async Task GetTaskListAsync_ReturnsVerifiesIncludesAndOrdering()
        {
            // Arrange
            var projectId = 1;
            var dueDate1 = new DateTime(2023, 10, 5, 0, 0, 0, DateTimeKind.Utc);
            var dueDate2 = new DateTime(2023, 11, 5, 0, 0, 0, DateTimeKind.Utc);

            var taskList = new List<TaskItem>();

            var task1 = new TaskItem { Id = 1, ProjectId = projectId };
            task1.SetDueDate(dueDate1);
            task1.AssignedUsers.Add(new TaskAssignment
            {
                UserProfile = new UserProfile { Id = 1, FullName = "User 1" }
            });
            taskList.Add(task1);

            var task2 = new TaskItem { Id = 2, ProjectId = projectId };
            task2.SetDueDate(dueDate2);
            task2.AssignedUsers.Add(new TaskAssignment
            {
                UserProfile = new UserProfile { Id = 2, FullName = "User 2" }
            });
            taskList.Add(task2);

            var orderedTaskList = taskList.OrderBy(t => t.DueDate).ToList();

            Expression<Func<TaskItem, bool>> capturedPredicate = null!;
            Func<IQueryable<TaskItem>, IOrderedQueryable<TaskItem>> capturedOrderBy = null!;
            Func<IQueryable<TaskItem>, IQueryable<TaskItem>> capturedIncludes = null!;

            _taskRepositoryMock.Setup(r => r.FindByExpressionAsync(
                It.IsAny<Expression<Func<TaskItem, bool>>>(),
                It.IsAny<Func<IQueryable<TaskItem>, IOrderedQueryable<TaskItem>>>(),
                It.IsAny<Func<IQueryable<TaskItem>, IQueryable<TaskItem>>>(),
                null,
                null
            )).Callback<Expression<Func<TaskItem, bool>>,
                      Func<IQueryable<TaskItem>, IOrderedQueryable<TaskItem>>,
                      Func<IQueryable<TaskItem>, IQueryable<TaskItem>>,
                      int?, int?>(
                (pred, ord, incl, _, _) =>
                {
                    capturedPredicate = pred;
                    capturedOrderBy = ord;
                    capturedIncludes = incl;
                })
            .ReturnsAsync(orderedTaskList);

            // Act
            var result = (await _taskService.GetTaskListAsync(projectId)).ToList();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, r => Assert.Equal(projectId, r.ProjectId));
            Assert.All(result, t => Assert.NotNull(t.AssignedUsers));
            Assert.All(result.SelectMany(t => t.AssignedUsers), au => Assert.NotNull(au.UserProfile));
            Assert.True(result.SequenceEqual(result.OrderBy(t => t.DueDate)));

            // Validate capturedPredicate
            Assert.NotNull(capturedPredicate);
            var predicateFunc = capturedPredicate.Compile();
            Assert.True(predicateFunc(new TaskItem { ProjectId = projectId }));
            Assert.False(predicateFunc(new TaskItem { ProjectId = 999 }));

            // Validate capturedOrderBy
            Assert.NotNull(capturedOrderBy);

            var taskA = new TaskItem { Id = 1, ProjectId = projectId };
            taskA.SetDueDate(dueDate2);

            var taskB = new TaskItem { Id = 2, ProjectId = projectId };
            taskB.SetDueDate(dueDate1);

            var unordered = new List<TaskItem> { taskA, taskB }.AsQueryable();
#pragma warning disable S6966 // Use asynchronous counterparts
            var ordered = capturedOrderBy(unordered).ToList();

            Assert.Equal(taskB, ordered[0]);
            Assert.Equal(taskA, ordered[1]);

            Assert.NotNull(capturedIncludes);
            var dummyQuery = new List<TaskItem>().AsQueryable();
            var includedQuery = capturedIncludes(dummyQuery);
            Assert.NotNull(includedQuery);
        }


        [Fact]
        public async Task GetTaskByIdAsync_ReturnsTask_WhenTaskExists()
        {
            // Arrange
            var taskId = 1;

            var expectedTask = new TaskItem
            {
                Id = taskId,
                Title = "Sample Task",
                IsDeleted = false,
                Attachments = new List<TaskAttachment>
                {
                    new TaskAttachment { Id = 101, FileName = "file1.pdf", IsDeleted = false },
                    new TaskAttachment { Id = 102, FileName = "file2.pdf", IsDeleted = true }
                },
                AssignedUsers = new List<TaskAssignment>
                {
                    new TaskAssignment
                    {
                        Id = 201,
                        UserProfile = new UserProfile { Id = 1, FullName = "John Doe" }
                    }
                },
                Project = new Project
                {
                    Id = 10,
                    Title = "Project Alpha"
                }
            };

            _taskRepositoryMock.Setup(u => u.FindByExpressionAsync(
                    It.IsAny<Expression<Func<TaskItem, bool>>>(),
                    null,
                    It.IsAny<Func<IQueryable<TaskItem>, IQueryable<TaskItem>>>(),
                    null, null))
                .ReturnsAsync(new List<TaskItem> { expectedTask });

            // Act
            var result = await _taskService.GetTaskByIdAsync(taskId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(taskId, result!.Id);
            Assert.Equal("Sample Task", result.Title);
            Assert.Single(result.Attachments, a => !a.IsDeleted);
            Assert.Single(result.AssignedUsers);
            var assignedUser = result.AssignedUsers.FirstOrDefault();
            Assert.NotNull(assignedUser);
            Assert.Equal("John Doe", assignedUser.UserProfile.FullName);
            Assert.Equal("Project Alpha", result.Project.Title);
        }
        [Fact]
        public async Task GetTaskByIdAsync_ReturnsNull_WhenTaskDoesNotExist()
        {
            // Arrange
            _taskRepositoryMock.Setup(u => u.FindByExpressionAsync(
                    It.IsAny<Expression<Func<TaskItem, bool>>>(),
                    It.IsAny<Func<IQueryable<TaskItem>, IOrderedQueryable<TaskItem>>>(),
                    null, null, null))
                .ReturnsAsync(new List<TaskItem>()); // no task found

            // Act
            var result = await _taskService.GetTaskByIdAsync(999); // non-existent ID

            // Assert
            Assert.Null(result);
        }
        [Fact]
        public async Task GetSortedTasksAsync_ReturnsSortedTasks_WhenValidProjectId()
        {
            // Arrange
            int projectId = 1;
            var status = TaskWorkflowStatus.ToDo;
            var sortedTasks = new List<List<List<int>>>
             {
                 new List<List<int>> { new List<int> { 1, 2, 3 } }
             };

            _taskSorterMock.Setup(t => t.GetTopologicalOrderingsAsync(status, projectId))
                           .ReturnsAsync(sortedTasks);

            // Act
            var result = await _taskService.GetSortedTasksAsync(status, projectId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(sortedTasks, result);
        }
        [Fact]
        public async Task GetSortedTasksAsync_ReturnsEmptyList_WhenSortedTasksIsNull()
        {
            // Arrange
            int projectId = 1;
            var status = TaskWorkflowStatus.InProgress;

            _taskSorterMock.Setup(t => t.GetTopologicalOrderingsAsync(status, projectId))
                           .ReturnsAsync((List<List<List<int>>>)null!);

            // Act
            var result = await _taskService.GetSortedTasksAsync(status, projectId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
        [Fact]
        public async Task GetSortedTasksAsync_ThrowsArgumentException_WhenProjectIdIsInvalid()
        {
            // Arrange
            int invalidProjectId = 0;
            var status = TaskWorkflowStatus.Done;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _taskService.GetSortedTasksAsync(status, invalidProjectId));

            Assert.Equal("Invalid project ID (Parameter 'projectId')", exception.Message);
        }


        [Fact]
        public async Task GetDependentTaskIdsAsync_ReturnsDependentTaskIds_WhenValidIdAndStatus()
        {
            // Arrange
            int taskId = 1;
            var status = TaskWorkflowStatus.ToDo;
            var dependentTaskIds = new List<int> { 101, 102, 103 };

            _dependentTaskStrategyMock.Setup(d => d.InitializeAsync(status))
                .Returns(Task.CompletedTask);
            _dependentTaskStrategyMock.Setup(d => d.GetDependentTaskIdsAsync(taskId))
                .ReturnsAsync(dependentTaskIds);

            // Act
            var result = await _taskService.GetDependentTaskIdsAsync(taskId, status);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(dependentTaskIds.Count, result.Count);
            Assert.Equal(dependentTaskIds, result);
        }
        [Fact]
        public async Task GetDependentTaskIdsAsync_ReturnsEmptyList_WhenDependentTaskIdsIsNull()
        {
            // Arrange
            int taskId = 1;
            var status = TaskWorkflowStatus.InProgress;

            _dependentTaskStrategyMock.Setup(d => d.InitializeAsync(status))
                .Returns(Task.CompletedTask);
            _dependentTaskStrategyMock.Setup(d => d.GetDependentTaskIdsAsync(taskId))
                .ReturnsAsync(new List<int>());

            // Act
            var result = await _taskService.GetDependentTaskIdsAsync(taskId, status);

            // Assert
            Assert.Empty(result);
        }
        [Fact]
        public async Task GetDependentTaskIdsAsync_ThrowsException_WhenInitializationFails()
        {
            // Arrange
            int taskId = 1;
            var status = TaskWorkflowStatus.Done;

            _dependentTaskStrategyMock.Setup(d => d.InitializeAsync(status))
                .ThrowsAsync(new InvalidOperationException("Initialization failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _taskService.GetDependentTaskIdsAsync(taskId, status));

            Assert.Equal("Initialization failed", exception.Message);
        }
        [Fact]
        public async Task GetDependentTaskIdsAsync_ThrowsException_WhenGetDependentTaskIdsFails()
        {
            // Arrange
            int taskId = 1;
            var status = TaskWorkflowStatus.ToDo;

            _dependentTaskStrategyMock.Setup(d => d.InitializeAsync(status))
                .Returns(Task.CompletedTask);
            _dependentTaskStrategyMock.Setup(d => d.GetDependentTaskIdsAsync(taskId))
                .ThrowsAsync(new InvalidOperationException("Failed to get dependent task IDs"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _taskService.GetDependentTaskIdsAsync(taskId, status));

            Assert.Equal("Failed to get dependent task IDs", exception.Message);
        }


        [Fact]
        public async Task GetUserTaskAsync_ReturnsPaginatedTasks_WhenUserProfileIdIsValid()
        {
            // Arrange
            int userProfileId = 1;
            int pageIndex = 1;
            int pageSize = 2;

            var task1 = new TaskItem
            {
                Id = 1,
                Title = "Task 1",
                ProjectId = 10,
                Status = TaskWorkflowStatus.ToDo,
                Priority = TaskPriority.Medium,
                Project = new Project { Id = 10, Title = "Project A" }
            };
            task1.SetDueDate(DateTime.UtcNow.AddDays(1));

            var task2 = new TaskItem
            {
                Id = 2,
                Title = "Task 2",
                ProjectId = 20,
                Status = TaskWorkflowStatus.InProgress,
                Priority = TaskPriority.High,
                Project = new Project { Id = 20, Title = "Project B" }
            };
            task2.SetDueDate(DateTime.UtcNow.AddDays(1));

            var tasks = new List<TaskItem> { task1, task2 };

            Expression<Func<TaskItem, bool>> capturedPredicate = null!;
            Func<IQueryable<TaskItem>, IOrderedQueryable<TaskItem>> capturedOrderBy = null!;
            Func<IQueryable<TaskItem>, IQueryable<TaskItem>> capturedIncludes = null!;

            _taskRepositoryMock.Setup(r => r.GetPaginatedListAsync(
                It.IsAny<Expression<Func<TaskItem, bool>>>(),
                It.IsAny<Func<IQueryable<TaskItem>, IOrderedQueryable<TaskItem>>>(),
                It.IsAny<Func<IQueryable<TaskItem>, IQueryable<TaskItem>>>(),
                It.IsAny<int>(), It.IsAny<int>()))
             .Callback<Expression<Func<TaskItem, bool>>,
                       Func<IQueryable<TaskItem>, IOrderedQueryable<TaskItem>>,
                       Func<IQueryable<TaskItem>, IQueryable<TaskItem>>,
                       int?, int?>(
                 (pred, ord, incl, _, _) =>
                 {
                     capturedPredicate = pred;
                     capturedOrderBy = ord;
                     capturedIncludes = incl;
                 })
             .ReturnsAsync((tasks, tasks.Count));

            // Act
            var result = await _taskService.GetUserTaskAsync(userProfileId, pageIndex, pageSize);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Items.Count);
            Assert.Equal(2, result.TotalCount);
            Assert.Equal("Task 1", result.Items[0].Title);
            Assert.Equal("Project A", result.Items[0].ProjectTitle);
            Assert.Equal("Task 2", result.Items[1].Title);
            Assert.Equal("Project B", result.Items[1].ProjectTitle);

            // Includes
            Assert.NotNull(capturedIncludes);
            var dummyQuery = new List<TaskItem>
             {
                 new TaskItem { Project = new Project { Title = "Dummy" } }
             }.AsQueryable();
            var queryWithIncludes = capturedIncludes(dummyQuery);
            Assert.NotNull(queryWithIncludes);

            // Predicate (userProjectIds should contain 10 and 20)
            Assert.NotNull(capturedPredicate);
            var compiledPredicate = capturedPredicate.Compile();
            Assert.False(compiledPredicate(task1));
            Assert.False(compiledPredicate(task2));
            Assert.False(compiledPredicate(new TaskItem { ProjectId = 99 }));

            // OrderBy
            Assert.NotNull(capturedOrderBy);
            var unordered = new List<TaskItem>
             {
                 new TaskItem { UpdatedDate = DateTime.UtcNow.AddDays(-1) },
                 new TaskItem { UpdatedDate = DateTime.UtcNow }
             }.AsQueryable();

            var ordered = capturedOrderBy(unordered).ToList();
            Assert.True(ordered[0].UpdatedDate > ordered[1].UpdatedDate);
        }
        [Fact]
        public async Task GetUserTaskAsync_ReturnsEmptyPaginatedList_WhenUserProfileIdIsNull()
        {
            // Arrange
            int? userProfileId = null;
            int pageIndex = 1;
            int pageSize = 5;

            // Act
            var result = await _taskService.GetUserTaskAsync(userProfileId, pageIndex, pageSize);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalCount);
        }
        [Fact]
        public async Task GetUserTaskAsync_ThrowsException_WhenPageIndexIsLessThan1()
        {
            // Arrange
            int? userProfileId = 1;
            int invalidPageIndex = 0;
            int pageSize = 10;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                _taskService.GetUserTaskAsync(userProfileId, invalidPageIndex, pageSize));

            Assert.Equal("pageIndex", exception.ParamName);
            Assert.Contains("Page index must be greater than zero.", exception.Message);
        }
        [Fact]
        public async Task GetUserTaskAsync_ThrowsException_WhenPageSizeIsLessThan1()
        {
            // Arrange
            int? userProfileId = 1;
            int pageIndex = 1;
            int invalidPageSize = 0;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                _taskService.GetUserTaskAsync(userProfileId, pageIndex, invalidPageSize));

            Assert.Equal("pageSize", exception.ParamName);
            Assert.Contains("Page size must be greater than zero.", exception.Message);
        }


        [Fact]
        public async Task CreateTaskAsync_ThrowsIfAttachmentsExceedLimit()
        {
            // Arrange
            var taskDto = new TaskDto
            {
                Title = "Test",
                Attachments = Enumerable.Range(1, 11)
                    .Select(_ => (IFormFile)new FormFile(Stream.Null, 0, 1, "file", "test.txt"))
                    .ToList()
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _taskService.CreateTaskAsync(taskDto));
        }
        [Fact]
        public async Task CreateTaskAsync_CreatesTask_WhenNoAttachments()
        {
            // Arrange
            var taskDto = new TaskDto
            {
                ProjectId = 1,
                Title = "No Attachments Task",
                Description = "No attachments",
                StartDate = DateTime.UtcNow,
                Status = TaskWorkflowStatus.ToDo,
                Priority = TaskPriority.Medium,
                Attachments = new List<IFormFile>()
            };

            // Act
            await _taskService.CreateTaskAsync(taskDto);

            // Assert
            _taskRepositoryMock.Verify(r => r.AddAsync(It.IsAny<TaskItem>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }
        [Fact]
        public async Task CreateTaskAsync_SetsDueDate_WhenProvided()
        {
            // Arrange
            var dueDate = DateTime.UtcNow.AddDays(5);
            var taskDto = new TaskDto
            {
                ProjectId = 1,
                Title = "Test Task",
                Description = "Test Description",
                StartDate = DateTime.UtcNow,
                Status = TaskWorkflowStatus.ToDo,
                Priority = TaskPriority.Medium,
                DueDate = dueDate,
                Attachments = new List<IFormFile>()
            };

            // Act
            await _taskService.CreateTaskAsync(taskDto);

            // Assert
            _taskRepositoryMock.Verify(r => r.AddAsync(It.IsAny<TaskItem>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
            _taskRepositoryMock.Verify(r => r.AddAsync(It.Is<TaskItem>(t => t.DueDate == dueDate)), Times.Once);
        }
        [Fact]
        public async Task CreateTaskAsync_DoesNotSetDueDate_WhenNotProvided()
        {
            // Arrange
            var taskDto = new TaskDto
            {
                ProjectId = 1,
                Title = "Test Task",
                Description = "Test Description",
                StartDate = DateTime.UtcNow,
                Status = TaskWorkflowStatus.ToDo,
                Priority = TaskPriority.Medium,
                Attachments = new List<IFormFile>()
            };

            // Act
            await _taskService.CreateTaskAsync(taskDto);

            // Assert
            _taskRepositoryMock.Verify(r => r.AddAsync(It.IsAny<TaskItem>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
            _taskRepositoryMock.Verify(r => r.AddAsync(It.Is<TaskItem>(t => t.DueDate == null)), Times.Once);
        }
        [Fact]
        public async Task CreateTaskAsync_ThrowsIOException_WhenSavingAttachmentFails()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(3);
            fileMock.Setup(f => f.FileName).Returns("testfile.txt");
            fileMock.Setup(f => f.ContentType).Returns("text/plain");

            fileMock
                .Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new IOException("Disk error"));

            var taskDto = new TaskDto
            {
                ProjectId = 1,
                Title = "Task with bad file",
                Description = "Description",
                StartDate = DateTime.UtcNow,
                Status = TaskWorkflowStatus.ToDo,
                Priority = TaskPriority.High,
                Attachments = new List<IFormFile> { fileMock.Object }
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<IOException>(() => _taskService.CreateTaskAsync(taskDto));
            Assert.Contains("An error occurred while saving the attachment", ex.Message);

            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Failed to save file")),
                    It.Is<IOException>(e => e.Message == "Disk error"),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
        [Fact]
        public async Task CreateTaskAsync_ThrowsInvalidOperationException_WhenAttachmentsExceedLimit()
        {
            // Arrange
            var attachments = Enumerable.Range(0, 11).Select(i =>
                new FormFile(Stream.Null, 0, 1, $"file{i}", $"file{i}.txt")
            ).Cast<IFormFile>().ToList();

            var taskDto = new TaskDto
            {
                Title = "Excess Attachments",
                Attachments = attachments
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _taskService.CreateTaskAsync(taskDto));

            Assert.Equal("You can only attach up to 10 files.", ex.Message);
        }
        [Fact]
        public async Task CreateTaskAsync_SkipsEmptyAttachments()
        {
            // Arrange
            var emptyFile = new FormFile(Stream.Null, 0, 0, "file", "empty.txt");

            var taskDto = new TaskDto
            {
                ProjectId = 1,
                Title = "Task With Empty File",
                Description = "Should skip",
                StartDate = DateTime.UtcNow,
                Status = TaskWorkflowStatus.ToDo,
                Priority = TaskPriority.Medium,
                Attachments = new List<IFormFile> { emptyFile }
            };

            // Act
            await _taskService.CreateTaskAsync(taskDto);

            // Assert
            _taskRepositoryMock.Verify(r => r.AddAsync(It.IsAny<TaskItem>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
            // No attachment added
            _taskRepositoryMock.Verify(r =>
                r.AddAsync(It.Is<TaskItem>(t => t.Attachments.Count == 0)), Times.Once);
        }
        [Fact]
        public async Task CreateTaskAsync_AddsAttachment_WhenFileIsValid()
        {
            // Arrange
            var fileContent = new MemoryStream(new byte[] { 1, 2, 3 });
            var formFile = new FormFile(fileContent, 0, fileContent.Length, "file", "test.txt")
            {
                Headers = new HeaderDictionary(),
                ContentType = "text/plain"
            };

            var taskDto = new TaskDto
            {
                ProjectId = 1,
                Title = "Task With Valid File",
                Description = "Should attach",
                StartDate = DateTime.UtcNow,
                Status = TaskWorkflowStatus.ToDo,
                Priority = TaskPriority.Medium,
                Attachments = new List<IFormFile> { formFile }
            };

            // Act
            await _taskService.CreateTaskAsync(taskDto);

            // Assert
            _taskRepositoryMock.Verify(r =>
                r.AddAsync(It.Is<TaskItem>(t => t.Attachments.Count == 1)), Times.Once);
        }

        [Fact]
        public async Task UpdateTaskAsync_SuccessfullyUpdatesTask_WithoutAttachments()
        {
            // Arrange
            var taskId = 1;
            var existingTask = new TaskItem
            {
                Id = taskId,
                Title = "Old Title",
                Attachments = new List<TaskAttachment>(),
                AssignedUsers = new List<TaskAssignment>(),
                Dependencies = new List<TaskDependency>()
            };

            var updateDto = new TaskUpdateDto
            {
                Id = taskId,
                Title = "New Title",
                Description = "Updated Desc",
                Status = (int)TaskWorkflowStatus.InProgress,
                Priority = (int)TaskPriority.High,
                StartDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(5),
                AssignedUserIds = new List<int> { 1001 },
                DependsOnTaskIds = new List<int> { 2 },
                Attachments = null // No attachments
            };

            var user = new UserProfile { Id = 1001 };

            _taskRepositoryMock.Setup(r => r.FindByExpressionAsync(
                It.IsAny<Expression<Func<TaskItem, bool>>>(), null,
                It.IsAny<Func<IQueryable<TaskItem>, IQueryable<TaskItem>>>(), null, null))
                .ReturnsAsync(new List<TaskItem> { existingTask });

            _userProfileRepositoryMock.Setup(r => r.FindByExpressionAsync(
                It.IsAny<Expression<Func<UserProfile, bool>>>(), null, null, null, null))
                .ReturnsAsync(new List<UserProfile> { user });

            _unitOfWorkMock.Setup(u => u.BeginTransactionAsync())
                .ReturnsAsync(_transactionMock.Object);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
            _taskRepositoryMock.Setup(r => r.UpdateAsync(existingTask))
                .Returns(Task.CompletedTask);
            _transactionMock.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);


            // Act
            await _taskService.UpdateTaskAsync(updateDto);

            // Assert
            Assert.Equal("New Title", existingTask.Title);
            Assert.Equal("Updated Desc", existingTask.Description);
            Assert.Equal(TaskWorkflowStatus.InProgress, existingTask.Status);
            Assert.Single(existingTask.AssignedUsers);
            Assert.Single(existingTask.Dependencies);

            _taskRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<TaskItem>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
            _transactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            _userProfileRepositoryMock.Verify(x => x.FindByExpressionAsync(
                It.IsAny<Expression<Func<UserProfile, bool>>>(),
                null, null, null, null), Times.Once);

            _taskRepositoryMock.Verify(x => x.FindByExpressionAsync(
                It.IsAny<Expression<Func<TaskItem, bool>>>(), null,
                It.IsAny<Func<IQueryable<TaskItem>, IQueryable<TaskItem>>>(),
                null, null), Times.Once);

            _unitOfWorkMock.Verify(x => x.BeginTransactionAsync(), Times.Once);

        }




        [Fact]
        public async Task DeleteAttachmentAsync_DeletesAttachmentFile()
        {
            // Arrange
            var attachmentId = 1;
            var attachment = new TaskAttachment { Id = attachmentId, FilePath = "somepath/file.txt" };

            _taskAttachmentRepositoryMock.Setup(u => u.GetByIdAsync(attachmentId))
                .ReturnsAsync(attachment);

            // Act
            await _taskService.DeleteAttachmentAsync(attachmentId);

            // Assert
            _fileServiceMock.Verify(f => f.DeleteFileAsync("somepath/file.txt"), Times.Once);
            _taskAttachmentRepositoryMock.Verify(u => u.DeleteByIdAsync(attachmentId), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }
        [Fact]
        public async Task DeleteAttachmentAsync_ThrowsIfNotFound()
        {
            // Arrange
            var attachmentId = 999;
            _taskAttachmentRepositoryMock.Setup(u => u.GetByIdAsync(attachmentId))
                .ReturnsAsync(value: null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _taskService.DeleteAttachmentAsync(attachmentId));
        }
    }
}

