using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TMP.Application.DTOs.SubtaskDtos;
using TMPApplication.Interfaces.Subtasks;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TMPService.Controllers.Subtasks
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubtaskController : ControllerBase
    {
        private readonly ISubtaskService _subtaskService;
        private readonly ILogger<SubtaskController> _logger;

        public SubtaskController(ISubtaskService subtaskService, ILogger<SubtaskController> logger)
        {
            _subtaskService = subtaskService;
            _logger = logger;
        }

        #region Read
        /// <summary>
        /// Retrieves a list of all subtasks.
        /// </summary>
        /// <returns>200 OK with a list of subtasks.</returns>
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SubtaskDto>>> GetSubtasks()
        {
            _logger.LogInformation("Fetching all subtasks");
            var allSubtasks = await _subtaskService.GetAllSubtasksAsync();
            return Ok(allSubtasks);
        }

        /// <summary>
        /// Retrieves a subtask by its ID.
        /// </summary>
        /// <param name="id">The ID of the subtask to retrieve.</param>
        /// <returns>200 OK with the subtask details; 404 Not Found if the subtask does not exist.</returns>
        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<SubtaskDto>> GetSubtask(int id)
        {
            _logger.LogInformation("Fetching subtask with ID: {SubtaskId}", id);

            var subtask = await _subtaskService.GetSubtaskByIdAsync(id);
            if (subtask == null)
            {
                _logger.LogWarning("Subtask with ID: {SubtaskId} not found", id);
                return NotFound();
            }

            return Ok(subtask);
        }
        #endregion

        #region Create
        /// <summary>
        /// Adds a new subtask.
        /// </summary>
        /// <param name="newSubtask">The details of the subtask to add.</param>
        /// <returns>The created subtask.</returns>
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<SubtaskDto>> AddSubtask([FromBody] AddSubtaskDto newSubtask)
        {
            _logger.LogInformation("Adding new subtask for task with ID: {TaskId}", newSubtask.TaskId);

            try
            {
                var subtask = await _subtaskService.AddSubtaskAsync(newSubtask);
                return CreatedAtAction(nameof(GetSubtask), new { id = subtask.Id }, subtask);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding subtask for task with ID: {TaskId}", newSubtask.TaskId);
                return BadRequest(new { Message = ex.Message });
            }
        }
        #endregion

        #region Update
        /// <summary>
        /// Updates an existing subtask.
        /// </summary>
        /// <param name="id">The ID of the subtask to update.</param>
        /// <param name="updatedSubtask">The updated subtask details.</param>
        /// <returns>200 OK if the update is successful; 404 Not Found if the subtask does not exist.</returns>
        [Authorize]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateSubtask(int id, [FromBody] UpdateSubtaskDto updatedSubtask)
        {
            _logger.LogInformation("Updating subtask with ID: {SubtaskId}", id);

            var result = await _subtaskService.UpdateSubtaskAsync(id, updatedSubtask);
            if (!result)
            {
                _logger.LogWarning("Subtask with ID: {SubtaskId} not found", id);
                return NotFound(new { Message = "Subtask not found" });
            }

            _logger.LogInformation("Subtask with ID: {SubtaskId} updated successfully", id);
            return Ok(new { Message = "Subtask updated successfully" });
        }

        /// <summary>
        /// Updates the completion status of a subtask.
        /// </summary>
        /// <param name="dto">The completion status details.</param>
        /// <returns>200 OK if the update is successful; 400 Bad Request if the update fails.</returns>
        [Authorize]
        [HttpPatch("update-completion")]
        public async Task<IActionResult> UpdateSubtaskCompletion([FromBody] UpdateSubtaskCompletionDto dto)
        {
            _logger.LogInformation("Updating completion status of subtask with ID: {SubtaskId}", dto.SubtaskId);

            var success = await _subtaskService.UpdateSubtaskCompletionAsync(dto);
            if (!success)
            {
                _logger.LogWarning("Failed to update completion status of subtask with ID: {SubtaskId}", dto.SubtaskId);
                return BadRequest("Failed to update subtask completion status.");
            }

            return Ok(new
            {
                Message = "Subtask completion status updated successfully.",
                SubtaskId = dto.SubtaskId,
                IsCompleted = dto.IsCompleted
            });
        }
        #endregion

        #region Delete
        /// <summary>
        /// Deletes a subtask by its ID.
        /// </summary>
        /// <param name="id">The ID of the subtask to delete.</param>
        /// <returns>204 No Content if the deletion is successful; 404 Not Found if the subtask does not exist.</returns>
        [Authorize]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteSubtask(int id)
        {
            _logger.LogInformation("Deleting subtask with ID: {SubtaskId}", id);

            var result = await _subtaskService.DeleteSubtaskAsync(id);
            if (!result)
            {
                _logger.LogWarning("Subtask with ID: {SubtaskId} not found", id);
                return NotFound();
            }

            _logger.LogInformation("Subtask with ID: {SubtaskId} deleted successfully", id);
            return NoContent();
        }
        #endregion
    }
}
