using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TMPApplication.Notifications;
using System.Threading.Tasks;

namespace TMPService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(INotificationService notificationService, ILogger<NotificationController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        #region Read
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllNotifications()
        {
            _logger.LogInformation("Fetching all notifications for the current user");
            var notifications = await _notificationService.GetAllNotifications();
            return Ok(notifications);
        }

        [HttpGet("latest")]
        [Authorize]
        public async Task<IActionResult> GetLatestNotifications(int numberOfLatestNotifications)
        {
            _logger.LogInformation("Fetching the latest {Number} notifications for the current user", numberOfLatestNotifications);
            var latestNotifications = await _notificationService.GetLatestNotifications(numberOfLatestNotifications);
            return Ok(latestNotifications);
        }
        #endregion
    }
}
