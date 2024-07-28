using TMP.Application.DTOs.TeamDtos;

namespace TMP.Application.DTOs.ProjectDtos
{
    public class ProjectTeamsDto
    {
        public int ProjectId { get; set; }
        public ICollection<TeamDto> Teams { get; set; } = new List<TeamDto>();
    }
}
