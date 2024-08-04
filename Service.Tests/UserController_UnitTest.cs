using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using TMPApplication.DTOs.UserDtos;
using TMPApplication.UserTasks;
using TMPDomain.HelperModels;
using TMPService.Controllers.Users;
using Xunit;

public class UserController_UnitTest
{
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly UserController _controller;
    private readonly Mock<HttpContext> _mockHttpContext;

    public UserController_UnitTest()
    {
        _mockUserService = new Mock<IUserService>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockHttpContext = new Mock<HttpContext>();
        _controller = new UserController(_mockUserService.Object, _mockConfiguration.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = _mockHttpContext.Object
            }
        };
    }

    [Fact]
    public async Task Login_ReturnsOkResult_WithAccessToken()
    {
        // Arrange
        var loginRequest = new LoginRequest { Email = "testuser", Password = "password" };
        var loginResponse = new LoginResponse { AccessToken = "access_token" };
        _mockUserService.Setup(service => service.LoginWithCredentials(loginRequest)).ReturnsAsync(loginResponse);

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("access_token", okResult.Value);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenLoginFails()
    {
        // Arrange
        var loginRequest = new LoginRequest { Email = "testuser", Password = "password" };
        var loginResponse = new LoginResponse { AccessToken = null, Message = "Login failed" };
        _mockUserService.Setup(service => service.LoginWithCredentials(loginRequest)).ReturnsAsync(loginResponse);

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("Login failed", unauthorizedResult.Value);
    }

    [Fact]
    public async Task Register_ReturnsOkResult_WhenRegistrationIsSuccessful()
    {
        // Arrange
        var registerRequest = new RegisterRequest { Email = "test@test.com", Password = "password" };
        var response = new Dictionary<string, object> { { "Success", true } };
        _mockUserService.Setup(service => service.RegisterWithCredentials(registerRequest, "John", "Doe")).ReturnsAsync(response);

        // Act
        var result = await _controller.Register(registerRequest, "John", "Doe");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(response, okResult.Value);
    }

    [Fact]
    public async Task UpdateUserProfile_ReturnsOkResult_WhenProfileIsUpdated()
    {
        // Arrange
        var updateRequest = new UserProfileUpdateDto { FirstName = "John", LastName = "Doe" };
        var userId = "1";
        var result = new OkResult();
        _mockUserService.Setup(service => service.UpdateUserProfileAsync(userId, updateRequest)).ReturnsAsync(result);

        // Act
        var response = await _controller.UpdateUserProfile(userId, updateRequest);

        // Assert
        Assert.IsType<OkResult>(response);
    }

    [Fact]
    public async Task DeleteUserAsync_ReturnsOkResult_WhenUserIsDeleted()
    {
        // Arrange
        var userId = "1";
        var deleteResponse = new ApiResponse { StatusCode = 200, Success = true };
        _mockUserService.Setup(service => service.DeleteUserAsync(userId)).ReturnsAsync(deleteResponse);

        // Act
        var result = await _controller.DeleteUserAsync(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(deleteResponse, okResult.Value);
    }

    [Fact]
    public async Task DeleteUserAsync_ReturnsBadRequest_WhenUserDeletionFails()
    {
        // Arrange
        var userId = "1";
        var deleteResponse = new ApiResponse { StatusCode = 400, Success = false };
        _mockUserService.Setup(service => service.DeleteUserAsync(userId)).ReturnsAsync(deleteResponse);

        // Act
        var result = await _controller.DeleteUserAsync(userId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(deleteResponse, badRequestResult.Value);
    }

    [Fact]
    public async Task ChangePassword_ReturnsBadRequest_WhenRequestIsNull()
    {
        // Act
        var result = await _controller.ChangePassword(null);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid input.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetUsersStatistics_ReturnsOkResult_WithUserStatistics()
    {
        // Arrange
        var statistics = new UserStatistics { AllUsers = 10 };
        _mockUserService.Setup(service => service.GetUserStatistics()).ReturnsAsync(statistics);

        // Act
        var result = await _controller.GetUsersStatistics();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(statistics, okResult.Value);
    }

    [Fact]
    public async Task GetUsersStatistics_ReturnsNoContent_WhenNoStatisticsFound()
    {
        // Arrange
        _mockUserService.Setup(service => service.GetUserStatistics()).ReturnsAsync((UserStatistics)null);

        // Act
        var result = await _controller.GetUsersStatistics();

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task GetPagedUsers_ReturnsOkResult_WithPagedUsers()
    {
        // Arrange
        var pagedUsers = new PagedResult<UserInfoDto>
        {
            Items = new List<UserInfoDto> { new UserInfoDto { Id = "1", FirstName = "John", LastName = "Doe", Email = "test@test.com" } },
            TotalItems = 1
        };
        _mockUserService.Setup(service => service.GetPagedAsync(1, 10)).ReturnsAsync(pagedUsers);

        // Act
        var result = await _controller.GetPagedUsers(1, 10);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<PagedResult<UserInfoDto>>(okResult.Value);
        Assert.Single(returnValue.Items);
    }

    [Fact]
    public async Task GetPagedUsers_ReturnsNotFound_WhenNoUsersFound()
    {
        // Arrange
        var pagedUsers = new PagedResult<UserInfoDto> { Items = new List<UserInfoDto>(), TotalItems = 0 };
        _mockUserService.Setup(service => service.GetPagedAsync(1, 10)).ReturnsAsync(pagedUsers);

        // Act
        var result = await _controller.GetPagedUsers(1, 10);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("No users found", notFoundResult.Value);
    }
}
