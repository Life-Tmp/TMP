using AutoMapper;
using TMP.Application.DTOs.TagDtos;
using TMPDomain.Entities;

namespace TMP.Application.MapperProfiles
{
    public class TagMappingProfile : Profile
    {
        public TagMappingProfile()
        {
            CreateMap<Tag, TagDto>().ReverseMap();
            CreateMap<AddTagDto, Tag>().ReverseMap();
        }
    }
}
