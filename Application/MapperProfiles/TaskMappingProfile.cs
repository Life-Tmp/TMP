using AutoMapper;
using TMP.Application.DTOs.TaskDtos;
using TMPDomain.Entities;
using Task = TMPDomain.Entities.Task;

namespace TMP.Application.MapperProfiles
{
    public class TaskMappingProfile : Profile
    {
        public TaskMappingProfile()
        {
            CreateMap<Task, TaskDto>().ReverseMap();
            CreateMap<AddTaskDto, Task>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ReverseMap();
        }
    }
}
