using TMP.Application.DTOs.ProjectDtos;
using TMP.Application.DTOs.ProjectUserDtos;
using TMPDomain.Enumerations;

namespace TMPApplication.Interfaces.Projects
{
    public interface IProjectService
    {
        Task<IEnumerable<ProjectDto>> GetAllProjectsAsync();
        Task<ProjectDto> GetProjectByIdAsync(int id);
        Task<ProjectDto> AddProjectAsync(AddProjectDto newProject, string userId);
        Task<bool> UpdateProjectAsync(int id, AddProjectDto updatedProject, string userId);
        Task<bool> DeleteProjectAsync(int id, string userId);
        Task<IEnumerable<ProjectDto>> GetProjectsByUserAsync(string userId);
        Task<string> GetUserRoleInProjectAsync(int projectId, string userId);
        Task<bool> AddUserToProjectAsync(AddProjectUserDto addProjectUserDto, string currentUserId);
        Task<bool> UpdateUserRoleAsync(int projectId, string userId, ProjectRole newRole, string currentUserId);
        Task<bool> RemoveUserFromProjectAsync(int projectId, string userId, string currentUserId);
        Task<ProjectUsersDto> GetProjectUsersAsync(int projectId);
        Task<ProjectTasksDto> GetProjectTasksAsync(int projectId);
    }
}
