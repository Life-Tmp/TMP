using TMP.Application.DTOs.ProjectDtos;

namespace TMP.Application.DTOs.TeamDtos
{
    public class TeamProjectsDto
    {
        public int TeamId { get; set; }
        public ICollection<ProjectDto> Projects { get; set; } = new List<ProjectDto>();
    }
}
