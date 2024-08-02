using AutoMapper;
using TMP.Application.DTOs.ProjectDtos;
using TMP.Application.DTOs.ProjectUserDtos;
using TMP.Application.DTOs.TeamDtos;
using TMPApplication.DTOs.ProjectDtos;
using TMPApplication.DTOs.UserDtos;
using TMPDomain.Entities;
using TMPDomain.HelperModels;

namespace TMP.Application.MapperProfiles
{
    public class ProjectMappingProfile : Profile
    {
        public ProjectMappingProfile()
        {
            CreateMap<Project, ProjectDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.CreatedByUserId, opt => opt.MapFrom(src => src.CreatedByUserId))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
                .ForMember(dest => dest.Columns, opt => opt.MapFrom(src => src.Columns.Select(c => c.Name).ToList()));

            CreateMap<AddProjectDto, Project>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Columns, opt => opt.Ignore());

            CreateMap<UpdateProjectDto, Project>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<ProjectUser, ProjectUserDto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role));

            CreateMap<ProjectUserDto, UserProfileResponseDto>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName));

            CreateMap<Project, ProjectUsersDto>()
                .ForMember(dest => dest.ProjectId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Users, opt => opt.MapFrom(src => src.ProjectUsers.Select(pu => new ProjectUserDto
                {
                    FirstName = pu.User.FirstName,
                    LastName = pu.User.LastName
                })));

            CreateMap<Project, ProjectTasksDto>()
                .ForMember(dest => dest.ProjectId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Tasks, opt => opt.MapFrom(src => src.Tasks));

            CreateMap<Project, ProjectTeamsDto>()
                .ForMember(dest => dest.ProjectId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Teams, opt => opt.MapFrom(src => src.ProjectTeams.Select(pt => new TeamDto
                {
                    Id = pt.Team.Id,
                    Name = pt.Team.Name,
                    Description = pt.Team.Description,
                    CreatedAt = pt.Team.CreatedAt,
                    UpdatedAt = pt.Team.UpdatedAt
                })));

            CreateMap<Column, ColumnDto>().ReverseMap();
        }
    }
}
