using TMPDomain.Enumerations;

namespace TMP.Application.DTOs.ProjectUserDtos
{
    public class ProjectUserDto
    {
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public ProjectRole Role { get; set; }
    }
}
