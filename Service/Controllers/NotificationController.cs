using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TMPApplication.Notifications;
using System.Threading.Tasks;

namespace TMPService.Controllers
{
    // <summary>
    /// Controller for managing notifications.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationController"/> class.
        /// </summary>
        /// <param name="notificationService">The notification service.</param>
        /// <param name="logger">The logger.</param>
        public NotificationController(INotificationService notificationService, ILogger<NotificationController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        #region Read

        /// <summary>
        /// Fetches all notifications for the current user.
        /// </summary>
        /// <returns>List of notifications.</returns>
        /// <response code="200">Returns the list of notifications.</response>
        /// <response code="500">If there is an unexpected error.</response>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllNotifications()
        {
            _logger.LogInformation("Fetching all notifications for the current user");
            var notifications = await _notificationService.GetAllNotifications();
            return Ok(notifications);
        }

        /// <summary>
        /// Fetches the latest notifications for the current user.
        /// </summary>
        /// <param name="numberOfLatestNotifications">The number of latest notifications to fetch.</param>
        /// <returns>List of latest notifications.</returns>
        /// <response code="200">Returns the list of latest notifications.</response>
        /// <response code="500">If there is an unexpected error.</response>
        [HttpGet("latest")]
        [Authorize]
        public async Task<IActionResult> GetLatestNotifications(int numberOfLatestNotifications)
        {
            _logger.LogInformation("Fetching all notifications for the current user");
            try
            {
                var notifications = await _notificationService.GetAllNotifications();
                return Ok(notifications);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An unexpected error occurred while fetching notifications");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred");
            }
        }
        #endregion
    }
}
