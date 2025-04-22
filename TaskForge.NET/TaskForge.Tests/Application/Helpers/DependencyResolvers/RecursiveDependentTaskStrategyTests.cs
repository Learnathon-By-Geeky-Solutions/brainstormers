using Moq;
using System.Linq.Expressions;
using TaskForge.Application.Helpers.DependencyResolvers;
using TaskForge.Application.Interfaces.Repositories;
using TaskForge.Domain.Entities;
using TaskForge.Domain.Enums;
using Xunit;

namespace TaskForge.Tests.Application.Helpers.DependencyResolvers
{
    public class RecursiveDependentTaskStrategyTests
    {
        private readonly Mock<ITaskDependencyRepository> _mockRepository;
        private readonly RecursiveDependentTaskStrategy _strategy;

        public RecursiveDependentTaskStrategyTests()
        {
            _mockRepository = new Mock<ITaskDependencyRepository>();
            _strategy = new RecursiveDependentTaskStrategy(_mockRepository.Object);
        }

        [Fact]
        public async Task InitializeAsync_ShouldBuildAdjacencyList()
        {
            // Arrange
            var status = TaskWorkflowStatus.ToDo;
            var dependencies = new List<TaskDependency>
            {
                new TaskDependency
                {
                    Id = 1,
                    TaskId = 2,
                    DependsOnTaskId = 1,
                    Task = new TaskItem { Id = 2, Status = status },
                    DependsOnTask = new TaskItem { Id = 1, Status = status }
                },
                new TaskDependency
                {
                    Id = 2,
                    TaskId = 3,
                    DependsOnTaskId = 2,
                    Task = new TaskItem { Id = 3, Status = status },
                    DependsOnTask = new TaskItem { Id = 2, Status = status }
                },
                new TaskDependency
                {
                    Id = 3,
                    TaskId = 4,
                    DependsOnTaskId = 1,
                    Task = new TaskItem { Id = 4, Status = status },
                    DependsOnTask = new TaskItem { Id = 1, Status = status }
                }
            };

            _mockRepository.Setup(u => u.FindByExpressionAsync(
                It.IsAny<Expression<Func<TaskDependency, bool>>>(), null, null, null, null))
                .ReturnsAsync(dependencies);

            // Act
            await _strategy.InitializeAsync(status);
            var result = await _strategy.GetDependentTaskIdsAsync(1);

            result.Remove(1);

            // Assert
            Assert.Contains(2, result);
            Assert.Contains(3, result);
            Assert.Contains(4, result);
            Assert.Equal(3, result.Count); 
        }

        [Fact]
        public async Task GetDependentTaskIdsAsync_ShouldReturnEmptyList_IfNoDependencies()
        {
            // Arrange
            _mockRepository.Setup(r => r.FindByExpressionAsync(
                It.IsAny<Expression<Func<TaskDependency, bool>>>(), null, null, null, null))
                .ReturnsAsync(new List<TaskDependency>());

            await _strategy.InitializeAsync(TaskWorkflowStatus.InProgress);

            // Act
            var result = await _strategy.GetDependentTaskIdsAsync(99); 

            result.Remove(99);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetDependentTaskIdsAsync_ShouldNotRevisitNodes()
        {
            // Arrange
            var status = TaskWorkflowStatus.ToDo;
            var dependencies = new List<TaskDependency>
            {
                new TaskDependency
                {
                    Id = 1,
                    TaskId = 2,
                    DependsOnTaskId = 1,
                    Task = new TaskItem { Id = 2, Status = status },
                    DependsOnTask = new TaskItem { Id = 1, Status = status }
                },
                new TaskDependency
                {
                    Id = 2,
                    TaskId = 3,
                    DependsOnTaskId = 2,
                    Task = new TaskItem { Id = 3, Status = status },
                    DependsOnTask = new TaskItem { Id = 2, Status = status }
                },
                new TaskDependency
                {
                    Id = 3,
                    TaskId = 1,
                    DependsOnTaskId = 3,
                    Task = new TaskItem { Id = 1, Status = status },
                    DependsOnTask = new TaskItem { Id = 3, Status = status }
                }
            };

            _mockRepository.Setup(r => r.FindByExpressionAsync(
                It.IsAny<Expression<Func<TaskDependency, bool>>>(), null, null, null, null))
                .ReturnsAsync(dependencies);

            await _strategy.InitializeAsync(status);

            // Act
            var result = await _strategy.GetDependentTaskIdsAsync(1);

            result.Remove(1);

            // Assert
            Assert.Contains(2, result);
            Assert.Contains(3, result);
            Assert.Equal(2, result.Count); 
        }
    }
}
