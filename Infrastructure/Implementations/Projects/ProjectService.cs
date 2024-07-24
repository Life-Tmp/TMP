using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TMP.Application.DTOs.ProjectDtos;
using TMP.Application.DTOs.ProjectUserDtos;
using TMP.Application.Interfaces;
using TMPApplication.Interfaces.Projects;
using TMPDomain.Entities;
using TMPDomain.Enumerations;

namespace TMPInfrastructure.Implementations.Projects
{
    public class ProjectService : IProjectService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProjectService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProjectDto>> GetAllProjectsAsync()
        {
            var projects = await _unitOfWork.Repository<Project>().GetAll()
                .Select(p => new ProjectDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    CreatedByUserId = p.CreatedByUserId
                }).ToListAsync();

            return projects;
        }

        public async Task<ProjectDto> GetProjectByIdAsync(int id)
        {
            var project = await _unitOfWork.Repository<Project>()
                .GetById(p => p.Id == id)
                .Select(p => new ProjectDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    CreatedByUserId = p.CreatedByUserId
                }).FirstOrDefaultAsync();

            return project;
        }

        public async Task<ProjectDto> AddProjectAsync(AddProjectDto newProject, string userId)
        {
            var project = _mapper.Map<Project>(newProject);
            project.CreatedByUserId = userId;

            _unitOfWork.Repository<Project>().Create(project);
            await _unitOfWork.Repository<Project>().SaveChangesAsync();

            // Add the authenticated user as an admin
            var projectUser = new ProjectUser
            {
                ProjectId = project.Id,
                UserId = userId,
                Role = ProjectRole.Admin
            };

            _unitOfWork.Repository<ProjectUser>().Create(projectUser);
            await _unitOfWork.Repository<ProjectUser>().SaveChangesAsync();

            return _mapper.Map<ProjectDto>(project);
        }

        public async Task<bool> UpdateProjectAsync(int id, AddProjectDto updatedProject, string userId)
        {
            var project = await _unitOfWork.Repository<Project>().GetById(p => p.Id == id).FirstOrDefaultAsync();
            if (project == null) return false;

            var projectUser = await _unitOfWork.Repository<ProjectUser>()
                .GetByCondition(pu => pu.ProjectId == id && pu.UserId == userId)
                .FirstOrDefaultAsync();

            if (projectUser == null || projectUser.Role != ProjectRole.Admin)
                return false;

            _mapper.Map(updatedProject, project);
            project.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<Project>().Update(project);
            await _unitOfWork.Repository<Project>().SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteProjectAsync(int id, string userId)
        {
            var project = await _unitOfWork.Repository<Project>().GetById(p => p.Id == id).FirstOrDefaultAsync();
            if (project == null) return false;

            var projectUser = await _unitOfWork.Repository<ProjectUser>()
                .GetByCondition(pu => pu.ProjectId == id && pu.UserId == userId)
                .FirstOrDefaultAsync();

            if (projectUser == null || projectUser.Role != ProjectRole.Admin)
                return false;

            _unitOfWork.Repository<Project>().Delete(project);
            await _unitOfWork.Repository<Project>().SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ProjectDto>> GetProjectsByUserAsync(string userId)
        {
            var projects = await _unitOfWork.Repository<Project>()
                .GetByCondition(p => p.ProjectUsers.Any(pu => pu.UserId == userId))
                .Select(p => new ProjectDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    CreatedByUserId = p.CreatedByUserId
                }).ToListAsync();

            return projects;
        }


        public async Task<string> GetUserRoleInProjectAsync(int projectId, string userId)
        {
            var projectUser = await _unitOfWork.Repository<ProjectUser>()
                .GetByCondition(pu => pu.ProjectId == projectId && pu.UserId == userId)
                .FirstOrDefaultAsync();

            return projectUser?.Role.ToString();
        }

        public async Task<bool> AddUserToProjectAsync(AddProjectUserDto addProjectUserDto, string currentUserId)
        {
            var project = await _unitOfWork.Repository<Project>().GetById(p => p.Id == addProjectUserDto.ProjectId).FirstOrDefaultAsync();
            if (project == null) return false;

            var user = await _unitOfWork.Repository<User>().GetById(u => u.Id == addProjectUserDto.UserId).FirstOrDefaultAsync();
            if (user == null) return false;

            var projectUser = new ProjectUser
            {
                ProjectId = addProjectUserDto.ProjectId,
                UserId = addProjectUserDto.UserId,
                Role = ProjectRole.Member
            };

            _unitOfWork.Repository<ProjectUser>().Create(projectUser);
            await _unitOfWork.Repository<ProjectUser>().SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateUserRoleAsync(int projectId, string userId, ProjectRole newRole, string currentUserId)
        {
            var currentUserRole = await _unitOfWork.Repository<ProjectUser>()
                .GetByCondition(pu => pu.ProjectId == projectId && pu.UserId == currentUserId)
                .Select(pu => pu.Role)
                .FirstOrDefaultAsync();

            if (currentUserRole != ProjectRole.Admin)
                return false;

            var projectUser = await _unitOfWork.Repository<ProjectUser>()
                .GetByCondition(pu => pu.ProjectId == projectId && pu.UserId == userId)
                .FirstOrDefaultAsync();

            if (projectUser == null)
                return false;

            projectUser.Role = newRole;
            _unitOfWork.Repository<ProjectUser>().Update(projectUser);
            await _unitOfWork.Repository<ProjectUser>().SaveChangesAsync();

            return true;
        }

        public async Task<ProjectUsersDto> GetProjectUsersAsync(int projectId)
        {
            var project = await _unitOfWork.Repository<Project>()
                .GetById(p => p.Id == projectId)
                .Include(p => p.ProjectUsers).ThenInclude(pu => pu.User)
                .FirstOrDefaultAsync();

            if (project == null) return null;

            return _mapper.Map<ProjectUsersDto>(project);
        }

        public async Task<ProjectTasksDto> GetProjectTasksAsync(int projectId)
        {
            var project = await _unitOfWork.Repository<Project>()
                .GetById(p => p.Id == projectId)
                .Include(p => p.Tasks)
                .FirstOrDefaultAsync();

            if (project == null) return null;

            return _mapper.Map<ProjectTasksDto>(project);
        }

        public async Task<bool> RemoveUserFromProjectAsync(int projectId, string userId, string currentUserId)
        {
            // Check if the current user is an admin in the project
            var currentUserRole = await _unitOfWork.Repository<ProjectUser>()
                .GetByCondition(pu => pu.ProjectId == projectId && pu.UserId == currentUserId)
                .Select(pu => pu.Role)
                .FirstOrDefaultAsync();

            if (currentUserRole != ProjectRole.Admin)
                return false;

            var projectUser = await _unitOfWork.Repository<ProjectUser>()
                .GetByCondition(pu => pu.ProjectId == projectId && pu.UserId == userId)
                .FirstOrDefaultAsync();

            if (projectUser == null)
                return false;

            _unitOfWork.Repository<ProjectUser>().Delete(projectUser);
            await _unitOfWork.Repository<ProjectUser>().SaveChangesAsync();

            return true;
        }

    }
}
