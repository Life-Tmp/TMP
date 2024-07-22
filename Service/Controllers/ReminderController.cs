using Microsoft.AspNetCore.Mvc;
using TMPApplication.Interfaces.Reminders;

namespace TMPService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReminderController : ControllerBase
    {
        private readonly IReminderService _reminderService;

        public ReminderController(IReminderService reminderService)
        {
            _reminderService = reminderService;
        }


        [HttpPost("add-reminder")]
        public async Task<IActionResult> CreateReminder(string description, DateTime reminderDate, int taskId)
        {
            await _reminderService.CreateReminderAsync( description, reminderDate, taskId);
            return Ok("Successfully created a reminder");
        }

        [HttpPost("process-reminder")]
        public async Task<IActionResult> ProcessReminder(int reminderId)
        {
            await _reminderService.ProcessReminder(reminderId);
            return Ok();
        }

    }
}
