using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TMPApplication.DTOs.ReminderDtos;
using TMPApplication.Interfaces.Reminders;
using System.Threading.Tasks;

namespace TMPService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReminderController : ControllerBase
    {
        private readonly IReminderService _reminderService;
        private readonly ILogger<ReminderController> _logger;

        public ReminderController(IReminderService reminderService, ILogger<ReminderController> logger)
        {
            _reminderService = reminderService;
            _logger = logger;
        }

        #region Read
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetReminderAsync(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid reminder ID: {ReminderId}", id);
                return BadRequest("The reminderId must be a positive integer.");
            }

            _logger.LogInformation("Fetching reminder with ID: {ReminderId}", id);
            var reminder = await _reminderService.GetReminderAsync(id);

            if (reminder != null)
            {
                return Ok(reminder);
            }

            _logger.LogWarning("Reminder with ID: {ReminderId} not found", id);
            return NoContent();
        }

        [HttpGet("task/{id:int}")]
        public async Task<IActionResult> GetRemindersForTask(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid task ID: {TaskId}", id);
                return BadRequest("Not a valid task Id");
            }

            _logger.LogInformation("Fetching reminders for task with ID: {TaskId}", id);
            var reminderList = await _reminderService.GetRemindersForTask(id);

            return Ok(reminderList);
        }
        #endregion

        #region Create
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateReminder([FromBody] CreateReminderDto createReminderDto)
        {
            if (createReminderDto == null)
            {
                _logger.LogWarning("Invalid reminder data provided");
                return BadRequest("Invalid reminder data.");
            }

            _logger.LogInformation("Creating reminder for task with ID: {TaskId}", createReminderDto.TaskId);
            await _reminderService.CreateReminderAsync(createReminderDto);
            return Ok("Successfully created a reminder");
        }

        [HttpPost("process-testing")]
        public async Task<IActionResult> ProcessReminder(int reminderId)
        {
            _logger.LogInformation("Processing reminder with ID: {ReminderId}", reminderId);
            await _reminderService.ProcessReminder(reminderId);
            return Ok();
        }
        #endregion

        #region Update
        [HttpPatch("{id:int}")]
        public async Task<IActionResult> UpdateReminder(int id, [FromBody] ReminderDto reminderDto)
        {
            _logger.LogInformation("Updating reminder with ID: {ReminderId}", id);
            var isUpdated = await _reminderService.UpdateReminder(id, reminderDto);
            if (isUpdated)
            {
                return Ok("Updated successfully");
            }

            _logger.LogWarning("Failed to update reminder with ID: {ReminderId}", id);
            return BadRequest();
        }
        #endregion

        #region Delete
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteReminder(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid reminder ID: {ReminderId}", id);
                return BadRequest("The reminder id must be a positive integer.");
            }

            _logger.LogInformation("Deleting reminder with ID: {ReminderId}", id);
            var isDeleted = await _reminderService.DeleteReminder(id);
            if (isDeleted)
            {
                return Ok("Deleted successfully");
            }

            _logger.LogWarning("Failed to delete reminder with ID: {ReminderId}", id);
            return NotFound("Reminder not found.");
        }
        #endregion
    }
}
