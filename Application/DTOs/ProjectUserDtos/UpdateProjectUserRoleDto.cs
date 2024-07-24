using TMPDomain.Enumerations;

namespace TMPApplication.DTOs.ProjectUserDtos
{
    public class UpdateProjectUserRoleDto
    {
        public string UserId { get; set; }
        public ProjectRole NewRole { get; set; }
    }
}
