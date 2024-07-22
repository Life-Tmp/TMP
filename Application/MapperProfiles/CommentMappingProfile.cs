using AutoMapper;
using TMP.Application.DTOs.CommentDtos;
using TMPDomain.Entities;

namespace TMP.Application.MapperProfiles
{
    public class CommentMappingProfile : Profile
    {
        public CommentMappingProfile()
        {
            CreateMap<Comment, CommentDto>().ReverseMap();
            CreateMap<AddCommentDto, Comment>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ReverseMap();
        }
    }
}
