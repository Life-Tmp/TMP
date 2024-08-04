using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TMP.Application.DTOs.ContactFormDtos;
using TMPApplication.Interfaces.ContactForms;

namespace TMP.Service.Controllers.ContactForms
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactFormController : ControllerBase
    {
        private readonly IContactFormService _contactFormService;

        public ContactFormController(IContactFormService contactFormService)
        {
            _contactFormService = contactFormService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ContactFormDto>>> GetContactForms()
        {
            var contactForms = await _contactFormService.GetAllContactFormsAsync();
            return Ok(contactForms);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<ContactFormDto>> GetContactForm(int id)
        {
            var contactForm = await _contactFormService.GetContactFormByIdAsync(id);
            if (contactForm == null) return NotFound();

            return Ok(contactForm);
        }

        [HttpPost]
        public async Task<ActionResult<ContactFormDto>> AddContactForm(AddContactFormDto newContactForm)
        {
            var contactForm = await _contactFormService.AddContactFormAsync(newContactForm);
            return CreatedAtAction(nameof(GetContactForm), new { id = contactForm.Id }, contactForm);
        }

        [Authorize]
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteContactForm(int id)
        {
            var result = await _contactFormService.DeleteContactFormAsync(id);
            if (!result) return NotFound();

            return NoContent();
        }

        [Authorize]
        [HttpPost("respond")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> RespondToContactForm([FromBody] RespondToContactFormDto respondToContactFormDto)
        {
            var result = await _contactFormService.RespondToContactFormAsync(respondToContactFormDto);
            if (!result) return NotFound();

            return Ok("Response sent successfully.");
        }
    }
}
