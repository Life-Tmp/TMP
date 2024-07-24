using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TMP.Application.DTOs.ProjectDtos;
using TMP.Application.DTOs.ProjectUserDtos;
using TMPApplication.DTOs.ProjectUserDtos;
using TMPApplication.Interfaces.Projects;

namespace TMPService.Controllers.Projects
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ProjectController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectDto>>> GetProjects()
        {
            var projects = await _projectService.GetAllProjectsAsync();
            return Ok(projects);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectDto>> GetProject(int id)
        {
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

            var projects = await _projectService.GetProjectsByUserAsync(userId);
            return Ok(projects);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<ProjectDto>> AddProject(AddProjectDto newProject)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var project = await _projectService.AddProjectAsync(newProject, userId);
            return CreatedAtAction(nameof(GetProject), new { id = project.Id }, project);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(int id, AddProjectDto updatedProject)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var result = await _projectService.UpdateProjectAsync(id, updatedProject, userId);
            if (!result) return Forbid();

            return NoContent();
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var result = await _projectService.DeleteProjectAsync(id, userId);
            if (!result) return Forbid();

            return NoContent();
        }

        [Authorize]
        [HttpPost("add-user")]
        public async Task<IActionResult> AddUserToProject(AddProjectUserDto addProjectUserDto)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var result = await _projectService.AddUserToProjectAsync(addProjectUserDto, userId);
            if (!result) return Forbid();

            return Ok();
        }

        [Authorize]
        [HttpPatch("{id}/update-user-role")]
        public async Task<IActionResult> UpdateUserRole(int id, UpdateProjectUserRoleDto updateUserRoleDto)
        {
            var currentUserId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId == null) return Unauthorized();

            var result = await _projectService.UpdateUserRoleAsync(id, updateUserRoleDto.UserId, updateUserRoleDto.NewRole, currentUserId);
            if (!result) return Forbid();

            return Ok();
        }

        [Authorize]
        [HttpPost("{id}/remove-user")]
        public async Task<IActionResult> RemoveUserFromProject(int id, RemoveProjectUserDto removeProjectUserDto)
        {
            var currentUserId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId == null) return Unauthorized();

            var result = await _projectService.RemoveUserFromProjectAsync(id, removeProjectUserDto.UserId, currentUserId);
            if (!result) return Forbid();

            return Ok();
        }

        [HttpGet("{id}/users")]
        public async Task<ActionResult<ProjectUsersDto>> GetProjectUsers(int id)
        {
            var projectUsers = await _projectService.GetProjectUsersAsync(id);
            if (projectUsers == null) return NotFound();

            return Ok(projectUsers);
        }

        [HttpGet("{id}/tasks")]
        public async Task<ActionResult<ProjectTasksDto>> GetProjectTasks(int id)
        {
            var projectTasks = await _projectService.GetProjectTasksAsync(id);
            if (projectTasks == null) return NotFound();

            return Ok(projectTasks);
        }

        [Authorize]
        [HttpGet("{id}/role")]
        public async Task<ActionResult<string>> GetUserRoleInProject(int id)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var role = await _projectService.GetUserRoleInProjectAsync(id, userId);
            if (role == null) return NotFound();

            return Ok(role);
        }
    }
}
