using Microsoft.AspNetCore.Mvc;
using TMPApplication.DTOs.ReminderDtos;
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

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateReminder(int id,[FromBody] ReminderDto reminderdto)
        {
            var isUpdated = await _reminderService.UpdateReminder(id,reminderdto);
            if (isUpdated)
            {
                return Ok("Updated successfully");
            }
            return BadRequest();
        }



        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReminder(int id)
        {
            if (id <= 0)
            {
                return BadRequest("The reminder id must be a positive integer.");
            }
            var isDeleted = await _reminderService.DeleteReminder(id);
            if (isDeleted)
            {
                return Ok("Deleted successfully");
            }

            return NotFound("Reminder not found.");
        }

    }
}
