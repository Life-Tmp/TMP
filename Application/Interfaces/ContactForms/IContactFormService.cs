using TMP.Application.DTOs.ContactFormDtos;

namespace TMPApplication.Interfaces.ContactForms
{
    public interface IContactFormService
    {
        Task<IEnumerable<ContactFormDto>> GetAllContactFormsAsync();
        Task<ContactFormDto> GetContactFormByIdAsync(int id);
        Task<ContactFormDto> AddContactFormAsync(AddContactFormDto newContactForm);
        Task<bool> DeleteContactFormAsync(int id);
        Task<bool> RespondToContactFormAsync(RespondToContactFormDto respondToContactFormDto);
    }
}
