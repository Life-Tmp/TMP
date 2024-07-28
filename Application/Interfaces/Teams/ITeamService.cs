using TMP.Application.DTOs.TeamDtos;
using TMPDomain.Enumerations;

namespace TMP.Application.Interfaces
{
    public interface ITeamService
    {
        Task<IEnumerable<TeamDto>> GetAllTeamsAsync();
        Task<TeamDto> GetTeamByIdAsync(int id);
        Task<IEnumerable<TeamDto>> GetUserTeamsAsync(string userId);
        Task<IEnumerable<TeamMemberDto>> GetTeamMembersAsync(int teamId);
        Task<TeamProjectsDto> GetTeamProjectsAsync(int teamId);
        Task<TeamDto> AddTeamAsync(AddTeamDto newTeam, string userId);
        Task<bool> AddUserToTeamAsync(AddTeamMemberDto addUserToTeamDto);
        Task<bool> UpdateTeamAsync(int id, AddTeamDto updatedTeam);
        Task<bool> UpdateUserRoleInTeamAsync(int teamId, string userId, MemberRole newRole);
        Task<bool> DeleteTeamAsync(int id);
        Task<bool> RemoveUserFromTeamAsync(RemoveTeamMemberDto removeUserFromTeamDto);
    }
}
