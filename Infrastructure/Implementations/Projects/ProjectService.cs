using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TMP.Application.DTOs.ProjectDtos;
using TMP.Application.DTOs.ProjectUserDtos;
using TMP.Application.Interfaces;
using TMPApplication.DTOs.ProjectDtos;
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

        public async Task<ProjectUsersDto> GetProjectUsersAsync(int projectId)
        {
            var project = await _unitOfWork.Repository<Project>()
                .GetById(p => p.Id == projectId)
                .Include(p => p.ProjectUsers).ThenInclude(pu => pu.User)
                .FirstOrDefaultAsync();

            if (project == null) return null;

            return _mapper.Map<ProjectUsersDto>(project);
        }

        public async Task<ProjectTeamsDto> GetProjectTeamsAsync(int projectId)
        {
            var project = await _unitOfWork.Repository<Project>()
                .GetById(p => p.Id == projectId)
                .Include(p => p.ProjectTeams)
                    .ThenInclude(pt => pt.Team)
                .FirstOrDefaultAsync();

            if (project == null) return null;

            return _mapper.Map<ProjectTeamsDto>(project);
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

        public async Task<ProjectDto> AddProjectAsync(AddProjectDto newProject, string userId)
        {
            var project = _mapper.Map<Project>(newProject);
            project.CreatedByUserId = userId;

            _unitOfWork.Repository<Project>().Create(project);
            await _unitOfWork.Repository<Project>().SaveChangesAsync();

            var projectUser = new ProjectUser
            {
                ProjectId = project.Id,
                UserId = userId,
                Role = MemberRole.TeamLead
            };

            _unitOfWork.Repository<ProjectUser>().Create(projectUser);
            await _unitOfWork.Repository<ProjectUser>().SaveChangesAsync();

            return _mapper.Map<ProjectDto>(project);
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
                Role = addProjectUserDto.Role
            };

            _unitOfWork.Repository<ProjectUser>().Create(projectUser);
            await _unitOfWork.Repository<ProjectUser>().SaveChangesAsync();

            return true;
        }

        public async Task<bool> AssignTeamToProjectAsync(ManageProjectTeamDto manageProjectTeamDto)
        {
            var project = await _unitOfWork.Repository<Project>()
                .GetById(p => p.Id == manageProjectTeamDto.ProjectId)
                .Include(p => p.ProjectUsers)
                .FirstOrDefaultAsync();

            if (project == null)
                return false;

            var team = await _unitOfWork.Repository<Team>()
                .GetById(t => t.Id == manageProjectTeamDto.TeamId)
                .Include(t => t.TeamMembers)
                .ThenInclude(tm => tm.User)
                .FirstOrDefaultAsync();

            if (team == null)
                return false;

            foreach (var teamMember in team.TeamMembers)
            {
                if (!project.ProjectUsers.Any(pu => pu.UserId == teamMember.UserId))
                {
                    var projectUser = new ProjectUser
                    {
                        ProjectId = project.Id,
                        UserId = teamMember.UserId,
                        Role = teamMember.Role
                    };
                    _unitOfWork.Repository<ProjectUser>().Create(projectUser);
                }
            }

            _unitOfWork.Repository<ProjectTeam>().Create(new ProjectTeam
            {
                ProjectId = project.Id,
                TeamId = team.Id
            });

            await _unitOfWork.Repository<Project>().SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateProjectAsync(int id, AddProjectDto updatedProject, string userId)
        {
            var project = await _unitOfWork.Repository<Project>().GetById(p => p.Id == id).FirstOrDefaultAsync();
            if (project == null) return false;

            var projectUser = await _unitOfWork.Repository<ProjectUser>()
                .GetByCondition(pu => pu.ProjectId == id && pu.UserId == userId)
                .FirstOrDefaultAsync();

            if (projectUser == null || projectUser.Role != MemberRole.TeamLead)
                return false;

            _mapper.Map(updatedProject, project);
            project.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<Project>().Update(project);
            await _unitOfWork.Repository<Project>().SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateUserRoleAsync(int projectId, string userId, MemberRole newRole, string currentUserId)
        {
            var currentUserRole = await _unitOfWork.Repository<ProjectUser>()
                .GetByCondition(pu => pu.ProjectId == projectId && pu.UserId == currentUserId)
                .Select(pu => pu.Role)
                .FirstOrDefaultAsync();

            if (currentUserRole != MemberRole.TeamLead)
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

        public async Task<bool> DeleteProjectAsync(int id, string userId)
        {
            var project = await _unitOfWork.Repository<Project>().GetById(p => p.Id == id).FirstOrDefaultAsync();
            if (project == null) return false;

            var projectUser = await _unitOfWork.Repository<ProjectUser>()
                .GetByCondition(pu => pu.ProjectId == id && pu.UserId == userId)
                .FirstOrDefaultAsync();

            if (projectUser == null || projectUser.Role != MemberRole.TeamLead)
                return false;

            _unitOfWork.Repository<Project>().Delete(project);
            await _unitOfWork.Repository<Project>().SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveUserFromProjectAsync(int projectId, string userId, string currentUserId)
        {
            var currentUserRole = await _unitOfWork.Repository<ProjectUser>()
                .GetByCondition(pu => pu.ProjectId == projectId && pu.UserId == currentUserId)
                .Select(pu => pu.Role)
                .FirstOrDefaultAsync();

            if (currentUserRole != MemberRole.TeamLead)
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

        public async Task<bool> RemoveTeamFromProjectAsync(ManageProjectTeamDto manageProjectTeamDto)
        {
            var projectTeam = await _unitOfWork.Repository<ProjectTeam>()
                .GetByCondition(pt => pt.ProjectId == manageProjectTeamDto.ProjectId && pt.TeamId == manageProjectTeamDto.TeamId)
                .Include(pt => pt.Team)
                .ThenInclude(t => t.TeamMembers)
                .FirstOrDefaultAsync();

            if (projectTeam == null || projectTeam.Team == null || !projectTeam.Team.TeamMembers.Any())
                return false;

            var teamMembers = projectTeam.Team.TeamMembers;

            var project = await _unitOfWork.Repository<Project>()
                .GetById(p => p.Id == manageProjectTeamDto.ProjectId)
                .FirstOrDefaultAsync();

            if (project == null)
                return false;

            _unitOfWork.Repository<ProjectTeam>().Delete(projectTeam);

            foreach (var teamMember in teamMembers)
            {
                if (teamMember.UserId == project.CreatedByUserId)
                {
                    continue;
                }

                var otherTeams = await _unitOfWork.Repository<ProjectTeam>()
                    .GetByCondition(pt => pt.ProjectId == manageProjectTeamDto.ProjectId && pt.TeamId != manageProjectTeamDto.TeamId)
                    .Include(pt => pt.Team.TeamMembers)
                    .ToListAsync();

                bool isUserInOtherTeams = otherTeams.Any(pt => pt.Team.TeamMembers.Any(tm => tm.UserId == teamMember.UserId));

                if (!isUserInOtherTeams)
                {
                    var projectUser = await _unitOfWork.Repository<ProjectUser>()
                        .GetByCondition(pu => pu.ProjectId == manageProjectTeamDto.ProjectId && pu.UserId == teamMember.UserId)
                        .FirstOrDefaultAsync();

                    if (projectUser != null)
                    {
                        _unitOfWork.Repository<ProjectUser>().Delete(projectUser);
                    }
                }
            }

            await _unitOfWork.Repository<Project>().SaveChangesAsync();

            return true;
        }

    }
}
