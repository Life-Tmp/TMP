using System.Collections.Generic;
using System.Threading.Tasks;
using TMP.Application.DTOs.ProjectDtos;

namespace TMP.Application.Projects
{
    public interface IProjectService
    {
        Task<IEnumerable<ProjectDto>> GetAllProjectsAsync();
        Task<ProjectDto> GetProjectByIdAsync(int id);
        Task<ProjectDto> AddProjectAsync(AddProjectDto newProject, string userId);
        Task<bool> UpdateProjectAsync(int id, AddProjectDto updatedProject);
        Task<bool> DeleteProjectAsync(int id);
        Task<IEnumerable<ProjectDto>> GetProjectsByUserAsync(string userId); // New method
    }
}
