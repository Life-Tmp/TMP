using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TMPApplication.Hubs;
using TMPApplication.Notifications;

namespace TMPService.Tasks
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly INotificationService _noti;
        private readonly IHubContext<NotificationHub> _notificaitonHub;
        public ValuesController(INotificationService noti, IHubContext<NotificationHub> notificaitonHub)
        {
            _noti = noti;
            _notificaitonHub = notificaitonHub;
        }

        [HttpPost]
        public async Task<IActionResult> EmailIsRead()
        {

            var isRead = _noti.MarksAsRead(3);
            if (isRead == null)
            {
                return NotFound();
            }
            return Ok(isRead);
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendNotification(string userId, string message)
        {
            await _notificaitonHub.Clients.User(userId).SendAsync("ReceiveNotification", message);
            await _notificaitonHub.Clients.All.SendAsync("RecieveNotification", message);
            return Ok("Notification sent");
        }

    }
}
