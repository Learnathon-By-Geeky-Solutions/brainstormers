using Moq;
using Xunit;
using TaskForge.Domain.Enums;
using System.Linq.Expressions;
using TaskForge.Domain.Entities;
using TaskForge.Application.Helpers.TaskSorters;
using TaskForge.Application.Interfaces.Repositories;

namespace TaskForge.Tests.Application.Helpers.TaskSorters
{
    public class TopologicalTaskSorterTests
    {
        private Mock<ITaskRepository> _taskRepoMock = null!;
        private Mock<ITaskDependencyRepository> _depRepoMock = null!;

        public TopologicalTaskSorterTests()
        {
            ResetMocks();
        }

        private void ResetMocks()
        {
            _taskRepoMock = new Mock<ITaskRepository>(MockBehavior.Strict);
            _depRepoMock = new Mock<ITaskDependencyRepository>(MockBehavior.Strict);
        }

        private TopologicalTaskSorter CreateSorter() =>
            new TopologicalTaskSorter(_depRepoMock.Object, _taskRepoMock.Object);

        [Fact]
        public async Task GetTopologicalOrderingsAsync_LinearChain_ReturnsCorrectLevels()
        {
            // Arrange
            var status = TaskWorkflowStatus.ToDo;
            var projectId = 1;

            var task1 = new TaskItem { Id = 1, Status = status, ProjectId = projectId };
            var task2 = new TaskItem { Id = 2, Status = status, ProjectId = projectId };
            var task3 = new TaskItem { Id = 3, Status = status, ProjectId = projectId };

            var allTasks = new List<TaskItem> { task1, task2, task3 };
            var dependencies = new List<TaskDependency>
            {
                new() { TaskId = 2, DependsOnTaskId = 1, Task = task2, DependsOnTask = task1 },
                new() { TaskId = 3, DependsOnTaskId = 2, Task = task3, DependsOnTask = task2 }
            };

            _taskRepoMock.Setup(r => r.FindByExpressionAsync(
                It.IsAny<Expression<Func<TaskItem, bool>>>(),
                null, null, null, null))
                .ReturnsAsync(allTasks);

            _depRepoMock.Setup(r => r.FindByExpressionAsync(
                It.IsAny<Expression<Func<TaskDependency, bool>>>(),
                null, null, null, null))
                .ReturnsAsync(dependencies);

            var sorter = CreateSorter();

            // Act
            var result = await sorter.GetTopologicalOrderingsAsync(status, projectId);

            // Assert
            Assert.Single(result);
            Assert.Equal(3, result[0].Count);
            Assert.Equal(new List<int> { 1 }, result[0][0]);
            Assert.Equal(new List<int> { 2 }, result[0][1]);
            Assert.Equal(new List<int> { 3 }, result[0][2]);
        }
        [Fact]
        public async Task GetTopologicalOrderingsAsync_NoDependencies_ReturnsEachInOwnComponent()
        {
            // Arrange
            var projectId = 1;
            var status = TaskWorkflowStatus.ToDo;

            var taskA = new TaskItem { Id = 1, Status = status, ProjectId = projectId };
            var taskB = new TaskItem { Id = 2, Status = status, ProjectId = projectId };

            _taskRepoMock.Setup(r => r.FindByExpressionAsync(
                It.IsAny<Expression<Func<TaskItem, bool>>>(),
                null, null, null, null))
                .ReturnsAsync(new List<TaskItem> { taskA, taskB });

            _depRepoMock.Setup(r => r.FindByExpressionAsync(
                It.IsAny<Expression<Func<TaskDependency, bool>>>(),
                null, null, null, null))

                .ReturnsAsync(new List<TaskDependency>());
            var sorter = CreateSorter();

            // Act
            var result = await sorter.GetTopologicalOrderingsAsync(status, projectId);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, component =>
            {
                Assert.Single(component); // One level per component
                Assert.Single(component[0]); // One task per level
            });

            var flatTaskIds = result.SelectMany(levels => levels.SelectMany(tasks => tasks)).ToList();
            Assert.Contains(1, flatTaskIds);
            Assert.Contains(2, flatTaskIds);
        }
        [Fact]
        public async Task GetTopologicalOrderingsAsync_MultipleLinearDependencies_ReturnsCorrectLevels()
        {
            // Arrange
            var projectId = 3;
            var status = TaskWorkflowStatus.Done;

            var task1 = new TaskItem { Id = 1, Status = status, ProjectId = projectId };
            var task2 = new TaskItem { Id = 2, Status = status, ProjectId = projectId };
            var task3 = new TaskItem { Id = 3, Status = status, ProjectId = projectId };
            var task4 = new TaskItem { Id = 4, Status = status, ProjectId = projectId };

            var tasks = new List<TaskItem> { task1, task2, task3, task4 };
            var dependencies = new List<TaskDependency>
            {
                new() { TaskId = 2, DependsOnTaskId = 1, Task = task2, DependsOnTask = task1 },
                new() { TaskId = 3, DependsOnTaskId = 2, Task = task3, DependsOnTask = task2 },
                new() { TaskId = 4, DependsOnTaskId = 3, Task = task4, DependsOnTask = task3 }
            };

            _taskRepoMock.Setup(r => r.FindByExpressionAsync(
                It.IsAny<Expression<Func<TaskItem, bool>>>(),
                null, null, null, null))
                .ReturnsAsync(tasks);

            _depRepoMock.Setup(r => r.FindByExpressionAsync(
                It.IsAny<Expression<Func<TaskDependency, bool>>>(),
                null, null, null, null))
                .ReturnsAsync(dependencies);

            var sorter = CreateSorter();

            // Act
            var result = await sorter.GetTopologicalOrderingsAsync(status, projectId);

            // Assert
            Assert.Single(result); // One connected component
            var levels = result[0];
            Assert.Equal(4, levels.Count);

            Assert.Equal(new List<int> { 1 }, levels[0]);
            Assert.Equal(new List<int> { 2 }, levels[1]);
            Assert.Equal(new List<int> { 3 }, levels[2]);
            Assert.Equal(new List<int> { 4 }, levels[3]);
        }
        [Fact]
        public async Task GetTopologicalOrderingsAsync_CycleDetected_Throws()
        {
            // Arrange
            var projectId = 4;
            var status = TaskWorkflowStatus.InProgress;

            var task1 = new TaskItem { Id = 10, Status = status, ProjectId = projectId };
            var task2 = new TaskItem { Id = 20, Status = status, ProjectId = projectId };

            _taskRepoMock.Setup(r => r.FindByExpressionAsync(
                It.IsAny<Expression<Func<TaskItem, bool>>>(),
                null, null, null, null))
                .ReturnsAsync(new List<TaskItem> { task1, task2 });

            _depRepoMock.Setup(r => r.FindByExpressionAsync(
                It.IsAny<Expression<Func<TaskDependency, bool>>>(),
                null, null, null, null))
                .ReturnsAsync(new List<TaskDependency>
                {
                    new() { TaskId = 20, DependsOnTaskId = 10, Task = task2, DependsOnTask = task1 },
                    new() { TaskId = 10, DependsOnTaskId = 20, Task = task1, DependsOnTask = task2 }
                });

            var sorter = CreateSorter();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => sorter.GetTopologicalOrderingsAsync(status, projectId));
            Assert.IsType<InvalidOperationException>(exception);
            Assert.Contains("Cycle", exception.Message);
        }
        [Fact]
        public async Task GetTopologicalOrderingsAsync_DependencyOnMissingTask_Throws()
        {
            var status = TaskWorkflowStatus.ToDo;
            var projectId = 1;

            var task1 = new TaskItem { Id = 1, Status = status, ProjectId = projectId };
            var allTasks = new List<TaskItem> { task1 };

            var dependencies = new List<TaskDependency>
            {
                new() { TaskId = 1, DependsOnTaskId = 999, Task = task1, DependsOnTask = null! }
            };

            _taskRepoMock.Setup(r => r.FindByExpressionAsync(
                It.IsAny<Expression<Func<TaskItem, bool>>>(), null, null, null, null))
                .ReturnsAsync(allTasks);

            _depRepoMock.Setup(r => r.FindByExpressionAsync(
                It.IsAny<Expression<Func<TaskDependency, bool>>>(), null, null, null, null))
                .ReturnsAsync(dependencies);

            var sorter = CreateSorter();

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => sorter.GetTopologicalOrderingsAsync(status, projectId));
        }
        [Fact]
        public async Task GetTopologicalOrderingsAsync_SimpleDependency_ReturnsCorrectOrdering()
        {
            var status = TaskWorkflowStatus.ToDo;
            var projectId = 1;

            var taskA = new TaskItem { Id = 1, Status = status, ProjectId = projectId };
            var taskB = new TaskItem { Id = 2, Status = status, ProjectId = projectId };

            var dependencies = new List<TaskDependency>
            {
                new() { TaskId = 1, DependsOnTaskId = 2, Task = taskA, DependsOnTask = taskB }
            };

            _taskRepoMock.Setup(r => r.FindByExpressionAsync(
                It.IsAny<Expression<Func<TaskItem, bool>>>(), null, null, null, null))
                .ReturnsAsync(new List<TaskItem> { taskA, taskB });

            _depRepoMock.Setup(r => r.FindByExpressionAsync(
                It.IsAny<Expression<Func<TaskDependency, bool>>>(), null, null, null, null))
                .ReturnsAsync(dependencies);

            var sorter = CreateSorter();

            var result = await sorter.GetTopologicalOrderingsAsync(status, projectId);

            Assert.Single(result);
            var ordering = result[0];

            // Flatten all levels into a single list of IDs
            var flatOrdering = ordering.SelectMany(x => x).ToList();

            // Expected: taskB (2) comes before taskA (1)
            Assert.Equal(2, flatOrdering[0]);
            Assert.Equal(1, flatOrdering[1]);
        }

    }
}