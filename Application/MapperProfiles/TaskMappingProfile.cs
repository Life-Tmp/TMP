using AutoMapper;
using TMP.Application.DTOs.TaskDtos;
using TMP.Application.DTOs.SubtaskDtos;
using TMPDomain.Entities;
using Task = TMPDomain.Entities.Task;

namespace TMP.Application.MapperProfiles
{
    public class TaskMappingProfile : Profile
    {
        public TaskMappingProfile()
        {
            CreateMap<Task, TaskDto>()
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags.Select(tag => tag.Name).ToList()))
                .ReverseMap();

            CreateMap<AddTaskDto, Task>()
                .ForMember(dest => dest.Tags, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ReverseMap();

            CreateMap<UpdateTaskDto, Task>()
                .ForMember(dest => dest.Tags, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<Subtask, SubtaskDto>().ReverseMap();
            CreateMap<AddSubtaskDto, Subtask>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ReverseMap();
        }
    }
}
