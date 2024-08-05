using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TMP.Application.DTOs.ProjectDtos;
using TMP.Application.DTOs.ProjectUserDtos;
using TMP.Application.Interfaces;
using TMPApplication.DTOs.ProjectDtos;
using TMPApplication.Interfaces;
using TMPApplication.Interfaces.Projects;
using TMPCommon.Constants;
using TMPDomain.Entities;
using TMPDomain.Enumerations;
using TMPInfrastructure.Implementations.CalendarApi;

namespace TMPInfrastructure.Implementations.Projects
{
    public class ProjectService : IProjectService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ISearchService<ProjectDto> _searchService;
        private readonly ILogger<ProjectService> _logger;
        private readonly IValidator<Project> _projectValidator;
        private readonly IValidator<Column> _columnValidator;
        private readonly ICacheService _cache;
        //private readonly IGoogleCalendarService _googleCalendarService;

        public ProjectService(IUnitOfWork unitOfWork, IMapper mapper, ISearchService<ProjectDto> searchService, ILogger<ProjectService> logger, ICacheService cache, IValidator<Project> projectValidator, IValidator<Column> columnValidator)

        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _searchService = searchService;
            _logger = logger;
            _cache = cache;
            //_googleCalendarService = googleCalendarService;
            _projectValidator = projectValidator;
            _columnValidator = columnValidator;
        }

        #region Read
        public async Task<IEnumerable<ProjectDto>> GetAllProjectsAsync()
        {
            _logger.LogInformation("Fetching all projects");

            var cacheKey = ProjectsConstants.AllProjects;
            var cachedProjects = await _cache.GetAsync<IEnumerable<ProjectDto>>(cacheKey);
            if (cachedProjects != null)
            {
                _logger.LogInformation("All projects found in cache");
                return cachedProjects;
            }

            var projects = await _unitOfWork.Repository<Project>()
                                            .GetAll()
                                            .Include(p => p.Columns)
                                            .ToListAsync();

            var projectDtos = _mapper.Map<IEnumerable<ProjectDto>>(projects);
            await _cache.SetAsync(cacheKey, projectDtos, TimeSpan.FromMinutes(60));

            return projectDtos;
        }

        public async Task<ProjectDto> GetProjectByIdAsync(int id)
        {
            _logger.LogInformation("Fetching project with ID: {ProjectId}", id);

            var cacheKey = string.Format(ProjectsConstants.ProjectById, id);
            var cachedProject = await _cache.GetAsync<ProjectDto>(cacheKey);
            if (cachedProject != null)
            {
                _logger.LogInformation("Project with ID: {ProjectId} found in cache", id);
                return cachedProject;
            }

            var project = await _unitOfWork.Repository<Project>()
                                           .GetById(p => p.Id == id)
                                           .Include(p => p.Columns)
                                           .FirstOrDefaultAsync();
            if (project == null)
            {
                _logger.LogWarning("Project with ID: {ProjectId} not found", id);
                return null;
            }

            var projectDto = _mapper.Map<ProjectDto>(project);
            await _cache.SetAsync(cacheKey, projectDto, TimeSpan.FromMinutes(60));

            return projectDto;
        }

        public async Task<IEnumerable<ProjectDto>> GetProjectsByUserAsync(string userId)
        {
            _logger.LogInformation("Fetching projects for user {UserId}", userId);

            var cacheKey = string.Format(ProjectsConstants.ProjectsByUser, userId);
            var cachedProjects = await _cache.GetAsync<IEnumerable<ProjectDto>>(cacheKey);
            if (cachedProjects != null)
            {
                _logger.LogInformation("Projects for user {UserId} found in cache", userId);
                return cachedProjects;
            }

            var projects = await _unitOfWork.Repository<Project>()
                .GetByCondition(p => p.ProjectUsers.Any(pu => pu.UserId == userId))
                .Include(p => p.Columns)
                .ToListAsync();

            var projectDtos = _mapper.Map<IEnumerable<ProjectDto>>(projects);
            await _cache.SetAsync(cacheKey, projectDtos, TimeSpan.FromMinutes(60));

            return projectDtos;
        }

        public async Task<string> GetUserRoleInProjectAsync(int projectId, string userId)
        {
            _logger.LogInformation("Fetching user role for user {UserId} in project {ProjectId}", userId, projectId);

            var cacheKey = string.Format(ProjectsConstants.UserRoleInProject, projectId, userId);
            var cachedUserRole = await _cache.GetAsync<string>(cacheKey);
            if (cachedUserRole != null)
            {
                _logger.LogInformation("User role for user {UserId} in project {ProjectId} found in cache", userId, projectId);
                return cachedUserRole;
            }

            var projectUser = await _unitOfWork.Repository<ProjectUser>()
                .GetByCondition(pu => pu.ProjectId == projectId && pu.UserId == userId)
                .FirstOrDefaultAsync();

            var userRole = projectUser?.Role.ToString();
            await _cache.SetAsync(cacheKey, userRole, TimeSpan.FromMinutes(60));

            return userRole;
        }

        public async Task<ProjectUsersDto> GetProjectUsersAsync(int projectId)
        {
            _logger.LogInformation("Fetching users for project {ProjectId}", projectId);

            var cacheKey = string.Format(ProjectsConstants.ProjectUsers, projectId);
            var cachedProjectUsers = await _cache.GetAsync<ProjectUsersDto>(cacheKey);
            if (cachedProjectUsers != null)
            {
                _logger.LogInformation("Users for project {ProjectId} found in cache", projectId);
                return cachedProjectUsers;
            }

            var project = await _unitOfWork.Repository<Project>()
                .GetById(p => p.Id == projectId)
                .Include(p => p.ProjectUsers).ThenInclude(pu => pu.User)
                .FirstOrDefaultAsync();

            if (project == null)
            {
                _logger.LogWarning("Project with ID: {ProjectId} not found", projectId);
                return null;
            }

            var projectUsersDto = _mapper.Map<ProjectUsersDto>(project);
            await _cache.SetAsync(cacheKey, projectUsersDto, TimeSpan.FromMinutes(60));

            return projectUsersDto;
        }

        public async Task<ProjectTeamsDto> GetProjectTeamsAsync(int projectId)
        {
            _logger.LogInformation("Fetching teams for project {ProjectId}", projectId);

            var cacheKey = string.Format(ProjectsConstants.ProjectTeams, projectId);
            var cachedProjectTeams = await _cache.GetAsync<ProjectTeamsDto>(cacheKey);
            if (cachedProjectTeams != null)
            {
                _logger.LogInformation("Teams for project {ProjectId} found in cache", projectId);
                return cachedProjectTeams;
            }

            var project = await _unitOfWork.Repository<Project>()
                .GetById(p => p.Id == projectId)
                .Include(p => p.ProjectTeams)
                    .ThenInclude(pt => pt.Team)
                .FirstOrDefaultAsync();

            if (project == null)
            {
                _logger.LogWarning("Project with ID: {ProjectId} not found", projectId);
                return null;
            }

            var projectTeamsDto = _mapper.Map<ProjectTeamsDto>(project);
            await _cache.SetAsync(cacheKey, projectTeamsDto, TimeSpan.FromMinutes(60));

            return projectTeamsDto;
        }

        public async Task<ProjectTasksDto> GetProjectTasksAsync(int projectId)
        {
            _logger.LogInformation("Fetching tasks for project {ProjectId}", projectId);

            var cacheKey = string.Format(ProjectsConstants.ProjectTasks, projectId);
            var cachedProjectTasks = await _cache.GetAsync<ProjectTasksDto>(cacheKey);
            if (cachedProjectTasks != null)
            {
                _logger.LogInformation("Tasks for project {ProjectId} found in cache", projectId);
                return cachedProjectTasks;
            }

            var project = await _unitOfWork.Repository<Project>()
                .GetById(p => p.Id == projectId)
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.Tags)
                .FirstOrDefaultAsync();

            if (project == null)
            {
                _logger.LogWarning("Project with ID: {ProjectId} not found", projectId);
                return null;
            }

            var projectTasksDto = _mapper.Map<ProjectTasksDto>(project);
            await _cache.SetAsync(cacheKey, projectTasksDto, TimeSpan.FromMinutes(60));

            return projectTasksDto;
        }
        #endregion

        #region Create
        public async Task<ProjectDto> AddProjectAsync(AddProjectDto newProject, string userId)
        {
            _logger.LogInformation("Adding a new project by user {UserId}", userId);

            var project = _mapper.Map<Project>(newProject);
            project.CreatedByUserId = userId;

            var validationResult = await _projectValidator.ValidateAsync(project);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Validation failed for Project entity: {Errors}", validationResult.Errors);
                throw new ValidationException(validationResult.Errors);
            }

            project.Columns = new List<Column>();

            var defaultColumns = new List<string> { "To Do", "In Progress", "Done" };
            foreach (var columnName in defaultColumns)
            {
                var existingColumn = await _unitOfWork.Repository<Column>().GetByCondition(c => c.Name == columnName).FirstOrDefaultAsync();
                if (existingColumn != null)
                {
                    var columnValidationResult = await _columnValidator.ValidateAsync(existingColumn);
                    if (!columnValidationResult.IsValid)
                    {
                        _logger.LogWarning("Validation failed for Column entity: {Errors}", columnValidationResult.Errors);
                        throw new ValidationException(columnValidationResult.Errors);
                    }

                    project.Columns.Add(existingColumn);
                }
                else
                {
                    var newColumn = new Column { Name = columnName };
                    _unitOfWork.Repository<Column>().Create(newColumn);
                    await _unitOfWork.Repository<Column>().SaveChangesAsync();
                    project.Columns.Add(newColumn);
                }
            }

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

            var projectDto = _mapper.Map<ProjectDto>(project);
            await _searchService.IndexDocumentAsync(projectDto, "projects");

            await _cache.DeleteKeyAsync(ProjectsConstants.AllProjects);

            _logger.LogInformation("Project {ProjectId} added successfully", project.Id);
            return projectDto;
        }

        public async Task<bool> AddColumnsToProjectAsync(ManageProjectColumnsDto addProjectColumnDto)
        {
            _logger.LogInformation("Adding columns to project {ProjectId}", addProjectColumnDto.ProjectId);

            var project = await _unitOfWork.Repository<Project>()
                                           .GetById(p => p.Id == addProjectColumnDto.ProjectId)
                                           .Include(p => p.Columns)
                                           .FirstOrDefaultAsync();
            if (project == null)
            {
                _logger.LogWarning("Project with ID: {ProjectId} not found", addProjectColumnDto.ProjectId);
                return false;
            }

            foreach (var columnName in addProjectColumnDto.ColumnNames)
            {
                var existingColumn = await _unitOfWork.Repository<Column>()
                                                      .GetByCondition(c => c.Name == columnName)
                                                      .FirstOrDefaultAsync();
                if (existingColumn != null)
                {
                    var columnValidationResult = await _columnValidator.ValidateAsync(existingColumn);
                    if (!columnValidationResult.IsValid)
                    {
                        _logger.LogWarning("Validation failed for Column entity: {Errors}", columnValidationResult.Errors);
                        throw new ValidationException(columnValidationResult.Errors);
                    }

                    if (!project.Columns.Any(c => c.Name == columnName))
                    {
                        project.Columns.Add(existingColumn);
                    }
                }
                else
                {
                    var newColumn = new Column { Name = columnName };
                    _unitOfWork.Repository<Column>().Create(newColumn);
                    await _unitOfWork.Repository<Column>().SaveChangesAsync();
                    project.Columns.Add(newColumn);
                }
            }

            _unitOfWork.Repository<Project>().Update(project);
            await _unitOfWork.Repository<Project>().SaveChangesAsync();

            await _cache.DeleteKeyAsync(string.Format(ProjectsConstants.ProjectById, project.Id));
            await _cache.DeleteKeyAsync(ProjectsConstants.AllProjects);

            return true;
        }

        public async Task<bool> AddUserToProjectAsync(AddProjectUserDto addProjectUserDto, string currentUserId)
        {
            _logger.LogInformation("Adding user {UserId} to project {ProjectId} by user {CurrentUserId}", addProjectUserDto.UserId, addProjectUserDto.ProjectId, currentUserId);

            var project = await _unitOfWork.Repository<Project>().GetById(p => p.Id == addProjectUserDto.ProjectId).FirstOrDefaultAsync();
            if (project == null)
            {
                _logger.LogWarning("Project with ID: {ProjectId} not found", addProjectUserDto.ProjectId);
                return false;
            }

            var user = await _unitOfWork.Repository<User>().GetById(u => u.Id == addProjectUserDto.UserId).FirstOrDefaultAsync();
            if (user == null)
            {
                _logger.LogWarning("User with ID: {UserId} not found", addProjectUserDto.UserId);
                return false;
            }

            var projectUser = new ProjectUser
            {
                ProjectId = addProjectUserDto.ProjectId,
                UserId = addProjectUserDto.UserId,
                Role = addProjectUserDto.Role
            };

            _unitOfWork.Repository<ProjectUser>().Create(projectUser);
            await _unitOfWork.Repository<ProjectUser>().SaveChangesAsync();

            await _cache.DeleteKeyAsync(string.Format(ProjectsConstants.ProjectUsers, addProjectUserDto.ProjectId));
            await _cache.DeleteKeyAsync(string.Format(ProjectsConstants.ProjectsByUser, addProjectUserDto.UserId));
            await _cache.DeleteKeyAsync(ProjectsConstants.AllProjects);

            _logger.LogInformation("User {UserId} added to project {ProjectId} successfully", addProjectUserDto.UserId, addProjectUserDto.ProjectId);
            return true;
        }

        public async Task<bool> AssignTeamToProjectAsync(ManageProjectTeamDto manageProjectTeamDto)
        {
            _logger.LogInformation("Assigning team {TeamId} to project {ProjectId}", manageProjectTeamDto.TeamId, manageProjectTeamDto.ProjectId);

            var project = await _unitOfWork.Repository<Project>()
                .GetById(p => p.Id == manageProjectTeamDto.ProjectId)
                .Include(p => p.ProjectUsers)
                .FirstOrDefaultAsync();

            if (project == null)
            {
                _logger.LogWarning("Project with ID: {ProjectId} not found", manageProjectTeamDto.ProjectId);
                return false;
            }

            var team = await _unitOfWork.Repository<Team>()
                .GetById(t => t.Id == manageProjectTeamDto.TeamId)
                .Include(t => t.TeamMembers)
                .ThenInclude(tm => tm.User)
                .FirstOrDefaultAsync();

            if (team == null)
            {
                _logger.LogWarning("Team with ID: {TeamId} not found", manageProjectTeamDto.TeamId);
                return false;
            }

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

            await _cache.DeleteKeyAsync(string.Format(ProjectsConstants.ProjectTeams, project.Id));
            await _cache.DeleteKeyAsync(string.Format(ProjectsConstants.ProjectUsers, project.Id));
            await _cache.DeleteKeyAsync(ProjectsConstants.AllProjects);

            _logger.LogInformation("Team {TeamId} assigned to project {ProjectId} successfully", manageProjectTeamDto.TeamId, manageProjectTeamDto.ProjectId);
            return true;
        }
        #endregion

        #region Update
        public async Task<bool> UpdateProjectAsync(int id, UpdateProjectDto updatedProject, string userId)
        {
            _logger.LogInformation("Updating project {ProjectId} by user {UserId}", id, userId);

            var project = await _unitOfWork.Repository<Project>().GetById(p => p.Id == id).FirstOrDefaultAsync();
            if (project == null)
            {
                _logger.LogWarning("Project with ID: {ProjectId} not found", id);
                return false;
            }

            var projectUser = await _unitOfWork.Repository<ProjectUser>()
                .GetByCondition(pu => pu.ProjectId == id && pu.UserId == userId)
                .FirstOrDefaultAsync();

            if (projectUser == null || projectUser.Role != MemberRole.TeamLead)
            {
                _logger.LogWarning("User {UserId} is not authorized to update project {ProjectId}", userId, id);
                return false;
            }

            _mapper.Map(updatedProject, project);

            var validationResult = await _projectValidator.ValidateAsync(project);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Validation failed for Project entity: {Errors}", validationResult.Errors);
                throw new ValidationException(validationResult.Errors);
            }

            _unitOfWork.Repository<Project>().Update(project);
            await _unitOfWork.Repository<Project>().SaveChangesAsync();
            await _searchService.IndexDocumentAsync(_mapper.Map<ProjectDto>(project), "projects");

            await _cache.DeleteKeyAsync(string.Format(ProjectsConstants.ProjectById, project.Id));
            await _cache.DeleteKeyAsync(ProjectsConstants.AllProjects);

            _logger.LogInformation("Project {ProjectId} updated successfully", id);
            return true;
        }

        public async Task<bool> UpdateUserRoleAsync(int projectId, string userId, MemberRole newRole, string currentUserId)
        {
            _logger.LogInformation("Updating user role for user {UserId} in project {ProjectId} by user {CurrentUserId}", userId, projectId, currentUserId);

            var currentUserRole = await _unitOfWork.Repository<ProjectUser>()
                .GetByCondition(pu => pu.ProjectId == projectId && pu.UserId == currentUserId)
                .Select(pu => pu.Role)
                .FirstOrDefaultAsync();

            if (currentUserRole != MemberRole.TeamLead)
            {
                _logger.LogWarning("User {CurrentUserId} is not authorized to update roles in project {ProjectId}", currentUserId, projectId);
                return false;
            }

            var projectUser = await _unitOfWork.Repository<ProjectUser>()
                .GetByCondition(pu => pu.ProjectId == projectId && pu.UserId == userId)
                .FirstOrDefaultAsync();

            if (projectUser == null)
            {
                _logger.LogWarning("User {UserId} not found in project {ProjectId}", userId, projectId);
                return false;
            }

            projectUser.Role = newRole;
            _unitOfWork.Repository<ProjectUser>().Update(projectUser);
            await _unitOfWork.Repository<ProjectUser>().SaveChangesAsync();

            await _cache.DeleteKeyAsync(string.Format(ProjectsConstants.ProjectUsers, projectId));
            await _cache.DeleteKeyAsync(ProjectsConstants.AllProjects);

            _logger.LogInformation("User role for user {UserId} updated to {NewRole} in project {ProjectId} successfully", userId, newRole, projectId);
            return true;
        }
        #endregion

        #region Delete
        public async Task<bool> DeleteProjectAsync(int id, string userId)
        {
            _logger.LogInformation("Deleting project {ProjectId} by user {UserId}", id, userId);

            var project = await _unitOfWork.Repository<Project>().GetById(p => p.Id == id).FirstOrDefaultAsync();
            if (project == null)
            {
                _logger.LogWarning("Project with ID: {ProjectId} not found", id);
                return false;
            }

            var projectUser = await _unitOfWork.Repository<ProjectUser>()
                .GetByCondition(pu => pu.ProjectId == id && pu.UserId == userId)
                .FirstOrDefaultAsync();

            if (projectUser == null || projectUser.Role != MemberRole.TeamLead)
            {
                _logger.LogWarning("User {UserId} is not authorized to delete project {ProjectId}", userId, id);
                return false;
            }

            _unitOfWork.Repository<Project>().Delete(project);
            await _unitOfWork.Repository<Project>().SaveChangesAsync();
            await _searchService.DeleteDocumentAsync(id.ToString(), "projects");

            await _cache.DeleteKeyAsync(string.Format(ProjectsConstants.ProjectById, project.Id));
            await _cache.DeleteKeyAsync(ProjectsConstants.AllProjects);

            _logger.LogInformation("Project {ProjectId} deleted successfully", id);
            return true;
        }

        public async Task<bool> RemoveColumnsFromProjectAsync(ManageProjectColumnsDto removeProjectColumnsDto)
        {
            _logger.LogInformation("Removing columns from project {ProjectId}", removeProjectColumnsDto.ProjectId);

            var project = await _unitOfWork.Repository<Project>()
                                           .GetById(p => p.Id == removeProjectColumnsDto.ProjectId)
                                           .Include(p => p.Columns)
                                           .FirstOrDefaultAsync();
            if (project == null)
            {
                _logger.LogWarning("Project with ID: {ProjectId} not found", removeProjectColumnsDto.ProjectId);
                return false;
            }

            foreach (var columnName in removeProjectColumnsDto.ColumnNames)
            {
                var columnToRemove = project.Columns.FirstOrDefault(c => c.Name == columnName);
                if (columnToRemove != null)
                {
                    project.Columns.Remove(columnToRemove);
                }
            }

            _unitOfWork.Repository<Project>().Update(project);
            await _unitOfWork.Repository<Project>().SaveChangesAsync();

            await _cache.DeleteKeyAsync(string.Format(ProjectsConstants.ProjectById, project.Id));
            await _cache.DeleteKeyAsync(ProjectsConstants.AllProjects);

            _logger.LogInformation("Columns removed from project {ProjectId} successfully", removeProjectColumnsDto.ProjectId);
            return true;
        }

        public async Task<bool> RemoveUserFromProjectAsync(int projectId, string userId, string currentUserId)
        {
            _logger.LogInformation("Removing user {UserId} from project {ProjectId} by user {CurrentUserId}", userId, projectId, currentUserId);

            var currentUserRole = await _unitOfWork.Repository<ProjectUser>()
                .GetByCondition(pu => pu.ProjectId == projectId && pu.UserId == currentUserId)
                .Select(pu => pu.Role)
                .FirstOrDefaultAsync();

            if (currentUserRole != MemberRole.TeamLead)
            {
                _logger.LogWarning("User {CurrentUserId} is not authorized to remove users from project {ProjectId}", currentUserId, projectId);
                return false;
            }

            var projectUser = await _unitOfWork.Repository<ProjectUser>()
                .GetByCondition(pu => pu.ProjectId == projectId && pu.UserId == userId)
                .FirstOrDefaultAsync();

            if (projectUser == null)
            {
                _logger.LogWarning("User {UserId} not found in project {ProjectId}", userId, projectId);
                return false;
            }

            _unitOfWork.Repository<ProjectUser>().Delete(projectUser);
            await _unitOfWork.Repository<ProjectUser>().SaveChangesAsync();

            await _cache.DeleteKeyAsync(string.Format(ProjectsConstants.ProjectUsers, projectId));
            await _cache.DeleteKeyAsync(ProjectsConstants.AllProjects);

            _logger.LogInformation("User {UserId} removed from project {ProjectId} successfully", userId, projectId);
            return true;
        }

        public async Task<bool> RemoveTeamFromProjectAsync(ManageProjectTeamDto manageProjectTeamDto)
        {
            _logger.LogInformation("Removing team {TeamId} from project {ProjectId}", manageProjectTeamDto.TeamId, manageProjectTeamDto.ProjectId);

            var projectTeam = await _unitOfWork.Repository<ProjectTeam>()
                .GetByCondition(pt => pt.ProjectId == manageProjectTeamDto.ProjectId && pt.TeamId == manageProjectTeamDto.TeamId)
                .Include(pt => pt.Team)
                .ThenInclude(t => t.TeamMembers)
                .FirstOrDefaultAsync();

            if (projectTeam == null || projectTeam.Team == null || !projectTeam.Team.TeamMembers.Any())
            {
                _logger.LogWarning("Team {TeamId} not found in project {ProjectId}", manageProjectTeamDto.TeamId, manageProjectTeamDto.ProjectId);
                return false;
            }

            var teamMembers = projectTeam.Team.TeamMembers;

            var project = await _unitOfWork.Repository<Project>()
                .GetById(p => p.Id == manageProjectTeamDto.ProjectId)
                .FirstOrDefaultAsync();

            if (project == null)
            {
                _logger.LogWarning("Project with ID: {ProjectId} not found", manageProjectTeamDto.ProjectId);
                return false;
            }

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

            await _cache.DeleteKeyAsync(string.Format(ProjectsConstants.ProjectTeams, project.Id));
            await _cache.DeleteKeyAsync(ProjectsConstants.AllProjects);

            _logger.LogInformation("Team {TeamId} removed from project {ProjectId} successfully", manageProjectTeamDto.TeamId, manageProjectTeamDto.ProjectId);
            return true;
        }
        #endregion

        #region Analytics
        public async Task<int> GetNumberOfCreatedProjects()
        {
            _logger.LogInformation("Fetching the number of created projects");

            var cacheKey = ProjectsConstants.NumberOfCreatedProjects;
            var cachedNumberOfProjects = await _cache.GetAsync<int?>(cacheKey);
            if (cachedNumberOfProjects.HasValue)
            {
                _logger.LogInformation("Number of created projects found in cache");
                return cachedNumberOfProjects.Value;
            }

            var numberOfProjects = await _unitOfWork.Repository<Project>().GetAll().CountAsync();
            await _cache.SetAsync(cacheKey, numberOfProjects, TimeSpan.FromMinutes(60));

            return numberOfProjects;
        }
        #endregion

        #region #CalendarApi
        /*public async Task<bool> AddProjectCalendar(int projectId)
        {
            try
            {
                var project = await _unitOfWork.Repository<Project>().GetById(x => x.Id == projectId).FirstOrDefaultAsync();

                        if (project == null)
                        {
                            _logger.LogWarning($"Project with ID {projectId} not found");
                            return false;
                        }

                        var calendarAdded = await _googleCalendarService.CreateCalendarAsync(project.Name, project.Description);
                        _logger.LogInformation($"Calendar '{calendarAdded.Summary}' created successfully for project ID {projectId}");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error creating calendar for project ID {projectId}.");
                        throw new ApplicationException($"Error creating calendar for project ID {projectId}.", ex);
                    }
                }*/
        #endregion
    }
}
