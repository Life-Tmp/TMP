using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TMP.Application.DTOs.ProjectDtos;
using TMP.Application.Projects;

namespace TMP.Service.Project
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
            var result = await _projectService.UpdateProjectAsync(id, updatedProject);
            if (!result) return NotFound();

            return NoContent();
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var result = await _projectService.DeleteProjectAsync(id);
            if (!result) return NotFound();

            return NoContent();
        }
    }
}
