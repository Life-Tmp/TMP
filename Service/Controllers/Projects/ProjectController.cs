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
        /// <summary>
        /// Retrieves a list of all projects.
        /// </summary>
        /// <returns>200 OK with a list of projects.</returns>
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectDto>>> GetProjects()
        {
            _logger.LogInformation("Fetching all projects");
            var projects = await _projectService.GetAllProjectsAsync();
            return Ok(projects);
        }

        /// <summary>
        /// Retrieves a project by its ID.
        /// </summary>
        /// <param name="id">The ID of the project to retrieve.</param>
        /// <returns>200 OK with the project details; 404 Not Found if the project does not exist.</returns>
        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProjectDto>> GetProject(int id)
        {
            _logger.LogInformation("Fetching project with ID: {ProjectId}", id);
            var project = await _projectService.GetProjectByIdAsync(id);
            if (project == null)
            {
                _logger.LogWarning("Project with ID: {ProjectId} not found", id);
                return NotFound();
            }

            return Ok(project);
        }

        /// <summary>
        /// Retrieves the projects assigned to the currently logged-in user.
        /// </summary>
        /// <returns>200 OK with a list of projects assigned to the user; 401 Unauthorized if the user is not authenticated.</returns>
        [Authorize]
        [HttpGet("my-projects")]
        public async Task<ActionResult<IEnumerable<ProjectDto>>> GetMyProjects()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                _logger.LogWarning("Unauthorized access to my-projects");
                return Unauthorized();
            }

            _logger.LogInformation("Fetching projects for user with ID: {UserId}", userId);
            var projects = await _projectService.GetProjectsByUserAsync(userId);
            return Ok(projects);
        }

        /// <summary>
        /// Retrieves the user's role in a specific project.
        /// </summary>
        /// <param name="id">The ID of the project.</param>
        /// <returns>200 OK with the user's role; 404 Not Found if the user does not have a role in the project.</returns>
        [Authorize]
        [HttpGet("{id}/role")]
        public async Task<ActionResult<string>> GetUserRoleInProject(int id)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                _logger.LogWarning("Unauthorized access to project role");
                return Unauthorized();
            }

            _logger.LogInformation("Fetching user role for user with ID: {UserId} in project with ID: {ProjectId}", userId, id);
            var role = await _projectService.GetUserRoleInProjectAsync(id, userId);
            if (role == null)
            {
                _logger.LogWarning("Role not found for user with ID: {UserId} in project with ID: {ProjectId}", userId, id);
                return NotFound();
            }

            return Ok(role);
        }

        /// <summary>
        /// Retrieves the users assigned to a specific project.
        /// </summary>
        /// <param name="id">The ID of the project.</param>
        /// <returns>200 OK with a list of assigned users; 404 Not Found if no users are assigned.</returns>
        [Authorize]
        [HttpGet("{id}/users")]
        public async Task<ActionResult<ProjectUsersDto>> GetProjectUsers(int id)
        {
            _logger.LogInformation("Fetching users for project with ID: {ProjectId}", id);
            var projectUsers = await _projectService.GetProjectUsersAsync(id);
            if (projectUsers == null)
            {
                _logger.LogWarning("Users not found for project with ID: {ProjectId}", id);
                return NotFound();
            }

            return Ok(projectUsers);
        }

        /// <summary>
        /// Retrieves the teams assigned to a specific project.
        /// </summary>
        /// <param name="id">The ID of the project.</param>
        /// <returns>200 OK with a list of assigned teams; 404 Not Found if no teams are assigned.</returns>
        [Authorize]
        [HttpGet("{id}/teams")]
        public async Task<ActionResult<ProjectTeamsDto>> GetProjectTeams(int id)
        {
            _logger.LogInformation("Fetching teams for project with ID: {ProjectId}", id);
            var projectTeams = await _projectService.GetProjectTeamsAsync(id);
            if (projectTeams == null)
            {
                _logger.LogWarning("Teams not found for project with ID: {ProjectId}", id);
                return NotFound();
            }

            return Ok(projectTeams);
        }

        /// <summary>
        /// Retrieves the tasks assigned to a specific project.
        /// </summary>
        /// <param name="id">The ID of the project.</param>
        /// <returns>200 OK with a list of assigned tasks; 404 Not Found if no tasks are assigned.</returns>
        [Authorize]
        [HttpGet("{id}/tasks")]
        public async Task<ActionResult<ProjectTasksDto>> GetProjectTasks(int id)
        {
            _logger.LogInformation("Fetching tasks for project with ID: {ProjectId}", id);
            var projectTasks = await _projectService.GetProjectTasksAsync(id);
            if (projectTasks == null)
            {
                _logger.LogWarning("Tasks not found for project with ID: {ProjectId}", id);
                return NotFound();
            }

            return Ok(projectTasks);
        }

        /// <summary>
        /// Searches for projects based on a search term.
        /// </summary>
        /// <param name="query">The term to search for.</param>
        /// <returns>200 OK with a list of projects matching the search term.</returns>
        [Authorize]
        [HttpGet("search")]
        public async Task<IActionResult> SearchProjects([FromQuery] string query)
        {
            _logger.LogInformation("Searching projects with query: {Query}", query);
            var projects = await _searchService.SearchDocumentAsync(query, "projects");
            return Ok(projects);
        }

        /// <summary>
        /// Retrieves the total number of created projects.
        /// </summary>
        /// <returns>200 OK with the total number of projects.</returns>
        [Authorize]
        [HttpGet("analytics/count")]
        public async Task<IActionResult> GetNumberOfProjects()
        {
            _logger.LogInformation("Fetching the number of created projects");
            var numberOfProjects = await _projectService.GetNumberOfCreatedProjects();
            return Ok(numberOfProjects);
        }
        #endregion

        #region Create
        /// <summary>
        /// Adds a new project.
        /// </summary>
        /// <param name="newProject">The details of the project to add.</param>
        /// <returns>The created project.</returns>
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<ProjectDto>> AddProject([FromBody] AddProjectDto newProject)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                _logger.LogWarning("Unauthorized access to add project");
                return Unauthorized();
            }

            _logger.LogInformation("Adding new project by user with ID: {UserId}", userId);
            var project = await _projectService.AddProjectAsync(newProject, userId);
            return CreatedAtAction(nameof(GetProject), new { id = project.Id }, project);
        }

        /// <summary>
        /// Adds columns to a project.
        /// </summary>
        /// <param name="addProjectColumnDto">The details of the columns to add.</param>
        /// <returns>200 OK if the columns are added successfully; 404 Not Found if the project does not exist.</returns>
        [Authorize]
        [HttpPost("add-columns")]
        public async Task<IActionResult> AddColumnsToProject([FromBody] ManageProjectColumnsDto addProjectColumnDto)
        {
            _logger.LogInformation("Adding columns to project with ID: {ProjectId}", addProjectColumnDto.ProjectId);
            var result = await _projectService.AddColumnsToProjectAsync(addProjectColumnDto);
            if (!result)
            {
                _logger.LogWarning("Failed to add columns to project with ID: {ProjectId}", addProjectColumnDto.ProjectId);
                return NotFound();
            }

            return Ok("Columns added successfully.");
        }

        /// <summary>
        /// Adds a user to a project.
        /// </summary>
        /// <param name="addProjectUserDto">The details of the user to add.</param>
        /// <returns>200 OK if the user is added successfully; 403 Forbidden if the user cannot be added.</returns>
        [Authorize]
        [HttpPost("add-user")]
        public async Task<IActionResult> AddUserToProject([FromBody] AddProjectUserDto addProjectUserDto)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                _logger.LogWarning("Unauthorized access to add user to project");
                return Unauthorized();
            }

            _logger.LogInformation("Adding user with ID: {UserId} to project with ID: {ProjectId} by user with ID: {CurrentUserId}", addProjectUserDto.UserId, addProjectUserDto.ProjectId, userId);
            var result = await _projectService.AddUserToProjectAsync(addProjectUserDto, userId);
            if (!result)
            {
                _logger.LogWarning("Failed to add user with ID: {UserId} to project with ID: {ProjectId}", addProjectUserDto.UserId, addProjectUserDto.ProjectId);
                return Forbid();
            }

            return Ok();
        }

        /// <summary>
        /// Assigns a team to a project.
        /// </summary>
        /// <param name="manageProjectTeamDto">The details of the team to assign.</param>
        /// <returns>200 OK if the team is assigned successfully; 400 Bad Request if the team cannot be assigned.</returns>
        [Authorize]
        [HttpPost("assign-team")]
        public async Task<IActionResult> AssignTeamToProject([FromBody] ManageProjectTeamDto manageProjectTeamDto)
        {
            _logger.LogInformation("Assigning team with ID: {TeamId} to project with ID: {ProjectId}", manageProjectTeamDto.TeamId, manageProjectTeamDto.ProjectId);
            var result = await _projectService.AssignTeamToProjectAsync(manageProjectTeamDto);
            if (!result)
            {
                _logger.LogWarning("Failed to assign team with ID: {TeamId} to project with ID: {ProjectId}", manageProjectTeamDto.TeamId, manageProjectTeamDto.ProjectId);
                return BadRequest();
            }

            return Ok("Team assigned to project successfully.");
        }

/*        /// <summary>
        /// Adds a calendar to a project.
        /// </summary>
        /// <param name="projectId">The ID of the project.</param>
        /// <returns>200 OK if the calendar is added successfully; 400 Bad Request if the calendar cannot be added.</returns>
        [Authorize]
        [HttpPost("calendar/add")]
        public async Task<IActionResult> AddCalendarToProject(int projectId)
        {
            _logger.LogInformation("Adding calendar to project with ID: {ProjectId}", projectId);
            var isCalendarAdded = await _projectService.AddProjectCalendar(projectId);
            if (!isCalendarAdded)
            {
                _logger.LogWarning("Failed to add calendar to project with ID: {ProjectId}", projectId);
                return BadRequest();
            }

            return Ok("Calendar added successfully.");
        }*/
        #endregion

        #region Update
        /// <summary>
        /// Updates a project.
        /// </summary>
        /// <param name="id">The ID of the project to update.</param>
        /// <param name="updatedProject">The updated project details.</param>
        /// <returns>No content if successful, otherwise a forbidden response.</returns>
        [Authorize]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateProject(int id, [FromBody] UpdateProjectDto updatedProject)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                _logger.LogWarning("Unauthorized access to update project");
                return Unauthorized();
            }

            _logger.LogInformation("Updating project with ID: {ProjectId} by user with ID: {UserId}", id, userId);
            var result = await _projectService.UpdateProjectAsync(id, updatedProject, userId);
            if (!result)
            {
                _logger.LogWarning("Failed to update project with ID: {ProjectId}", id);
                return Forbid();
            }

            return NoContent();
        }

        /// <summary>
        /// Updates the role of a user in a project.
        /// </summary>
        /// <param name="id">The ID of the project.</param>
        /// <param name="updateUserRoleDto">The new role details.</param>
        /// <returns>200 OK if the role is updated successfully; 403 Forbidden if the role cannot be updated.</returns>
        [Authorize]
        [HttpPatch("{id:int}/update-user-role")]
        public async Task<IActionResult> UpdateUserRole(int id, [FromBody] UpdateProjectUserRoleDto updateUserRoleDto)
        {
            var currentUserId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId == null)
            {
                _logger.LogWarning("Unauthorized access to update user role in project");
                return Unauthorized();
            }

            _logger.LogInformation("Updating user role for user with ID: {UserId} in project with ID: {ProjectId} by user with ID: {CurrentUserId}", updateUserRoleDto.UserId, id, currentUserId);
            var result = await _projectService.UpdateUserRoleAsync(id, updateUserRoleDto.UserId, updateUserRoleDto.NewRole, currentUserId);
            if (!result)
            {
                _logger.LogWarning("Failed to update user role for user with ID: {UserId} in project with ID: {ProjectId}", updateUserRoleDto.UserId, id);
                return Forbid();
            }

            return Ok();
        }
        #endregion

        #region Delete
        /// <summary>
        /// Deletes a project by its ID.
        /// </summary>
        /// <param name="id">The ID of the project to delete.</param>
        /// <returns>204 No Content if the deletion is successful; 403 Forbidden if the project cannot be deleted.</returns>
        [Authorize]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                _logger.LogWarning("Unauthorized access to delete project");
                return Unauthorized();
            }

            _logger.LogInformation("Deleting project with ID: {ProjectId} by user with ID: {UserId}", id, userId);
            var result = await _projectService.DeleteProjectAsync(id, userId);
            if (!result)
            {
                _logger.LogWarning("Failed to delete project with ID: {ProjectId}", id);
                return Forbid();
            }

            return NoContent();
        }

        /// <summary>
        /// Removes columns from a project.
        /// </summary>
        /// <param name="removeProjectColumnsDto">The details of the columns to remove.</param>
        /// <returns>200 OK if the columns are removed successfully; 404 Not Found if the project does not exist.</returns>
        [Authorize]
        [HttpDelete("remove-columns")]
        public async Task<IActionResult> RemoveColumnsFromProject([FromBody] ManageProjectColumnsDto removeProjectColumnsDto)
        {
            _logger.LogInformation("Removing columns from project with ID: {ProjectId}", removeProjectColumnsDto.ProjectId);
            var result = await _projectService.RemoveColumnsFromProjectAsync(removeProjectColumnsDto);
            if (!result)
            {
                _logger.LogWarning("Failed to remove columns from project with ID: {ProjectId}", removeProjectColumnsDto.ProjectId);
                return NotFound();
            }

            return Ok("Columns removed successfully.");
        }

        /// <summary>
        /// Removes a user from a project.
        /// </summary>
        /// <param name="id">The ID of the project.</param>
        /// <param name="removeProjectUserDto">The details of the user to remove.</param>
        /// <returns>200 OK if the user is removed successfully; 403 Forbidden if the user cannot be removed.</returns>
        [Authorize]
        [HttpDelete("{id:int}/remove-user")]
        public async Task<IActionResult> RemoveUserFromProject(int id, [FromBody] RemoveProjectUserDto removeProjectUserDto)
        {
            var currentUserId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId == null)
            {
                _logger.LogWarning("Unauthorized access to remove user from project");
                return Unauthorized();
            }

            _logger.LogInformation("Removing user with ID: {UserId} from project with ID: {ProjectId} by user with ID: {CurrentUserId}", removeProjectUserDto.UserId, id, currentUserId);
            var result = await _projectService.RemoveUserFromProjectAsync(id, removeProjectUserDto.UserId, currentUserId);
            if (!result)
            {
                _logger.LogWarning("Failed to remove user with ID: {UserId} from project with ID: {ProjectId}", removeProjectUserDto.UserId, id);
                return Forbid();
            }

            return Ok();
        }

        /// <summary>
        /// Removes a team from a project.
        /// </summary>
        /// <param name="manageProjectTeamDto">The details of the team to remove.</param>
        /// <returns>200 OK if the team is removed successfully; 400 Bad Request if the team cannot be removed.</returns>
        [Authorize]
        [HttpDelete("remove-team")]
        public async Task<IActionResult> RemoveTeamFromProject([FromBody] ManageProjectTeamDto manageProjectTeamDto)
        {
            _logger.LogInformation("Removing team with ID: {TeamId} from project with ID: {ProjectId}", manageProjectTeamDto.TeamId, manageProjectTeamDto.ProjectId);
            var result = await _projectService.RemoveTeamFromProjectAsync(manageProjectTeamDto);
            if (!result)
            {
                _logger.LogWarning("Failed to remove team with ID: {TeamId} from project with ID: {ProjectId}", manageProjectTeamDto.TeamId, manageProjectTeamDto.ProjectId);
                return BadRequest();
            }

            return Ok("Team removed from project successfully.");
        }
        #endregion
    }
}
