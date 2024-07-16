using AutoMapper;
using TMP.Application.DTOs.ProjectDtos;
using TMP.Application.DTOs.TaskDtos;
using TMPApplication.DTOs.UserDtos;
using TMPDomain.Entities;

namespace TMP.Application.MapperProfiles
{
    public class ProjectMappingProfile : Profile
    {
        public ProjectMappingProfile()
        {
            CreateMap<Project, ProjectDto>()
                .ForMember(dest => dest.Tasks, opt => opt.MapFrom(src => src.Tasks.Select(t => new TaskDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Priority = t.Priority,
                    Status = t.Status,
                    DueDate = t.DueDate,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    ProjectId = t.ProjectId
                })))
                .ForMember(dest => dest.Users, opt => opt.MapFrom(src => src.Users.Select(u => new UserProfileDto
                {
                    FirstName = u.FirstName,
                    LastName = u.LastName
                })))
                .ReverseMap();

            CreateMap<AddProjectDto, Project>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ReverseMap();
        }
    }
}
