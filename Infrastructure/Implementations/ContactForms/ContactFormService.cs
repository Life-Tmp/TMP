using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TMP.Application.DTOs.ContactFormDtos;
using TMP.Application.Interfaces;
using TMPApplication.Interfaces.ContactForms;
using TMPApplication.Interfaces;
using TMPDomain.Entities;
using FluentValidation;

namespace TMP.Infrastructure.Implementations.ContactForms
{
    public class ContactFormService : IContactFormService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly ILogger<ContactFormService> _logger;
        private readonly IValidator<ContactForm> _contactFormValidator;

        public ContactFormService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IEmailService emailService,
            ILogger<ContactFormService> logger,
            IValidator<ContactForm> contactFormValidator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _emailService = emailService;
            _logger = logger;
            _contactFormValidator = contactFormValidator;
        }

        #region Read
        public async Task<IEnumerable<ContactFormDto>> GetAllContactFormsAsync()
        {
            _logger.LogInformation("Fetching all contact forms");

            var contactForms = await _unitOfWork.Repository<ContactForm>().GetAll().ToListAsync();
            return _mapper.Map<IEnumerable<ContactFormDto>>(contactForms);
        }

        public async Task<ContactFormDto> GetContactFormByIdAsync(int id)
        {
            _logger.LogInformation("Fetching contact form with ID: {ContactFormId}", id);

            var contactForm = await _unitOfWork.Repository<ContactForm>().GetById(cf => cf.Id == id).FirstOrDefaultAsync();
            if (contactForm == null)
            {
                _logger.LogWarning("Contact form with ID: {ContactFormId} not found", id);
                return null;
            }

            return _mapper.Map<ContactFormDto>(contactForm);
        }
        #endregion

        #region Create
        public async Task<ContactFormDto> AddContactFormAsync(AddContactFormDto newContactFormDto)
        {
            _logger.LogInformation("Adding new contact form");

            var contactForm = _mapper.Map<ContactForm>(newContactFormDto);

            var validationResult = _contactFormValidator.Validate(contactForm);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Validation failed for adding new contact form");
                foreach (var error in validationResult.Errors)
                {
                    _logger.LogWarning("Validation Error: {Error}", error.ErrorMessage);
                }
                throw new ValidationException(validationResult.Errors);
            }

            _unitOfWork.Repository<ContactForm>().Create(contactForm);
            await _unitOfWork.Repository<ContactForm>().SaveChangesAsync();

            _logger.LogInformation("Contact form added successfully with ID: {ContactFormId}", contactForm.Id);

            return _mapper.Map<ContactFormDto>(contactForm);
        }
        #endregion

        #region Update
        public async Task<bool> RespondToContactFormAsync(RespondToContactFormDto respondToContactFormDto)
        {
            _logger.LogInformation("Responding to contact form with ID: {ContactFormId}", respondToContactFormDto.ContactFormId);

            var contactForm = await _unitOfWork.Repository<ContactForm>().GetById(cf => cf.Id == respondToContactFormDto.ContactFormId).FirstOrDefaultAsync();
            if (contactForm == null)
            {
                _logger.LogWarning("Contact form with ID: {ContactFormId} not found for response", respondToContactFormDto.ContactFormId);
                return false;
            }

            var subject = "Response to Your Inquiry";
            var emailContent = $"Dear {contactForm.FirstName} {contactForm.LastName},<br/><br/>{respondToContactFormDto.ResponseMessage}<br/><br/>Best regards,<br/>LIFE TMP PROJECT 2024";

            await _emailService.SendEmail(contactForm.Email, subject, emailContent);

            // Update the contact form with the response
            contactForm.ResponseMessage = respondToContactFormDto.ResponseMessage;
            contactForm.RespondedAt = DateTime.UtcNow;

            _unitOfWork.Repository<ContactForm>().Update(contactForm);
            await _unitOfWork.Repository<ContactForm>().SaveChangesAsync();

            _logger.LogInformation("Response to contact form with ID: {ContactFormId} sent successfully", respondToContactFormDto.ContactFormId);

            return true;
        }
        #endregion

        #region Delete
        public async Task<bool> DeleteContactFormAsync(int id)
        {
            _logger.LogInformation("Deleting contact form with ID: {ContactFormId}", id);

            var contactForm = await _unitOfWork.Repository<ContactForm>().GetById(cf => cf.Id == id).FirstOrDefaultAsync();
            if (contactForm == null)
            {
                _logger.LogWarning("Contact form with ID: {ContactFormId} not found for deletion", id);
                return false;
            }

            _unitOfWork.Repository<ContactForm>().Delete(contactForm);
            await _unitOfWork.Repository<ContactForm>().SaveChangesAsync();

            _logger.LogInformation("Contact form with ID: {ContactFormId} deleted successfully", id);

            return true;
        }
        #endregion
    }
}
