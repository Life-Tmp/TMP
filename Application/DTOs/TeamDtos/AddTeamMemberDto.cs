using TMPDomain.Enumerations;

namespace TMP.Application.DTOs.TeamDtos
{
    public class AddTeamMemberDto
    {
        public int TeamId { get; set; }
        public string UserId { get; set; }
        public MemberRole Role { get; set; }
    }
}
