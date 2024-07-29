using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using TMPApplication.Notifications;

namespace TMPService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController: ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllNotifications()
        {
            var notifications = await _notificationService.GetAllNotifications();
            return Ok(notifications);
        }

        [HttpGet("latest")]
        [Authorize]
        public async Task<IActionResult> GetLatestNotifications(int numberOfLatestNotifications)
        {
            var latestNotifications = await _notificationService.GetLatestNotifications(numberOfLatestNotifications);
            return Ok(latestNotifications);
        }

    }
}
