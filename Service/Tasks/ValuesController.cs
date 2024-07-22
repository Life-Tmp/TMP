using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TMPApplication.Notifications;

namespace TMPService.Tasks
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly INotificationService _noti;
        public ValuesController(INotificationService noti)
        {
            _noti = noti;
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
    }
}
