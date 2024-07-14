using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TMP.Application.DTOs.ProjectDtos;
using TMP.Application.DTOs.TaskDtos;
using TMP.Application.Projects;
using TMPDomain.Entities;
using TMP.Application.Interfaces;
using Task = TMPDomain.Entities.Task;
using TMPApplication.DTOs.UserDtos;

namespace TMP.Infrastructure.Implementations
{
    public class ProjectService : IProjectService
    {
        private readonly IDatabaseService _databaseService;

        public ProjectService(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<IEnumerable<ProjectDto>> GetAllProjectsAsync()
        {
            return await _databaseService.Projects
                .Include(p => p.Tasks)
                .Include(p => p.Users)
                .Select(p => new ProjectDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    CreatedByUserId = p.CreatedByUserId,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    Tasks = p.Tasks.Select(t => new TaskDto
                    {
                        Id = t.Id,
                        Title = t.Title,
                        Description = t.Description,
                        Priority = t.Priority,
                        Status = t.Status,
                        DueDate = t.DueDate,
                        CreatedAt = t.CreatedAt,
                        UpdatedAt = t.UpdatedAt,
                        ProjectId = t.ProjectId
                    }).ToList(),
                    Users = p.Users.Select(u => new UserProfileDto
                    {
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                    }).ToList()
                }).ToListAsync();
        }

        public async Task<ProjectDto> GetProjectByIdAsync(int id)
        {
            var project = await _databaseService.Projects
                .Include(p => p.Tasks)
                .Include(p => p.Users)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null) return null;

            return new ProjectDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                CreatedByUserId = project.CreatedByUserId,
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt,
                Tasks = project.Tasks.Select(t => new TaskDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Priority = t.Priority,
                    Status = t.Status,
                    DueDate = t.DueDate,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    ProjectId = t.ProjectId
                }).ToList(),
                Users = project.Users.Select(u => new UserProfileDto
                {
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                }).ToList()
            };
        }

        public async Task<ProjectDto> AddProjectAsync(AddProjectDto newProject, string userId)
        {
            var project = new Project
            {
                Name = newProject.Name,
                Description = newProject.Description,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _databaseService.Projects.Add(project);
            await _databaseService.SaveChangesAsync();

            return new ProjectDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                CreatedByUserId = project.CreatedByUserId,
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt
            };
        }

        public async Task<bool> UpdateProjectAsync(int id, AddProjectDto updatedProject)
        {
            var project = await _databaseService.Projects.FindAsync(id);
            if (project == null) return false;

            project.Name = updatedProject.Name;
            project.Description = updatedProject.Description;
            project.UpdatedAt = DateTime.UtcNow;

            await _databaseService.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteProjectAsync(int id)
        {
            var project = await _databaseService.Projects.FindAsync(id);
            if (project == null) return false;

            _databaseService.Projects.Remove(project);
            await _databaseService.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ProjectDto>> GetProjectsByUserAsync(string userId)
        {
            return await _databaseService.Projects
                .Where(p => p.CreatedByUserId == userId)
                .Include(p => p.Tasks)
                .Include(p => p.Users)
                .Select(p => new ProjectDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    CreatedByUserId = p.CreatedByUserId,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    Tasks = p.Tasks.Select(t => new TaskDto
                    {
                        Id = t.Id,
                        Title = t.Title,
                        Description = t.Description,
                        Priority = t.Priority,
                        Status = t.Status,
                        DueDate = t.DueDate,
                        CreatedAt = t.CreatedAt,
                        UpdatedAt = t.UpdatedAt,
                        ProjectId = t.ProjectId
                    }).ToList(),
                    Users = p.Users.Select(u => new UserProfileDto
                    {
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                    }).ToList()
                }).ToListAsync();
        }
    }
}
