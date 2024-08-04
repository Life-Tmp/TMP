using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TMPApplication.Interfaces.Invitations;
using System;
using System.Threading.Tasks;

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
