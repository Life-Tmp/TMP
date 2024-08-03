using AutoMapper;
using TMP.Application.DTOs.ContactFormDtos;
using TMPDomain.Entities;

namespace TMP.Application.MapperProfiles
{
    public class ContactFormMappingProfile : Profile
    {
        public ContactFormMappingProfile()
        {
            CreateMap<ContactForm, ContactFormDto>();
            CreateMap<AddContactFormDto, ContactForm>();
        }
    }
}
