using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TMP.Application.DTOs.TeamDtos;
using TMP.Application.Interfaces;

namespace TMP.Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeamController : ControllerBase
    {
        private readonly ITeamService _teamService;

        public TeamController(ITeamService teamService)
        {
            _teamService = teamService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TeamDto>>> GetTeams()
        {
            var teams = await _teamService.GetAllTeamsAsync();
            return Ok(teams);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TeamDto>> GetTeam(int id)
        {
            var team = await _teamService.GetTeamByIdAsync(id);
            if (team == null)
                return NotFound();

            return Ok(team);
        }

        [Authorize]
        [HttpGet("my-teams")]
        public async Task<ActionResult<IEnumerable<TeamDto>>> GetUserTeams()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var teams = await _teamService.GetUserTeamsAsync(userId);
            return Ok(teams);
        }

        [HttpGet("{id}/members")]
        public async Task<ActionResult<IEnumerable<TeamMemberDto>>> GetTeamMembers(int id)
        {
            var members = await _teamService.GetTeamMembersAsync(id);
            if (members == null) return NotFound();

            return Ok(members);
        }

        [HttpGet("{id}/projects")]
        public async Task<ActionResult<TeamProjectsDto>> GetTeamProjects(int id)
        {
            var teamProjects = await _teamService.GetTeamProjectsAsync(id);
            if (teamProjects == null) return NotFound();

            return Ok(teamProjects);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<TeamDto>> AddTeam([FromBody] AddTeamDto newTeam)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var team = await _teamService.AddTeamAsync(newTeam, userId);
            return CreatedAtAction(nameof(GetTeam), new { id = team.Id }, team);
        }

        [Authorize]
        [HttpPost("add-member")]
        public async Task<IActionResult> AddUserToTeam(AddTeamMemberDto addTeamMemberDto)
        {
            var result = await _teamService.AddUserToTeamAsync(addTeamMemberDto);
            if (!result) return BadRequest("User could not be added to the team.");
            return Ok("User added to the team successfully.");
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTeam(int id, AddTeamDto updatedTeam)
        {
            var result = await _teamService.UpdateTeamAsync(id, updatedTeam);
            if (!result)
                return NotFound();

            return NoContent();
        }

        [Authorize]
        [HttpPatch("{id}/update-member-role")]
        public async Task<IActionResult> UpdateUserRoleInTeam(int id, UpdateTeamMemberRoleDto updateUserRoleDto)
        {
            var result = await _teamService.UpdateUserRoleInTeamAsync(id, updateUserRoleDto.UserId, updateUserRoleDto.NewRole);
            if (!result) return BadRequest("Role could not be updated.");
            return Ok("Role updated successfully.");
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTeam(int id)
        {
            var result = await _teamService.DeleteTeamAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }

        [Authorize]
        [HttpDelete("remove-user")]
        public async Task<IActionResult> RemoveUserFromTeam(RemoveTeamMemberDto removeUserFromTeamDto)
        {
            var result = await _teamService.RemoveUserFromTeamAsync(removeUserFromTeamDto);
            if (!result)
                return BadRequest("Could not remove user from team.");

            return Ok("User removed from team successfully.");
        }
    }
}
