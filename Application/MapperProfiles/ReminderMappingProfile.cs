using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
