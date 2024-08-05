using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMP.Application.DTOs.ContactFormDtos;
using TMPApplication.Interfaces.ContactForms;
using TMP.Service.Controllers.ContactForms;
using Xunit;

namespace TMP.Service.Tests
{
    public class ContactFormController_UnitTest
    {
        private readonly Mock<IContactFormService> _mockContactFormService;
        private readonly Mock<ILogger<ContactFormController>> _mockLogger;
        private readonly ContactFormController _controller;

        public ContactFormController_UnitTest()
        {
            _mockContactFormService = new Mock<IContactFormService>();
            _mockLogger = new Mock<ILogger<ContactFormController>>();
            _controller = new ContactFormController(_mockContactFormService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetContactForms_ReturnsOkResult_WithListOfContactForms()
        {
            // Arrange
            var contactForms = new List<ContactFormDto> { new ContactFormDto { Id = 1, FirstName = "John Doe" } };
            _mockContactFormService.Setup(service => service.GetAllContactFormsAsync())
                .ReturnsAsync(contactForms);

            // Act
            var result = await _controller.GetContactForms();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<List<ContactFormDto>>(okResult.Value);
            Assert.Single(returnValue);
        }

        [Fact]
        public async Task GetContactForm_ReturnsOkResult_WithContactForm()
        {
            // Arrange
            var contactForm = new ContactFormDto { Id = 1, FirstName = "John Doe" };
            _mockContactFormService.Setup(service => service.GetContactFormByIdAsync(1))
                .ReturnsAsync(contactForm);

            // Act
            var result = await _controller.GetContactForm(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ContactFormDto>(okResult.Value);
            Assert.Equal(1, returnValue.Id);
        }

        [Fact]
        public async Task GetContactForm_ReturnsNotFoundResult_WhenContactFormNotFound()
        {
            // Arrange
            _mockContactFormService.Setup(service => service.GetContactFormByIdAsync(1))
                .ReturnsAsync((ContactFormDto)null);

            // Act
            var result = await _controller.GetContactForm(1);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task AddContactForm_ReturnsCreatedAtActionResult_WithContactForm()
        {
            // Arrange
            var newContactForm = new AddContactFormDto { FirstName = "John Doe" };
            var contactForm = new ContactFormDto { Id = 1, FirstName = "John Doe" };
            _mockContactFormService.Setup(service => service.AddContactFormAsync(newContactForm))
                .ReturnsAsync(contactForm);

            // Act
            var result = await _controller.AddContactForm(newContactForm);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnValue = Assert.IsType<ContactFormDto>(createdAtActionResult.Value);
            Assert.Equal(1, returnValue.Id);
        }

        [Fact]
        public async Task RespondToContactForm_ReturnsOkResult_WhenResponseIsSent()
        {
            // Arrange
            var respondToContactFormDto = new RespondToContactFormDto { ContactFormId = 1, ResponseMessage = "Response" };
            _mockContactFormService.Setup(service => service.RespondToContactFormAsync(respondToContactFormDto))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.RespondToContactForm(respondToContactFormDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Response sent successfully.", okResult.Value);
        }

        [Fact]
        public async Task RespondToContactForm_ReturnsNotFoundResult_WhenContactFormNotFound()
        {
            // Arrange
            var respondToContactFormDto = new RespondToContactFormDto { ContactFormId = 1, ResponseMessage = "Response" };
            _mockContactFormService.Setup(service => service.RespondToContactFormAsync(respondToContactFormDto))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.RespondToContactForm(respondToContactFormDto);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteContactForm_ReturnsNoContentResult_WhenContactFormIsDeleted()
        {
            // Arrange
            _mockContactFormService.Setup(service => service.DeleteContactFormAsync(1))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteContactForm(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteContactForm_ReturnsNotFoundResult_WhenContactFormNotFound()
        {
            // Arrange
            _mockContactFormService.Setup(service => service.DeleteContactFormAsync(1))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteContactForm(1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}