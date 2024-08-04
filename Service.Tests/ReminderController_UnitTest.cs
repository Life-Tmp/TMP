using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPApplication.DTOs.ReminderDtos;
using TMPApplication.Interfaces.Reminders;
using TMPService.Controllers;
using Xunit;

public class ReminderController_UnitTest
{
    private readonly Mock<IReminderService> _mockReminderService;
    private readonly Mock<ILogger<ReminderController>> _mockLogger;
    private readonly ReminderController _controller;

    public ReminderController_UnitTest()
    {
        _mockReminderService = new Mock<IReminderService>();
        _mockLogger = new Mock<ILogger<ReminderController>>();
        _controller = new ReminderController(_mockReminderService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetReminderAsync_ReturnsOkResult_WithReminder()
    {
        // Arrange
        var reminder = new GetReminderDto { Id = 1, Description = "Test Reminder", ReminderDateTime = DateTime.Now, TaskId = 1 };
        _mockReminderService.Setup(service => service.GetReminderAsync(1)).ReturnsAsync(reminder);

        // Act
        var result = await _controller.GetReminderAsync(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<GetReminderDto>(okResult.Value);
        Assert.Equal(1, returnValue.Id);
    }

    [Fact]
    public async Task GetReminderAsync_ReturnsBadRequest_WhenIdIsInvalid()
    {
        // Act
        var result = await _controller.GetReminderAsync(-1);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("The reminderId must be a positive integer.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetReminderAsync_ReturnsNoContent_WhenReminderNotFound()
    {
        // Arrange
        _mockReminderService.Setup(service => service.GetReminderAsync(1)).ReturnsAsync((GetReminderDto)null);

        // Act
        var result = await _controller.GetReminderAsync(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task GetRemindersForTask_ReturnsOkResult_WithReminders()
    {
        // Arrange
        var reminders = new List<GetReminderDto>
        {
            new GetReminderDto { Id = 1, Description = "Test Reminder 1", ReminderDateTime = DateTime.Now, TaskId = 1 },
            new GetReminderDto { Id = 2, Description = "Test Reminder 2", ReminderDateTime = DateTime.Now, TaskId = 1 }
        };
        _mockReminderService.Setup(service => service.GetRemindersForTask(1)).ReturnsAsync(reminders);

        // Act
        var result = await _controller.GetRemindersForTask(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<List<GetReminderDto>>(okResult.Value);
        Assert.Equal(2, returnValue.Count);
    }

    [Fact]
    public async Task GetRemindersForTask_ReturnsBadRequest_WhenIdIsInvalid()
    {
        // Act
        var result = await _controller.GetRemindersForTask(-1);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Not a valid task Id", badRequestResult.Value);
    }

    [Fact]
    public async Task CreateReminder_ReturnsOkResult_WhenReminderIsCreated()
    {
        // Arrange
        var createReminderDto = new CreateReminderDto { Description = "Test Reminder", ReminderDate = DateTime.Now, TaskId = 1 };

        // Act
        var result = await _controller.CreateReminder(createReminderDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Successfully created a reminder", okResult.Value);
    }

    [Fact]
    public async Task CreateReminder_ReturnsBadRequest_WhenDtoIsNull()
    {
        // Act
        var result = await _controller.CreateReminder(null);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid reminder data.", badRequestResult.Value);
    }

    [Fact]
    public async Task ProcessReminder_ReturnsOkResult()
    {
        // Arrange
        int reminderId = 1;

        // Act
        var result = await _controller.ProcessReminder(reminderId);

        // Assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task UpdateReminder_ReturnsOkResult_WhenReminderIsUpdated()
    {
        // Arrange
        var reminderDto = new ReminderDto { Description = "Updated Reminder", ReminderDateTime = DateTime.Now };
        _mockReminderService.Setup(service => service.UpdateReminder(1, reminderDto)).ReturnsAsync(true);

        // Act
        var result = await _controller.UpdateReminder(1, reminderDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Updated successfully", okResult.Value);
    }

    [Fact]
    public async Task UpdateReminder_ReturnsBadRequest_WhenUpdateFails()
    {
        // Arrange
        var reminderDto = new ReminderDto { Description = "Updated Reminder", ReminderDateTime = DateTime.Now };
        _mockReminderService.Setup(service => service.UpdateReminder(1, reminderDto)).ReturnsAsync(false);

        // Act
        var result = await _controller.UpdateReminder(1, reminderDto);

        // Assert
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task DeleteReminder_ReturnsOkResult_WhenReminderIsDeleted()
    {
        // Arrange
        _mockReminderService.Setup(service => service.DeleteReminder(1)).ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteReminder(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Deleted successfully", okResult.Value);
    }

    [Fact]
    public async Task DeleteReminder_ReturnsBadRequest_WhenIdIsInvalid()
    {
        // Act
        var result = await _controller.DeleteReminder(-1);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("The reminder id must be a positive integer.", badRequestResult.Value);
    }

    [Fact]
    public async Task DeleteReminder_ReturnsNotFound_WhenReminderNotFound()
    {
        // Arrange
        _mockReminderService.Setup(service => service.DeleteReminder(1)).ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteReminder(1);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Reminder not found.", notFoundResult.Value);
    }
}
