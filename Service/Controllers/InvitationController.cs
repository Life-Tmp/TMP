using Microsoft.AspNetCore.Mvc;
using TMPApplication.Interfaces.Invitations;

namespace TMPService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvitationController: ControllerBase
    {
        private readonly IInvitationsService _invitationsService;

        public InvitationController(IInvitationsService invitationsService)
        {
            _invitationsService = invitationsService;
        }

        [HttpGet("accept")]
        public async Task<IActionResult> AcceptInvitation(string token)
        {
            var isAccepted = _invitationsService.AcceptInvitationAsync(token);

            return Ok(isAccepted);

        }

        [HttpPost("to-project")]
        public async Task<IActionResult> CreateInvitationAsync(int projectId, string email)
        {
           await _invitationsService.CreateInvitationAsync(projectId, email);
            return Ok("Invited");
        }

    }
}
