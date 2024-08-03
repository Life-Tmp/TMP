using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TMP.Application.DTOs.ContactFormDtos;
using TMP.Application.Interfaces;
using TMPApplication.Interfaces;
using TMPApplication.Interfaces.ContactForms;
using TMPDomain.Entities;

namespace TMP.Infrastructure.Implementations.ContactForms
{
    public class ContactFormService : IContactFormService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;

        public ContactFormService(IUnitOfWork unitOfWork, IMapper mapper, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _emailService = emailService;
        }

        public async Task<IEnumerable<ContactFormDto>> GetAllContactFormsAsync()
        {
            var contactForms = await _unitOfWork.Repository<ContactForm>().GetAll().ToListAsync();
            return _mapper.Map<IEnumerable<ContactFormDto>>(contactForms);
        }

        public async Task<ContactFormDto> GetContactFormByIdAsync(int id)
        {
            var contactForm = await _unitOfWork.Repository<ContactForm>().GetById(cf => cf.Id == id).FirstOrDefaultAsync();
            if (contactForm == null) return null;

            return _mapper.Map<ContactFormDto>(contactForm);
        }

        public async Task<ContactFormDto> AddContactFormAsync(AddContactFormDto newContactForm)
        {
            var contactForm = _mapper.Map<ContactForm>(newContactForm);

            _unitOfWork.Repository<ContactForm>().Create(contactForm);
            await _unitOfWork.Repository<ContactForm>().SaveChangesAsync();

            return _mapper.Map<ContactFormDto>(contactForm);
        }

        public async Task<bool> DeleteContactFormAsync(int id)
        {
            var contactForm = await _unitOfWork.Repository<ContactForm>().GetById(cf => cf.Id == id).FirstOrDefaultAsync();
            if (contactForm == null) return false;

            _unitOfWork.Repository<ContactForm>().Delete(contactForm);
            await _unitOfWork.Repository<ContactForm>().SaveChangesAsync();
            return true;
        }

        public async Task<bool> RespondToContactFormAsync(RespondToContactFormDto respondToContactFormDto)
        {
            var contactForm = await _unitOfWork.Repository<ContactForm>().GetById(cf => cf.Id == respondToContactFormDto.ContactFormId).FirstOrDefaultAsync();
            if (contactForm == null) return false;

            var subject = "Response to Your Inquiry";
            var emailContent = $"Dear {contactForm.FirstName} {contactForm.LastName},<br/><br/>{respondToContactFormDto.ResponseMessage}<br/><br/>Best regards,<br/>LIFE TMP PROJECT 2024";

            await _emailService.SendEmail(contactForm.Email, subject, emailContent);

            // Update the contact form with the response
            contactForm.ResponseMessage = respondToContactFormDto.ResponseMessage;
            contactForm.RespondedAt = DateTime.UtcNow;

            _unitOfWork.Repository<ContactForm>().Update(contactForm);
            await _unitOfWork.Repository<ContactForm>().SaveChangesAsync();

            return true;
        }

    }
}
