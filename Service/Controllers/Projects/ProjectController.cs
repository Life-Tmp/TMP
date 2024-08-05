using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using TMP.Application.DTOs.ProjectDtos;
using TMP.Application.DTOs.ProjectUserDtos;
using TMPApplication.DTOs.ProjectDtos;
using TMPApplication.DTOs.ProjectUserDtos;
using TMPApplication.Interfaces;
using TMPApplication.Interfaces.Projects;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TMPService.Controllers.Projects
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly ISearchService<ProjectDto> _searchService;
        private readonly ILogger<ProjectController> _logger;

        public ProjectController(IProjectService projectService, ISearchService<ProjectDto> searchService, ILogger<ProjectController> logger)
        {
            _projectService = projectService;
            _searchService = searchService;
            _logger = logger;
        }

        #region Read
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectDto>>> GetProjects()
        {
            _logger.LogInformation("Fetching all projects");
            var projects = await _projectService.GetAllProjectsAsync();
            return Ok(projects);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProjectDto>> GetProject(int id)
        {
            _logger.LogInformation("Fetching project with ID: {ProjectId}", id);
            var project = await _projectService.GetProjectByIdAsync(id);
            if (project == null) return NotFound();

            return Ok(project);
        }

        [Authorize]
        [HttpGet("my-projects")]
        public async Task<ActionResult<IEnumerable<ProjectDto>>> GetMyProjects()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            _logger.LogInformation("Fetching projects for user {UserId}", userId);
            var projects = await _projectService.GetProjectsByUserAsync(userId);
            return Ok(projects);
        }

        [Authorize]
        [HttpGet("{id}/role")]
        public async Task<ActionResult<string>> GetUserRoleInProject(int id)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            _logger.LogInformation("Fetching user role for user {UserId} in project {ProjectId}", userId, id);
            var role = await _projectService.GetUserRoleInProjectAsync(id, userId);
            if (role == null) return NotFound();

            return Ok(role);
        }

        [HttpGet("{id}/users")]
        public async Task<ActionResult<ProjectUsersDto>> GetProjectUsers(int id)
        {
            _logger.LogInformation("Fetching users for project {ProjectId}", id);
            var projectUsers = await _projectService.GetProjectUsersAsync(id);
            if (projectUsers == null) return NotFound();

            return Ok(projectUsers);
        }

        [HttpGet("{id}/teams")]
        public async Task<ActionResult<ProjectTeamsDto>> GetProjectTeams(int id)
        {
            _logger.LogInformation("Fetching teams for project {ProjectId}", id);
            var projectTeams = await _projectService.GetProjectTeamsAsync(id);
            if (projectTeams == null) return NotFound();

            return Ok(projectTeams);
        }

        [HttpGet("{id}/tasks")]
        public async Task<ActionResult<ProjectTasksDto>> GetProjectTasks(int id)
        {
            _logger.LogInformation("Fetching tasks for project {ProjectId}", id);
            var projectTasks = await _projectService.GetProjectTasksAsync(id);
            if (projectTasks == null) return NotFound();

            return Ok(projectTasks);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchProjects([FromQuery] string query)
        {
            _logger.LogInformation("Searching projects with query: {Query}", query);
            var projects = await _searchService.SearchDocumentAsync(query, "projects");
            return Ok(projects);
        }

        [HttpGet("analytics/count")]
        public async Task<IActionResult> GetNumberOfProjects()
        {
            _logger.LogInformation("Fetching the number of created projects");
            var numberOfProjects = await _projectService.GetNumberOfCreatedProjects();
            return Ok(numberOfProjects);
        }
        #endregion

        #region Create
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<ProjectDto>> AddProject(AddProjectDto newProject)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            _logger.LogInformation("Adding new project by user {UserId}", userId);
            var project = await _projectService.AddProjectAsync(newProject, userId);
            return CreatedAtAction(nameof(GetProject), new { id = project.Id }, project);
        }

        [Authorize]
        [HttpPost("add-columns")]
        public async Task<IActionResult> AddColumnsToProject([FromBody] ManageProjectColumnsDto addProjectColumnDto)
        {
            _logger.LogInformation("Adding columns to project {ProjectId}", addProjectColumnDto.ProjectId);
            var result = await _projectService.AddColumnsToProjectAsync(addProjectColumnDto);
            if (!result) return NotFound();

            return Ok("Columns added successfully.");
        }

        [Authorize]
        [HttpPost("add-user")]
        public async Task<IActionResult> AddUserToProject(AddProjectUserDto addProjectUserDto)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            _logger.LogInformation("Adding user {UserId} to project {ProjectId} by user {CurrentUserId}", addProjectUserDto.UserId, addProjectUserDto.ProjectId, userId);
            var result = await _projectService.AddUserToProjectAsync(addProjectUserDto, userId);
            if (!result) return Forbid();

            return Ok();
        }

        [Authorize]
        [HttpPost("assign-team")]
        public async Task<IActionResult> AssignTeamToProject(ManageProjectTeamDto manageProjectTeamDto)
        {
            _logger.LogInformation("Assigning team {TeamId} to project {ProjectId}", manageProjectTeamDto.TeamId, manageProjectTeamDto.ProjectId);
            var result = await _projectService.AssignTeamToProjectAsync(manageProjectTeamDto);
            if (!result) return BadRequest();

            return Ok("Team assigned to project successfully.");
        }
        #endregion

        #region Update
        [Authorize]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateProject(int id, UpdateProjectDto updatedProject)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            _logger.LogInformation("Updating project {ProjectId} by user {UserId}", id, userId);
            var result = await _projectService.UpdateProjectAsync(id, updatedProject, userId);
            if (!result) return Forbid();

            return NoContent();
        }

        [Authorize]
        [HttpPatch("{id:int}/update-user-role")]
        public async Task<IActionResult> UpdateUserRole(int id, UpdateProjectUserRoleDto updateUserRoleDto)
        {
            var currentUserId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId == null) return Unauthorized();

            _logger.LogInformation("Updating user role for user {UserId} in project {ProjectId} by user {CurrentUserId}", updateUserRoleDto.UserId, id, currentUserId);
            var result = await _projectService.UpdateUserRoleAsync(id, updateUserRoleDto.UserId, updateUserRoleDto.NewRole, currentUserId);
            if (!result) return Forbid();

            return Ok();
        }
        #endregion

        #region Delete
        [Authorize]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            _logger.LogInformation("Deleting project {ProjectId} by user {UserId}", id, userId);
            var result = await _projectService.DeleteProjectAsync(id, userId);
            if (!result) return Forbid();

            return NoContent();
        }

        [Authorize]
        [HttpDelete("remove-columns")]
        public async Task<IActionResult> RemoveColumnsFromProject([FromBody] ManageProjectColumnsDto removeProjectColumnsDto)
        {
            _logger.LogInformation("Removing columns from project {ProjectId}", removeProjectColumnsDto.ProjectId);
            var result = await _projectService.RemoveColumnsFromProjectAsync(removeProjectColumnsDto);
            if (!result) return NotFound();

            return Ok("Columns removed successfully.");
        }

        [Authorize]
        [HttpDelete("{id:int}/remove-user")]
        public async Task<IActionResult> RemoveUserFromProject(int id, RemoveProjectUserDto removeProjectUserDto)
        {
            var currentUserId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId == null) return Unauthorized();

            _logger.LogInformation("Removing user {UserId} from project {ProjectId} by user {CurrentUserId}", removeProjectUserDto.UserId, id, currentUserId);
            var result = await _projectService.RemoveUserFromProjectAsync(id, removeProjectUserDto.UserId, currentUserId);
            if (!result) return Forbid();

            return Ok();
        }

        [Authorize]
        [HttpDelete("remove-team")]
        public async Task<IActionResult> RemoveTeamFromProject(ManageProjectTeamDto manageProjectTeamDto)
        {
            _logger.LogInformation("Removing team {TeamId} from project {ProjectId}", manageProjectTeamDto.TeamId, manageProjectTeamDto.ProjectId);
            var result = await _projectService.RemoveTeamFromProjectAsync(manageProjectTeamDto);
            if (!result) return BadRequest();

            return Ok("Team removed from project successfully.");
        }

        [HttpPost("calendar/add")]
        [Authorize]
        public async Task<IActionResult> AddCalendarToProject(int projectId)
        {
            var isCalendarAdded = await _projectService.AddProjectCalendar(projectId);
            if (!isCalendarAdded) return BadRequest();

            return Ok("Calendar added successfully");
        }
        #endregion
    }
}
