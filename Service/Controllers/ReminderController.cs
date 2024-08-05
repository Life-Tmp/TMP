using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TMPApplication.DTOs.ReminderDtos;
using TMPApplication.Interfaces.Reminders;
using System.Threading.Tasks;

namespace TMPService.Controllers
{
    /// <summary>
    /// Controller for managing reminders.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ReminderController : ControllerBase
    {
        private readonly IReminderService _reminderService;
        private readonly ILogger<ReminderController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReminderController"/> class.
        /// </summary>
        /// <param name="reminderService">The reminder service.</param>
        /// <param name="logger">The logger.</param>
        public ReminderController(IReminderService reminderService, ILogger<ReminderController> logger)
        {
            _reminderService = reminderService;
            _logger = logger;
        }

        #region Read
        /// <summary>
        /// Fetches a reminder by its ID.
        /// </summary>
        /// <param name="id">The reminder ID.</param>
        /// <returns>The reminder details.</returns>
        /// <response code="200">Returns the reminder details.</response>
        /// <response code="204">If the reminder is not found.</response>
        /// <response code="400">If the reminder ID is not valid.</response>
        [HttpGet("{id:int}")]
        [Authorize]
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

        /// <summary>
        /// Fetches reminders for a specific task.
        /// </summary>
        /// <param name="id">The task ID.</param>
        /// <returns>List of reminders for the task.</returns>
        /// <response code="200">Returns the list of reminders.</response>
        /// <response code="400">If the task ID is not valid.</response>
        [HttpGet("task/{id:int}")]
        [Authorize]
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
        /// <summary>
        /// Creates a new reminder.
        /// </summary>
        /// <param name="createReminderDto">The reminder creation details.</param>
        /// <returns>A success message.</returns>
        /// <response code="200">If the reminder is created successfully.</response>
        /// <response code="400">If the reminder data is invalid.</response>
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

        /// <summary>
        /// Processes a reminder (for testing purposes).
        /// </summary>
        /// <param name="reminderId">The reminder ID.</param>
        /// <returns>An OK result.</returns>
        /// <response code="200">If the reminder is processed successfully.</response>
        [HttpPost("process-testing")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ProcessReminder(int reminderId)
        {
            _logger.LogInformation("Processing reminder with ID: {ReminderId}", reminderId);
            await _reminderService.ProcessReminder(reminderId);
            return Ok();
        }
        #endregion

        #region Update

        /// <summary>
        /// Updates a reminder.
        /// </summary>
        /// <param name="id">The reminder ID.</param>
        /// <param name="reminderDto">The updated reminder details.</param>
        /// <returns>A success message.</returns>
        /// <response code="200">If the reminder is updated successfully.</response>
        /// <response code="400">If the reminder data is invalid.</response>
        [HttpPatch("{id:int}")]
        [Authorize]
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

        /// <summary>
        /// Deletes a reminder by its ID.
        /// </summary>
        /// <param name="id">The reminder ID.</param>
        /// <returns>A success message.</returns>
        /// <response code="200">If the reminder is deleted successfully.</response>
        /// <response code="400">If the reminder ID is not valid.</response>
        /// <response code="404">If the reminder is not found.</response>
        [HttpDelete("{id:int}")]
        [Authorize]
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
