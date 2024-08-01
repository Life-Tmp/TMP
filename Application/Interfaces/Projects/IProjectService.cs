using TMP.Application.DTOs.ProjectDtos;
using TMP.Application.DTOs.ProjectUserDtos;
using TMPApplication.DTOs.ProjectDtos;
using TMPDomain.Enumerations;

namespace TMPApplication.Interfaces.Projects
{
    public interface IProjectService
    {
        Task<IEnumerable<ProjectDto>> GetAllProjectsAsync();
        Task<ProjectDto> GetProjectByIdAsync(int id);
        Task<IEnumerable<ProjectDto>> GetProjectsByUserAsync(string userId);
        Task<string> GetUserRoleInProjectAsync(int projectId, string userId);
        Task<ProjectUsersDto> GetProjectUsersAsync(int projectId);
        Task<ProjectTeamsDto> GetProjectTeamsAsync(int projectId);
        Task<ProjectTasksDto> GetProjectTasksAsync(int projectId);
        Task<ProjectDto> AddProjectAsync(AddProjectDto newProject, string userId);
        Task<bool> AddUserToProjectAsync(AddProjectUserDto addProjectUserDto, string currentUserId);
        Task<bool> AssignTeamToProjectAsync(ManageProjectTeamDto manageProjectTeamDto);
        Task<bool> UpdateProjectAsync(int id, UpdateProjectDto updatedProject, string userId);
        Task<bool> UpdateUserRoleAsync(int projectId, string userId, MemberRole newRole, string currentUserId);
        Task<bool> DeleteProjectAsync(int id, string userId);
        Task<bool> RemoveUserFromProjectAsync(int projectId, string userId, string currentUserId);
        Task<bool> RemoveTeamFromProjectAsync(ManageProjectTeamDto manageProjectTeamDto);
    }
}
