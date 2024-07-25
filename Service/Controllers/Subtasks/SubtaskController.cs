using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TMP.Application.DTOs.SubtaskDtos;
using TMPApplication.Interfaces.Subtasks;

namespace TMPService.Controllers.Subtasks
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubtaskController : ControllerBase
    {
        private readonly ISubtaskService _subtaskService;

        public SubtaskController(ISubtaskService subtaskService)
        {
            _subtaskService = subtaskService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SubtaskDto>>> GetSubtasks([FromQuery] int? taskId)
        {
            if (taskId.HasValue)
            {
                var subtasks = await _subtaskService.GetSubtasksByTaskIdAsync(taskId.Value);
                return Ok(subtasks);
            }

            var allSubtasks = await _subtaskService.GetAllSubtasksAsync();
            return Ok(allSubtasks);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<SubtaskDto>> GetSubtask(int id)
        {
            var subtask = await _subtaskService.GetSubtaskByIdAsync(id);
            if (subtask == null) return NotFound();

            return Ok(subtask);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<SubtaskDto>> AddSubtask([FromBody] AddSubtaskDto newSubtask)
        {
            try
            {
                var subtask = await _subtaskService.AddSubtaskAsync(newSubtask);
                return CreatedAtAction(nameof(GetSubtask), new { id = subtask.Id }, subtask);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [Authorize]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateSubtask(int id, [FromBody] UpdateSubtaskDto updatedSubtask)
        {
            var result = await _subtaskService.UpdateSubtaskAsync(id, updatedSubtask);
            if (!result) return NotFound(new { Message = "Subtask not found" });

            return Ok(new { Message = "Subtask updated successfully" });
        }


        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteSubtask(int id)
        {
            var result = await _subtaskService.DeleteSubtaskAsync(id);
            if (!result) return NotFound();

            return NoContent();
        }

        [Authorize]
        [HttpPatch("update-completion")]
        public async Task<IActionResult> UpdateSubtaskCompletion([FromBody] UpdateSubtaskCompletionDto dto)
        {
            var success = await _subtaskService.UpdateSubtaskCompletionAsync(dto);
            if (!success) return BadRequest("Failed to update subtask completion status.");

            return Ok(new
            {
                Message = "Subtask completion status updated successfully.",
                SubtaskId = dto.SubtaskId,
                IsCompleted = dto.IsCompleted
            });
        }
    }
}
