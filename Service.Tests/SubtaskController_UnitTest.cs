using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using TMP.Application.DTOs.SubtaskDtos;
using TMPApplication.Interfaces.Subtasks;
using TMPService.Controllers.Subtasks;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace TMP.Service.Tests
{
    public class SubtaskController_UnitTest
    {
        private readonly Mock<ISubtaskService> _subtaskServiceMock;
        private readonly Mock<ILogger<SubtaskController>> _loggerServiceMock;
        private readonly SubtaskController _controller;

        public SubtaskController_UnitTest()
        {
            _subtaskServiceMock = new Mock<ISubtaskService>();
            _loggerServiceMock = new Mock<ILogger<SubtaskController>>();

            _controller = new SubtaskController(
                _subtaskServiceMock.Object,
                _loggerServiceMock.Object
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
        public async Task GetSubtasks_ReturnsOkResult_WithSubtasks()
        {
            // Arrange
            var mockSubtasks = new List<SubtaskDto> { new SubtaskDto { Id = 1, Title = "Subtask 1" } };
            _subtaskServiceMock.Setup(service => service.GetAllSubtasksAsync()).ReturnsAsync(mockSubtasks);

            // Act
            var result = await _controller.GetSubtasks();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<SubtaskDto>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedSubtasks = Assert.IsType<List<SubtaskDto>>(okResult.Value);
            Assert.Single(returnedSubtasks);
        }

        [Fact]
        public async Task GetSubtask_ReturnsOkResult_WithSubtask()
        {
            // Arrange
            var subtaskDto = new SubtaskDto { Id = 1, Title = "Subtask 1" };
            _subtaskServiceMock.Setup(service => service.GetSubtaskByIdAsync(1)).ReturnsAsync(subtaskDto);

            // Act
            var result = await _controller.GetSubtask(1);

            // Assert
            var actionResult = Assert.IsType<ActionResult<SubtaskDto>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedSubtask = Assert.IsType<SubtaskDto>(okResult.Value);
            Assert.Equal("Subtask 1", returnedSubtask.Title);
        }

        [Fact]
        public async Task AddSubtask_ReturnsCreatedAtActionResult_WhenSuccessful()
        {
            // Arrange
            var newSubtask = new AddSubtaskDto { TaskId = 1, Title = "New Subtask" };
            var subtaskDto = new SubtaskDto { Id = 1, Title = "New Subtask" };
            _subtaskServiceMock.Setup(service => service.AddSubtaskAsync(newSubtask)).ReturnsAsync(subtaskDto);

            // Act
            var result = await _controller.AddSubtask(newSubtask);

            // Assert
            var actionResult = Assert.IsType<ActionResult<SubtaskDto>>(result);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var returnedSubtask = Assert.IsType<SubtaskDto>(createdAtActionResult.Value);
            Assert.Equal("New Subtask", returnedSubtask.Title);
        }

        [Fact]
        public async Task UpdateSubtaskCompletion_ReturnsBadRequest_WhenUpdateFails()
        {
            // Arrange
            var completionDto = new UpdateSubtaskCompletionDto { SubtaskId = 1, IsCompleted = true };
            _subtaskServiceMock.Setup(service => service.UpdateSubtaskCompletionAsync(completionDto)).ReturnsAsync(false);

            // Act
            var result = await _controller.UpdateSubtaskCompletion(completionDto);

            // Assert
            var actionResult = Assert.IsType<BadRequestObjectResult>(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult);
            var returnedMessage = Assert.IsType<string>(badRequestResult.Value);
            Assert.Equal("Failed to update subtask completion status.", returnedMessage);
        }

        [Fact]
        public async Task DeleteSubtask_ReturnsNoContentResult_WhenSuccessful()
        {
            // Arrange
            _subtaskServiceMock.Setup(service => service.DeleteSubtaskAsync(1)).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteSubtask(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteSubtask_ReturnsNotFound_WhenSubtaskNotFound()
        {
            // Arrange
            _subtaskServiceMock.Setup(service => service.DeleteSubtaskAsync(1)).ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteSubtask(1);

            // Assert
            var actionResult = Assert.IsType<NotFoundResult>(result);
            Assert.IsType<NotFoundResult>(actionResult);
        }
    }
}
