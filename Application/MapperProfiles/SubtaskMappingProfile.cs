using AutoMapper;
using TMP.Application.DTOs.SubtaskDtos;
using TMPDomain.Entities;

namespace TMP.Application.MapperProfiles
{
    public class SubtaskMappingProfile : Profile
    {
        public SubtaskMappingProfile()
        {
            CreateMap<AddSubtaskDto, Subtask>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.IsCompleted, opt => opt.MapFrom(src => false));

            CreateMap<UpdateSubtaskDto, Subtask>()
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Subtask, SubtaskDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.IsCompleted, opt => opt.MapFrom(src => src.IsCompleted))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.CompletedAt, opt => opt.MapFrom(src => src.CompletedAt)); 
        }
    }
}
