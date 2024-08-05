using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TMPApplication.Interfaces.Invitations;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace TMPService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvitationController : ControllerBase
    {
        private readonly IInvitationsService _invitationsService;
        private readonly ILogger<InvitationController> _logger;

        public InvitationController(IInvitationsService invitationsService, ILogger<InvitationController> logger)
        {
            _invitationsService = invitationsService;
            _logger = logger;
        }

        #region Read

        /// <summary>
        /// Accepts an invitation using a token.
        /// </summary>
        /// <param name="token">The invitation token.</param>
        /// <returns>200 OK if the invitation is accepted; 400 Bad Request if there is an error.</returns>
        [Authorize]
        [HttpGet("accept")]
        public async Task<IActionResult> AcceptInvitation(string token)
        {
            _logger.LogInformation("Accepting invitation with token: {Token}", token);

            try
            {
                var isAccepted = await _invitationsService.AcceptInvitationAsync(token);
                _logger.LogInformation("Invitation with token: {Token} accepted successfully", token);
                return Ok(isAccepted);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Error accepting invitation with token: {Token}. Message: {Message}", token, ex.Message);
                return BadRequest(ex.Message);
            }
        }

        #endregion

        #region Create

        /// <summary>
        /// Creates an invitation to a project.
        /// </summary>
        /// <param name="projectId">The ID of the project.</param>
        /// <param name="email">The email address to send the invitation to.</param>
        /// <returns>200 OK if the invitation is created successfully; 400 Bad Request if there is an error.</returns>
        [Authorize]
        [HttpPost("to-project")]
        public async Task<IActionResult> CreateInvitationAsync(int projectId, string email)
        {
            _logger.LogInformation("Creating invitation for project ID: {ProjectId} to email: {Email}", projectId, email);

            try
            {
                await _invitationsService.CreateInvitationAsync(projectId, email);
                _logger.LogInformation("Invitation for project ID: {ProjectId} to email: {Email} created successfully", projectId, email);
                return Ok("Invited");
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Error creating invitation for project ID: {ProjectId} to email: {Email}. Message: {Message}", projectId, email, ex.Message);
                return BadRequest(ex.Message);
            }
        }

        #endregion
    }
}
