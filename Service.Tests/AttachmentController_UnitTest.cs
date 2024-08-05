using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TMPApplication.AttachmentTasks;
using TMPApplication.DTOs.AtachmentDtos;
using TMPDomain.Entities;
using TMPDomain.Exceptions;
using TMPService.Controllers;
using Xunit;
using Task = System.Threading.Tasks.Task;

public class AttachmentController_UnitTest
{
    private readonly Mock<IAttachmentService> _mockAttachmentService;
    private readonly Mock<ILogger<AttachmentController>> _mockLogger;
    private readonly AttachmentController _controller;

    public AttachmentController_UnitTest()
    {
        _mockAttachmentService = new Mock<IAttachmentService>();
        _mockLogger = new Mock<ILogger<AttachmentController>>();
        _controller = new AttachmentController(_mockAttachmentService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task UploadFileAttachment_ReturnsOkResult_WithAttachment()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        var fileName = "test.pdf";
        var ms = new MemoryStream();
        var writer = new StreamWriter(ms);
        writer.Write("Test file content");
        writer.Flush();
        ms.Position = 0;
        fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.Length).Returns(ms.Length);
        fileMock.Setup(f => f.ContentType).Returns("application/pdf");

        var addAttachmentDto = new AddAttachmentDto { Id = 1, FileName = fileName, FilePath = "path/to/file", FileSize = ms.Length, FileType = "application/pdf", TaskId = 1 };
        _mockAttachmentService.Setup(service => service.UploadAttachmentAsync(fileMock.Object, 1)).ReturnsAsync(addAttachmentDto);

        // Act
        var result = await _controller.UploadFileAttachment(fileMock.Object, 1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<AddAttachmentDto>(okResult.Value);
        Assert.Equal(1, returnValue.Id);
    }

    [Fact]
    public async Task UploadFileAttachment_ReturnsBadRequest_WhenFileIsEmpty()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(0);

        // Act
        var result = await _controller.UploadFileAttachment(fileMock.Object, 1);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("File is empty or not provided.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetAllTaskAttachments_ReturnsOkResult_WithAttachments()
    {
        // Arrange
        var attachments = new List<Attachment>
        {
            new Attachment { Id = 1, FileName = "file1.pdf", FilePath = "path/to/file1", FileSize = 1234, FileType = "application/pdf", TaskId = 1 },
            new Attachment { Id = 2, FileName = "file2.pdf", FilePath = "path/to/file2", FileSize = 5678, FileType = "application/pdf", TaskId = 1 }
        };
        _mockAttachmentService.Setup(service => service.GetAttachmentsAsync(1)).ReturnsAsync(attachments);

        // Act
        var result = await _controller.GetAllTaskAttachments(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<List<Attachment>>(okResult.Value);
        Assert.Equal(2, returnValue.Count);
    }

    [Fact]
    public async Task GetAllTaskAttachments_ReturnsNotFound_WhenNoAttachmentsFound()
    {
        // Arrange
        _mockAttachmentService.Setup(service => service.GetAttachmentsAsync(1)).ReturnsAsync((List<Attachment>)null);

        // Act
        var result = await _controller.GetAllTaskAttachments(1);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("There is no attachments for this Task", notFoundResult.Value);
    }

    [Fact]
    public async Task DownloadFileAttachment_ReturnsFileResult_WithFileContent()
    {
        // Arrange
        var fileAttachmentDto = new FileAttachmentDto { FileBytes = new byte[] { 1, 2, 3, 4 }, FileType = "application/pdf", FileName = "file.pdf" };
        _mockAttachmentService.Setup(service => service.DownloadAttachmentAsync(1)).ReturnsAsync(fileAttachmentDto);

        // Act
        var result = await _controller.DownloadFileAttachment(1);

        // Assert
        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/pdf", fileResult.ContentType);
        Assert.Equal("file.pdf", fileResult.FileDownloadName);
    }

    [Fact]
    public async Task DownloadFileAttachment_ReturnsNotFound_WhenFileNotFound()
    {
        // Arrange
        _mockAttachmentService.Setup(service => service.DownloadAttachmentAsync(1)).ReturnsAsync((FileAttachmentDto)null);

        // Act
        var result = await _controller.DownloadFileAttachment(1);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task RemoveFileAttachment_ReturnsOkResult_WhenAttachmentIsDeleted()
    {
        // Arrange
        _mockAttachmentService.Setup(service => service.RemoveAttachmentAsync(1)).ReturnsAsync(true);

        // Act
        var result = await _controller.RemoveFileAttachment(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.True((bool)okResult.Value);
    }
}
