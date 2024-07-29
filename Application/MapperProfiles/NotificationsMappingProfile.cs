using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPApplication.DTOs.NotificationDtos;
using TMPDomain.Entities;

namespace TMPApplication.MapperProfiles
{
    public class NotificationsMappingProfile : Profile
    {
        public NotificationsMappingProfile()
        {
            CreateMap<Notification, NotificationDto>().ReverseMap();
        }
    }
}
