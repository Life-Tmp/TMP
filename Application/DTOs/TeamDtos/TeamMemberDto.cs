using TMPDomain.Enumerations;

namespace TMP.Application.DTOs.TeamDtos
{
    public class TeamMemberDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public MemberRole Role { get; set; }
    }
}
