﻿using TaskForge.Domain.Entities;
using Xunit;

namespace TaskForge.Tests.Domain.Entities
{
    public class TaskAttachmentTests
    {
        [Fact]
        public void TaskAttachment_Should_Set_And_Get_Properties()
        {
            var task = new TaskItem
            {
                Id = 100,
                Title = "Test Task",
                Description = "Test Description",
                CreatedBy = "test-user"
            };
            var attachment = new TaskAttachment
            {
                Id = 1,
                FileName = "design.png",
                StoredFileName = "guid-file.png",
                FilePath = "/files/guid-file.png",
                ContentType = "image/png",
                TaskId = 100,
                Task = task,
                Description = "Mock file for testing",
                CreatedBy = "dev"
            };

            Assert.Equal(1, attachment.Id);
            Assert.NotNull(attachment.Task);
            Assert.Equal(100, attachment.TaskId);
            Assert.Equal(task, attachment.Task);
            Assert.Equal("design.png", attachment.FileName);
            Assert.Equal("guid-file.png", attachment.StoredFileName);
            Assert.Equal("/files/guid-file.png", attachment.FilePath);
            Assert.Equal("image/png", attachment.ContentType);
            Assert.Equal("Mock file for testing", attachment.Description);
            Assert.Equal("dev", attachment.CreatedBy);
        }
    }
}
