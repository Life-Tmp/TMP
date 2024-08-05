using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using TMP.Application.DTOs.CommentDtos;
using TMPService.Controllers.Comments;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using TMPApplication.Interfaces.Comments;

namespace TMP.Service.Tests
{
    public class CommentController_UnitTest
    {
        private readonly Mock<ICommentService> _commentServiceMock;
        private readonly Mock<ILogger<CommentController>> _loggerServiceMock;
        private readonly CommentController _controller;

        public CommentController_UnitTest()
        {
            _commentServiceMock = new Mock<ICommentService>();
            _loggerServiceMock = new Mock<ILogger<CommentController>>();

            _controller = new CommentController(
                _commentServiceMock.Object,
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
        public async Task GetComments_ReturnsOkResult_WithComments()
        {
            var mockComments = new List<CommentDto> { new CommentDto { Id = 1, Content = "Comment 1" } };
            _commentServiceMock.Setup(service => service.GetCommentsAsync()).ReturnsAsync(mockComments);

            var result = await _controller.GetComments();

            var actionResult = Assert.IsType<ActionResult<IEnumerable<CommentDto>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedComments = Assert.IsType<List<CommentDto>>(okResult.Value);
            Assert.Single(returnedComments);
        }

        [Fact]
        public async Task GetComments_ReturnsOkResult_WithEmptyList()
        {
            _commentServiceMock.Setup(service => service.GetCommentsAsync()).ReturnsAsync(new List<CommentDto>());

            var result = await _controller.GetComments();

            var actionResult = Assert.IsType<ActionResult<IEnumerable<CommentDto>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedComments = Assert.IsType<List<CommentDto>>(okResult.Value);
            Assert.Empty(returnedComments);
        }

        [Fact]
        public async Task GetComment_ReturnsOkResult_WithComment()
        {
            var commentDto = new CommentDto { Id = 1, Content = "Comment 1" };
            _commentServiceMock.Setup(service => service.GetCommentByIdAsync(1)).ReturnsAsync(commentDto);

            var result = await _controller.GetComment(1);

            var actionResult = Assert.IsType<ActionResult<CommentDto>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedComment = Assert.IsType<CommentDto>(okResult.Value);
            Assert.Equal("Comment 1", returnedComment.Content);
        }

        [Fact]
        public async Task GetComment_ReturnsNotFound_WhenCommentDoesNotExist()
        {
            _commentServiceMock.Setup(service => service.GetCommentByIdAsync(1)).ReturnsAsync((CommentDto)null);

            var result = await _controller.GetComment(1);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetMyComments_ReturnsOkResult_WithComments()
        {
            var mockComments = new List<CommentDto> { new CommentDto { Id = 1, Content = "Comment 1" } };
            _commentServiceMock.Setup(service => service.GetCommentsByUserIdAsync("user1")).ReturnsAsync(mockComments);

            var result = await _controller.GetMyComments();

            var actionResult = Assert.IsType<ActionResult<IEnumerable<CommentDto>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedComments = Assert.IsType<List<CommentDto>>(okResult.Value);
            Assert.Single(returnedComments);
        }

        [Fact]
        public async Task GetMyComments_ReturnsUnauthorized_WhenUserIsNotAuthenticated()
        {
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

            var result = await _controller.GetMyComments();

            Assert.IsType<UnauthorizedResult>(result.Result);
        }

        [Fact]
        public async Task AddComment_ReturnsCreatedAtActionResult_WhenSuccessful()
        {
            var newComment = new AddCommentDto { Content = "New Comment", TaskId = 1 };
            var commentDto = new CommentDto { Id = 1, Content = "New Comment" };
            _commentServiceMock.Setup(service => service.AddCommentAsync(newComment, "user1")).ReturnsAsync(commentDto);

            var result = await _controller.AddComment(newComment);

            var actionResult = Assert.IsType<ActionResult<CommentDto>>(result);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var returnedComment = Assert.IsType<CommentDto>(createdAtActionResult.Value);
            Assert.Equal("New Comment", returnedComment.Content);
        }

        [Fact]
        public async Task AddComment_ReturnsUnauthorized_WhenUserIsNotAuthenticated()
        {
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

            var newComment = new AddCommentDto { Content = "New Comment", TaskId = 1 };

            var result = await _controller.AddComment(newComment);

            Assert.IsType<UnauthorizedResult>(result.Result);
        }

        [Fact]
        public async Task UpdateComment_ReturnsNoContentResult_WhenSuccessful()
        {
            var updateDto = new AddCommentDto { Content = "Updated Comment" };
            _commentServiceMock.Setup(service => service.UpdateCommentAsync(1, updateDto)).ReturnsAsync(true);

            var result = await _controller.UpdateComment(1, updateDto);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task UpdateComment_ReturnsNotFound_WhenCommentDoesNotExist()
        {
            var updateDto = new AddCommentDto { Content = "Updated Comment" };
            _commentServiceMock.Setup(service => service.UpdateCommentAsync(1, updateDto)).ReturnsAsync(false);

            var result = await _controller.UpdateComment(1, updateDto);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteComment_ReturnsNoContentResult_WhenSuccessful()
        {
            _commentServiceMock.Setup(service => service.DeleteCommentAsync(1)).ReturnsAsync(true);

            var result = await _controller.DeleteComment(1);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteComment_ReturnsNotFound_WhenCommentDoesNotExist()
        {
            _commentServiceMock.Setup(service => service.DeleteCommentAsync(1)).ReturnsAsync(false);

            var result = await _controller.DeleteComment(1);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetComments_LogsInformation_WhenCommentsAreFetched()
        {
            var mockComments = new List<CommentDto> { new CommentDto { Id = 1, Content = "Comment 1" } };
            _commentServiceMock.Setup(service => service.GetCommentsAsync()).ReturnsAsync(mockComments);

            await _controller.GetComments();

            _loggerServiceMock.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Fetching all comments")),
                    It.IsAny<System.Exception>(),
                    It.Is<Func<It.IsAnyType, System.Exception, string>>((v, t) => true)));
        }

        [Fact]
        public async Task DeleteComment_LogsWarning_WhenCommentDoesNotExist()
        {
            _commentServiceMock.Setup(service => service.DeleteCommentAsync(1)).ReturnsAsync(false);

            await _controller.DeleteComment(1);

            _loggerServiceMock.Verify(
                logger => logger.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Comment with ID: 1 not found for deletion")),
                    It.IsAny<System.Exception>(),
                    It.Is<Func<It.IsAnyType, System.Exception, string>>((v, t) => true)));
        }
    }
}
