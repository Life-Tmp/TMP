using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using TMP.Application.DTOs.TeamDtos;
using TMP.Application.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TMP.Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeamController : ControllerBase
    {
        private readonly ITeamService _teamService;
        private readonly ILogger<TeamController> _logger;

        public TeamController(ITeamService teamService, ILogger<TeamController> logger)
        {
            _teamService = teamService;
            _logger = logger;
        }

        #region Read

        /// <summary>
        /// Retrieves a list of all teams.
        /// </summary>
        /// <returns>200 OK with a list of teams.</returns>
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TeamDto>>> GetTeams()
        {
            _logger.LogInformation("Fetching all teams");
            var teams = await _teamService.GetAllTeamsAsync();
            return Ok(teams);
        }

        /// <summary>
        /// Retrieves a team by its ID.
        /// </summary>
        /// <param name="id">The ID of the team to retrieve.</param>
        /// <returns>200 OK with the team details; 404 Not Found if the team does not exist.</returns>
        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<TeamDto>> GetTeam(int id)
        {
            _logger.LogInformation("Fetching team with ID: {TeamId}", id);
            var team = await _teamService.GetTeamByIdAsync(id);
            if (team == null)
            {
                _logger.LogWarning("Team with ID: {TeamId} not found", id);
                return NotFound();
            }

            return Ok(team);
        }

        /// <summary>
        /// Retrieves the teams assigned to the currently logged-in user.
        /// </summary>
        /// <returns>200 OK with a list of teams; 401 Unauthorized if the user is not authenticated.</returns>
        [Authorize]
        [HttpGet("my-teams")]
        public async Task<ActionResult<IEnumerable<TeamDto>>> GetUserTeams()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                _logger.LogWarning("Unauthorized access to my-teams");
                return Unauthorized();
            }

            _logger.LogInformation("Fetching teams for user with ID: {UserId}", userId);
            var teams = await _teamService.GetUserTeamsAsync(userId);
            return Ok(teams);
        }

        /// <summary>
        /// Retrieves the members of a specific team.
        /// </summary>
        /// <param name="id">The ID of the team.</param>
        /// <returns>200 OK with a list of team members; 404 Not Found if the team does not exist.</returns>
        [Authorize]
        [HttpGet("{id:int}/members")]
        public async Task<ActionResult<IEnumerable<TeamMemberDto>>> GetTeamMembers(int id)
        {
            _logger.LogInformation("Fetching team members for team with ID: {TeamId}", id);
            var members = await _teamService.GetTeamMembersAsync(id);
            if (members == null)
            {
                _logger.LogWarning("Team members for team with ID: {TeamId} not found", id);
                return NotFound();
            }

            return Ok(members);
        }

        /// <summary>
        /// Retrieves the projects assigned to a specific team.
        /// </summary>
        /// <param name="id">The ID of the team.</param>
        /// <returns>200 OK with a list of team projects; 404 Not Found if the team does not exist.</returns>
        [Authorize]
        [HttpGet("{id:int}/projects")]
        public async Task<ActionResult<TeamProjectsDto>> GetTeamProjects(int id)
        {
            _logger.LogInformation("Fetching projects for team with ID: {TeamId}", id);
            var teamProjects = await _teamService.GetTeamProjectsAsync(id);
            if (teamProjects == null)
            {
                _logger.LogWarning("Projects for team with ID: {TeamId} not found", id);
                return NotFound();
            }

            return Ok(teamProjects);
        }

        #endregion

        #region Create

        /// <summary>
        /// Adds a new team.
        /// </summary>
        /// <param name="newTeam">The details of the team to add.</param>
        /// <returns>The created team.</returns>
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<TeamDto>> AddTeam([FromBody] AddTeamDto newTeam)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                _logger.LogWarning("Unauthorized access to add team");
                return Unauthorized();
            }

            _logger.LogInformation("Adding new team");
            var team = await _teamService.AddTeamAsync(newTeam, userId);
            return CreatedAtAction(nameof(GetTeam), new { id = team.Id }, team);
        }

        /// <summary>
        /// Adds a user to a team.
        /// </summary>
        /// <param name="addTeamMemberDto">The details of the user to add to the team.</param>
        /// <returns>200 OK if the user is added successfully; 400 Bad Request if the user cannot be added.</returns>
        [Authorize]
        [HttpPost("add-member")]
        public async Task<IActionResult> AddUserToTeam([FromBody] AddTeamMemberDto addTeamMemberDto)
        {
            _logger.LogInformation("Adding user with ID: {UserId} to team with ID: {TeamId}", addTeamMemberDto.UserId, addTeamMemberDto.TeamId);
            var result = await _teamService.AddUserToTeamAsync(addTeamMemberDto);
            if (!result)
            {
                _logger.LogWarning("Failed to add user with ID: {UserId} to team with ID: {TeamId}", addTeamMemberDto.UserId, addTeamMemberDto.TeamId);
                return BadRequest("User could not be added to the team.");
            }
            return Ok("User added to the team successfully.");
        }

        #endregion

        #region Update

        /// <summary>
        /// Updates an existing team.
        /// </summary>
        /// <param name="id">The ID of the team to update.</param>
        /// <param name="updatedTeam">The updated team details.</param>
        /// <returns>No content if successful, otherwise a not found response.</returns>
        [Authorize]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateTeam(int id, [FromBody] AddTeamDto updatedTeam)
        {
            _logger.LogInformation("Updating team with ID: {TeamId}", id);
            var result = await _teamService.UpdateTeamAsync(id, updatedTeam);
            if (!result)
            {
                _logger.LogWarning("Team with ID: {TeamId} not found", id);
                return NotFound();
            }

            return NoContent();
        }

        /// <summary>
        /// Updates the role of a user in a team.
        /// </summary>
        /// <param name="id">The ID of the team.</param>
        /// <param name="updateUserRoleDto">The new role details.</param>
        /// <returns>200 OK if the role is updated successfully; 400 Bad Request if the role cannot be updated.</returns>
        [Authorize]
        [HttpPatch("{id:int}/update-member-role")]
        public async Task<IActionResult> UpdateUserRoleInTeam(int id, [FromBody] UpdateTeamMemberRoleDto updateUserRoleDto)
        {
            _logger.LogInformation("Updating role of user with ID: {UserId} in team with ID: {TeamId}", updateUserRoleDto.UserId, id);
            var result = await _teamService.UpdateUserRoleInTeamAsync(id, updateUserRoleDto.UserId, updateUserRoleDto.NewRole);
            if (!result)
            {
                _logger.LogWarning("Failed to update role of user with ID: {UserId} in team with ID: {TeamId}", updateUserRoleDto.UserId, id);
                return BadRequest("Role could not be updated.");
            }
            return Ok("Role updated successfully.");
        }

        #endregion

        #region Delete

        /// <summary>
        /// Deletes a team by its ID.
        /// </summary>
        /// <param name="id">The ID of the team to delete.</param>
        /// <returns>204 No Content if the deletion is successful; 404 Not Found if the team does not exist.</returns>
        [Authorize]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteTeam(int id)
        {
            _logger.LogInformation("Deleting team with ID: {TeamId}", id);
            var result = await _teamService.DeleteTeamAsync(id);
            if (!result)
            {
                _logger.LogWarning("Team with ID: {TeamId} not found", id);
                return NotFound();
            }

            return NoContent();
        }

        /// <summary>
        /// Removes a user from a team.
        /// </summary>
        /// <param name="removeUserFromTeamDto">The details of the user to remove from the team.</param>
        /// <returns>200 OK if the user is removed successfully; 400 Bad Request if the user cannot be removed.</returns>
        [Authorize]
        [HttpDelete("remove-user")]
        public async Task<IActionResult> RemoveUserFromTeam([FromBody] RemoveTeamMemberDto removeUserFromTeamDto)
        {
            _logger.LogInformation("Removing user with ID: {UserId} from team with ID: {TeamId}", removeUserFromTeamDto.UserId, removeUserFromTeamDto.TeamId);
            var result = await _teamService.RemoveUserFromTeamAsync(removeUserFromTeamDto);
            if (!result)
            {
                _logger.LogWarning("Failed to remove user with ID: {UserId} from team with ID: {TeamId}", removeUserFromTeamDto.UserId, removeUserFromTeamDto.TeamId);
                return BadRequest("Could not remove user from team.");
            }

            return Ok("User removed from team successfully.");
        }

        #endregion
    }
}
