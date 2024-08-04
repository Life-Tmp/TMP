using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TMP.Application.DTOs.ContactFormDtos;
using TMPApplication.Interfaces.ContactForms;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TMP.Service.Controllers.ContactForms
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactFormController : ControllerBase
    {
        private readonly IContactFormService _contactFormService;
        private readonly ILogger<ContactFormController> _logger;

        public ContactFormController(IContactFormService contactFormService, ILogger<ContactFormController> logger)
        {
            _contactFormService = contactFormService;
            _logger = logger;
        }

        #region Read
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ContactFormDto>>> GetContactForms()
        {
            _logger.LogInformation("Fetching all contact forms");
            var contactForms = await _contactFormService.GetAllContactFormsAsync();
            return Ok(contactForms);
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<ContactFormDto>> GetContactForm(int id)
        {
            _logger.LogInformation("Fetching contact form with ID: {ContactFormId}", id);
            var contactForm = await _contactFormService.GetContactFormByIdAsync(id);
            if (contactForm == null)
            {
                _logger.LogWarning("Contact form with ID: {ContactFormId} not found", id);
                return NotFound();
            }

            return Ok(contactForm);
        }
        #endregion

        #region Create
        [HttpPost]
        public async Task<ActionResult<ContactFormDto>> AddContactForm(AddContactFormDto newContactForm)
        {
            _logger.LogInformation("Adding new contact form");
            var contactForm = await _contactFormService.AddContactFormAsync(newContactForm);
            return CreatedAtAction(nameof(GetContactForm), new { id = contactForm.Id }, contactForm);
        }
        #endregion

        #region Update
        [Authorize(Roles = "admin")]
        [HttpPost("respond")]
        public async Task<IActionResult> RespondToContactForm([FromBody] RespondToContactFormDto respondToContactFormDto)
        {
            _logger.LogInformation("Responding to contact form with ID: {ContactFormId}", respondToContactFormDto.ContactFormId);
            var result = await _contactFormService.RespondToContactFormAsync(respondToContactFormDto);
            if (!result)
            {
                _logger.LogWarning("Contact form with ID: {ContactFormId} not found for response", respondToContactFormDto.ContactFormId);
                return NotFound();
            }

            return Ok("Response sent successfully.");
        }
        #endregion

        #region Delete
        [Authorize(Roles = "admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteContactForm(int id)
        {
            _logger.LogInformation("Deleting contact form with ID: {ContactFormId}", id);
            var result = await _contactFormService.DeleteContactFormAsync(id);
            if (!result)
            {
                _logger.LogWarning("Contact form with ID: {ContactFormId} not found for deletion", id);
                return NotFound();
            }

            _logger.LogInformation("Contact form with ID: {ContactFormId} deleted successfully", id);
            return NoContent();
        }
        #endregion
    }
}
