using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using TMPApplication.DTOs.AtachmentDtos;
using Attachment = TMPDomain.Entities.Attachment;
namespace TMPApplication.MapperProfiles
{
    public class AttachmentMappingProfile: Profile
    {
        public AttachmentMappingProfile()
        {
            CreateMap<AddAttachmentDto, Attachment>().ForMember(dest => dest.UploadDate, opt => opt.MapFrom(src => DateTime.UtcNow)).ReverseMap();
        }
    }
}
