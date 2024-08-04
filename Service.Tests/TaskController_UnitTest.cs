using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using TMP.Application.DTOs.CommentDtos;
using TMP.Application.DTOs.SubtaskDtos;
using TMP.Application.DTOs.TaskDtos;
using TMPApplication.Interfaces;
using TMPApplication.Interfaces.Tasks;
using TMPService.Controllers.Tasks;

namespace TMP.Service.Tests
{
    public class TaskController_UnitTest
    {
        private readonly Mock<ITaskService> _taskServiceMock;
        private readonly Mock<ITimeTrackingService> _timeTrackingServiceMock;
        private readonly Mock<ISearchService<TaskDto>> _searchServiceMock;
        private readonly TaskController _controller;

        public TaskController_UnitTest()
        {
            _taskServiceMock = new Mock<ITaskService>();
            _timeTrackingServiceMock = new Mock<ITimeTrackingService>();
            _searchServiceMock = new Mock<ISearchService<TaskDto>>();
            _controller = new TaskController(_taskServiceMock.Object,
                _timeTrackingServiceMock.Object,
                _searchServiceMock.Object

                );
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, "user1")
                }, "mock"))
                }
            };
        }

        [Fact]
        public async Task GetTasks_ReturnsOkResult_WithTasks()
        {
            // Arrange
            var mockTasks = new List<TaskDto> { new TaskDto { Id = 1, Title = "Task 1" } };
            _taskServiceMock.Setup(service => service.GetTasksAsync(null)).ReturnsAsync(mockTasks);

            // Act
            var result = await _controller.GetTasks(null);

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<TaskDto>>>(result);
            var okResutl = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedTasks = Assert.IsType<List<TaskDto>>(okResutl.Value);
            Assert.Single(returnedTasks);
        }

        [Fact]
        public async Task GetTask_ReturnsOkResult_WithTask()
        {
            // Arrange
            var taskDto = new TaskDto { Id = 1, Title = "Task 1" };
            _taskServiceMock.Setup(service => service.GetTaskByIdAsync(1)).ReturnsAsync(taskDto);

            // Act
            var result = await _controller.GetTask(1);

            // Assert
            var actionResult = Assert.IsType<ActionResult<TaskDto>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedTask = Assert.IsType<TaskDto>(okResult.Value);
            Assert.Equal("Task 1", returnedTask.Title);
        }

        [Fact]
        public async Task AssignUserToTask_ReturnsOkResult_WhenSuccessful()
        {
            // Arrange
            var assignUserDto = new AssignUserToTaskDto { TaskId = 1, UserId = "user1" };
            _taskServiceMock.Setup(service => service.AssignUserToTaskAsync(assignUserDto)).ReturnsAsync(true);

            // Act
            var result = await _controller.AssignUserToTask(assignUserDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("User assigned to task successfully.", okResult.Value);
        }

        [Fact]
        public async Task UpdateTask_ReturnsNoContentResult_WhenSuccessful()
        {
            // Arrange
            var updateDto = new UpdateTaskDto { Title = "Updated Title" };
            _taskServiceMock.Setup(service => service.UpdateTaskAsync(1, updateDto)).ReturnsAsync(true);

            // Act
            var result = await _controller.UpdateTask(1, updateDto);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteTask_ReturnsNoContentResult_WhenSuccessful()
        {
            // Arrange
            _taskServiceMock.Setup(service => service.DeleteTaskAsync(1)).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteTask(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task GetAssignedUsers_ReturnsOkResult_WithUsers()
        {
            // Arrange
            var users = new List<UserDetailsDto> { new UserDetailsDto { FirstName = "user", LastName = "User" } };
            _taskServiceMock.Setup(service => service.GetAssignedUsersAsync(1)).ReturnsAsync(users);

            // Act
            var result = await _controller.GetAssignedUsers(1);

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<UserDetailsDto>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedUsers = Assert.IsType<List<UserDetailsDto>>(okResult.Value);
            Assert.Single(returnedUsers);

        }

        [Fact]
        public async Task GetComments_ReturnsOkResult_WithComments()
        {
            // Arrange
            var comments = new List<CommentDto> { new CommentDto { Id = 1, Content = "Comment 1" } };
            _taskServiceMock.Setup(service => service.GetCommentsByTaskIdAsync(1)).ReturnsAsync(comments);

            // Act
            var result = await _controller.GetComments(1);

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<CommentDto>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedComments = Assert.IsType<List<CommentDto>>(okResult.Value);
            Assert.Single(returnedComments);
        }

        [Fact]
        public async Task GetSubtasks_ReturnsOkResult_WithSubtasks()
        {
            // Arrange
            var subtasks = new List<SubtaskDto> { new SubtaskDto { Id = 1, Title = "Subtask 1" } };
            _taskServiceMock.Setup(service => service.GetSubtasksByTaskIdAsync(1)).ReturnsAsync(subtasks);

            // Act
            var result = await _controller.GetSubtasks(1);

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<SubtaskDto>>>(result);
            var okResutl = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedSubtasks = Assert.IsType<List<SubtaskDto>>(okResutl.Value);
            Assert.Single(returnedSubtasks);
        }
    }
}
