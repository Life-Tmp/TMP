using TMPDomain.Enumerations;

namespace TMP.Application.DTOs.TeamDtos
{
    public class UpdateTeamMemberRoleDto
    {
        public string UserId { get; set; }
        public MemberRole NewRole { get; set; }
    }
}
