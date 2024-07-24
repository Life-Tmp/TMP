using TMPApplication.DTOs.UserDtos;

namespace TMP.Application.DTOs.ProjectDtos
{
    public class ProjectUsersDto
    {
        public int ProjectId { get; set; }
        public ICollection<UserProfileDto> Users { get; set; } = new List<UserProfileDto>();
    }
}
