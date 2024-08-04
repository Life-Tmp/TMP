using AutoMapper;
using TMP.Application.DTOs.ProjectDtos;
using TMP.Application.DTOs.TeamDtos;
using TMPDomain.Entities;

namespace TMP.Application.MapperProfiles
{
    public class TeamMappingProfile : Profile
    {
        public TeamMappingProfile()
        {
            CreateMap<Team, TeamDto>().ReverseMap();
            CreateMap<AddTeamDto, Team>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ReverseMap();

            CreateMap<TeamMember, TeamMemberDto>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
                .ReverseMap();

            CreateMap<Team, TeamProjectsDto>()
                .ForMember(dest => dest.TeamId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Projects, opt => opt.MapFrom(src => src.ProjectTeams.Select(pt => new ProjectDto
                {
                    Id = pt.Project.Id,
                    Name = pt.Project.Name,
                    Description = pt.Project.Description,
                    CreatedByUserId = pt.Project.CreatedByUserId,
                    Columns = pt.Project.Columns.Select(c => c.Name).ToList()
                })));
        }
    }
}
