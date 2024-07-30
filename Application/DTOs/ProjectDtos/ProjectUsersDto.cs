using TMP.Application.DTOs.ProjectUserDtos;
using TMPApplication.DTOs.UserDtos;

namespace TMP.Application.DTOs.ProjectDtos
{
    public class ProjectUsersDto
    {
        public int ProjectId { get; set; }
        public ICollection<ProjectUserDto> Users { get; set; } = new List<ProjectUserDto>();
    }
}
