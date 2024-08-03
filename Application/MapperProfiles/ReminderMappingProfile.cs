using AutoMapper;
using TMPApplication.DTOs.ReminderDtos;
using TMPDomain.Entities;

namespace TMPApplication.MapperProfiles
{
    public class ReminderMappingProfile : Profile
    {
        public ReminderMappingProfile()
        {
            CreateMap<Reminder, ReminderDto>().ReverseMap();
            CreateMap<Reminder, GetReminderDto>().ReverseMap();
        }
    }
}
