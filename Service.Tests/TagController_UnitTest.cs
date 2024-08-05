using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using TMP.Application.DTOs.TagDtos;
using TMP.Application.Interfaces.Tags;
using TMP.Service.Controllers.Tags;
using TMPApplication.Interfaces;

namespace TMP.Service.Tests
{
    public class TagController_UnitTest
    {
        private readonly Mock<ITagService> _tagServiceMock;
        private readonly Mock<ISearchService<TagDto>> _searchServiceMock;
        private readonly Mock<ILogger<TagController>> _loggerServiceMock;
        private readonly TagController _controller;

        public TagController_UnitTest()
        {
            _tagServiceMock = new Mock<ITagService>();
            _searchServiceMock = new Mock<ISearchService<TagDto>>();
            _loggerServiceMock = new Mock<ILogger<TagController>>();

            _controller = new TagController(
                _tagServiceMock.Object,
                _searchServiceMock.Object,
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
        public async Task GetTags_ReturnsOkResult_WithTags()
        {
            // Arrange
            var mockTags = new List<TagDto> { new TagDto { Id = 1, Name = "Tag 1" } };
            _tagServiceMock.Setup(service => service.GetAllTagsAsync()).ReturnsAsync(mockTags);

            // Act
            var result = await _controller.GetTags();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<TagDto>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedTags = Assert.IsType<List<TagDto>>(okResult.Value);
            Assert.Single(returnedTags);
        }

        [Fact]
        public async Task GetTags_ReturnsOkResult_WithEmptyList()
        {
            // Arrange
            _tagServiceMock.Setup(service => service.GetAllTagsAsync()).ReturnsAsync(new List<TagDto>());

            // Act
            var result = await _controller.GetTags();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<TagDto>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedTags = Assert.IsType<List<TagDto>>(okResult.Value);
            Assert.Empty(returnedTags);
        }

        [Fact]
        public async Task GetTag_ReturnsOkResult_WithTag()
        {
            // Arrange
            var tagDto = new TagDto { Id = 1, Name = "Tag 1" };
            _tagServiceMock.Setup(service => service.GetTagByIdAsync(1)).ReturnsAsync(tagDto);

            // Act
            var result = await _controller.GetTag(1);

            // Assert
            var actionResult = Assert.IsType<ActionResult<TagDto>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedTag = Assert.IsType<TagDto>(okResult.Value);
            Assert.Equal("Tag 1", returnedTag.Name);
        }

        [Fact]
        public async Task GetTag_ReturnsNotFound_WhenTagDoesNotExist()
        {
            // Arrange
            _tagServiceMock.Setup(service => service.GetTagByIdAsync(1)).ReturnsAsync((TagDto)null);

            // Act
            var result = await _controller.GetTag(1);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task SearchTags_ReturnsOkResult_WithTags()
        {
            // Arrange
            var mockTags = new List<TagDto> { new TagDto { Id = 1, Name = "Tag 1" } };
            _searchServiceMock.Setup(service => service.SearchDocumentAsync("test", "tags")).ReturnsAsync(mockTags);

            // Act
            var result = await _controller.SearchTags("test");

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<TagDto>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedTags = Assert.IsType<List<TagDto>>(okResult.Value);
            Assert.Single(returnedTags);
        }

        [Fact]
        public async Task SearchTags_ReturnsOkResult_WithEmptyList()
        {
            // Arrange
            _searchServiceMock.Setup(service => service.SearchDocumentAsync("test", "tags")).ReturnsAsync(new List<TagDto>());

            // Act
            var result = await _controller.SearchTags("test");

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<TagDto>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedTags = Assert.IsType<List<TagDto>>(okResult.Value);
            Assert.Empty(returnedTags);
        }

        [Fact]
        public async Task AddTag_ReturnsCreatedAtActionResult_WhenSuccessful()
        {
            // Arrange
            var newTag = new AddTagDto { Name = "New Tag" };
            var tagDto = new TagDto { Id = 1, Name = "New Tag" };
            _tagServiceMock.Setup(service => service.AddTagAsync(newTag)).ReturnsAsync(tagDto);

            // Act
            var result = await _controller.AddTag(newTag);

            // Assert
            var actionResult = Assert.IsType<ActionResult<TagDto>>(result);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var returnedTag = Assert.IsType<TagDto>(createdAtActionResult.Value);
            Assert.Equal("New Tag", returnedTag.Name);
        }

        [Fact]
        public async Task UpdateTag_ReturnsNoContentResult_WhenSuccessful()
        {
            // Arrange
            var updateDto = new AddTagDto { Name = "Updated Tag" };
            _tagServiceMock.Setup(service => service.UpdateTagAsync(1, updateDto)).ReturnsAsync(true);

            // Act
            var result = await _controller.UpdateTag(1, updateDto);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteTag_ReturnsNoContentResult_WhenSuccessful()
        {
            // Arrange
            _tagServiceMock.Setup(service => service.DeleteTagAsync(1)).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteTag(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}
