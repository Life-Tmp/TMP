using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using TMP.Application.DTOs.CommentDtos;
using TMP.Application.DTOs.SubtaskDtos;
using TMP.Application.DTOs.TaskDtos;
using TMPApplication.Interfaces;
using TMPApplication.Interfaces.Tasks;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TMPService.Controllers.Tasks
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly ITimeTrackingService _timeTrackingService;
        private readonly ISearchService<TaskDto> _searchService;
        private readonly ILogger<TaskController> _logger;

        public TaskController(ITaskService taskService, ITimeTrackingService timeTrackingService, ISearchService<TaskDto> searchService, ILogger<TaskController> logger)
        {
            _taskService = taskService;
            _timeTrackingService = timeTrackingService;
            _searchService = searchService;
            _logger = logger;
        }

        #region Read
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasks()
        {
            _logger.LogInformation("Fetching tasks");

            var tasks = await _taskService.GetTasksAsync();
            return Ok(tasks);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TaskDto>> GetTask(int id)
        {
            _logger.LogInformation("Fetching task with ID: {TaskId}", id);

            var task = await _taskService.GetTaskByIdAsync(id);
            if (task == null)
            {
                _logger.LogWarning("Task with ID: {TaskId} not found", id);
                return NotFound();
            }

            return Ok(task);
        }

        [Authorize]
        [HttpGet("{id}/assigned-users")]
        public async Task<ActionResult<IEnumerable<UserDetailsDto>>> GetAssignedUsers(int id)
        {
            _logger.LogInformation("Fetching assigned users for task with ID: {TaskId}", id);

            var users = await _taskService.GetAssignedUsersAsync(id);
            if (users == null || !users.Any())
            {
                _logger.LogWarning("No users assigned to task with ID: {TaskId}", id);
                return NotFound("No users assigned to this task.");
            }

            return Ok(users);
        }

        [Authorize]
        [HttpGet("my-tasks")]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasksByUser()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                _logger.LogWarning("Unauthorized access to my-tasks");
                return Unauthorized();
            }

            _logger.LogInformation("Fetching tasks for user with ID: {UserId}", userId);

            var tasks = await _taskService.GetTasksByUserIdAsync(userId);
            return Ok(tasks);
        }

        [Authorize]
        [HttpGet("{id}/work-time")]
        public async Task<ActionResult<WorkTimeDto>> GetTotalTimeSpent(int id)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                _logger.LogWarning("Unauthorized access to get total time spent");
                return Unauthorized();
            }

            _logger.LogInformation("Fetching total time spent for task with ID: {TaskId} by user with ID: {UserId}", id, userId);

            var workTimeDto = await _timeTrackingService.GetTotalTimeSpentAsync(id, userId);
            return Ok(workTimeDto);
        }

        [Authorize]
        [HttpGet("{id:int}/time-spent-by-users")]
        public async Task<ActionResult<IEnumerable<UserTimeSpentDto>>> GetTimeSpentByUsers(int id)
        {
            var currentUserId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId == null)
            {
                _logger.LogWarning("Unauthorized access to get time spent by users");
                return Unauthorized();
            }

            try
            {
                _logger.LogInformation("Fetching time spent by users for task with ID: {TaskId} by user with ID: {UserId}", id, currentUserId);

                var timeSpentByUsers = await _timeTrackingService.GetTimeSpentByUsersAsync(id, currentUserId);
                return Ok(timeSpentByUsers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching time spent by users for task with ID: {TaskId}", id);
                return Forbid(ex.Message);
            }
        }

        [HttpGet("{id}/comments")]
        public async Task<ActionResult<IEnumerable<CommentDto>>> GetComments(int id)
        {
            _logger.LogInformation("Fetching comments for task with ID: {TaskId}", id);

            var comments = await _taskService.GetCommentsByTaskIdAsync(id);
            if (!comments.Any())
            {
                _logger.LogWarning("No comments found for task with ID: {TaskId}", id);
                return NotFound("No comments found for this task.");
            }

            return Ok(comments);
        }

        [HttpGet("{id}/subtasks")]
        public async Task<ActionResult<IEnumerable<SubtaskDto>>> GetSubtasks(int id)
        {
            _logger.LogInformation("Fetching subtasks for task with ID: {TaskId}", id);

            var subtasks = await _taskService.GetSubtasksByTaskIdAsync(id);
            if (!subtasks.Any())
            {
                _logger.LogWarning("No subtasks found for task with ID: {TaskId}", id);
                return NotFound("No subtasks found for this task.");
            }

            return Ok(subtasks);
        }

        [HttpGet("{taskid:int}/duration")]
        public async Task<IActionResult> GetTaskDuration(int taskid)
        {
            _logger.LogInformation("Fetching task duration for task with ID: {TaskId}", taskid);

            var duration = await _taskService.GetTaskDurationAsync(taskid);
            if (duration == null)
            {
                _logger.LogWarning("Task duration not found or task isn't done yet for task with ID: {TaskId}", taskid);
                return NotFound("Task isn't done yet or it doesn't exist");
            }

            if (duration.Value.TotalSeconds == 0)
            {
                _logger.LogWarning("Task isn't done yet for task with ID: {TaskId}", taskid);
                return NotFound("Task isn't done yet");
            }

            return Ok(duration);
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<TaskDto>>> SearchTasks([FromQuery] string searchTerm)
        {
            _logger.LogInformation("Searching tasks with search term: {SearchTerm}", searchTerm);

            var tasks = await _searchService.SearchDocumentAsync(searchTerm, "tasks");
            return Ok(tasks);
        }
        #endregion

        #region Create
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<TaskDto>> AddTask([FromBody] AddTaskDto newTask)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                _logger.LogWarning("Unauthorized access to add task");
                return Unauthorized();
            }

            _logger.LogInformation("Adding new task");

            var task = await _taskService.AddTaskAsync(newTask);
            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
        }

        [Authorize]
        [HttpPost("assign-user")]
        public async Task<IActionResult> AssignUserToTask([FromBody] AssignUserToTaskDto assignUserToTaskDto)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                _logger.LogWarning("Unauthorized access to assign user to task");
                return Unauthorized();
            }

            _logger.LogInformation("Assigning user with ID: {UserId} to task with ID: {TaskId}", assignUserToTaskDto.UserId, assignUserToTaskDto.TaskId);

            var result = await _taskService.AssignUserToTaskAsync(assignUserToTaskDto);
            if (!result)
            {
                _logger.LogWarning("Failed to assign user with ID: {UserId} to task with ID: {TaskId}", assignUserToTaskDto.UserId, assignUserToTaskDto.TaskId);
                return BadRequest("User could not be assigned to task.");
            }

            return Ok("User assigned to task successfully.");
        }
        #endregion

        #region Update
        [Authorize]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] UpdateTaskDto updatedTask)
        {
            _logger.LogInformation("Updating task with ID: {TaskId}", id);

            var result = await _taskService.UpdateTaskAsync(id, updatedTask);
            if (!result)
            {
                _logger.LogWarning("Task with ID: {TaskId} not found", id);
                return NotFound();
            }

            return NoContent();
        }

        [HttpPatch("update-status")]
        public async Task<IActionResult> UpdateStatusOfTask([FromBody] UpdateTaskStatusDto updateTaskStatusDto)
        {
            _logger.LogInformation("Updating status of task with ID: {TaskId} to {Status}", updateTaskStatusDto.TaskId, updateTaskStatusDto.Status);

            var statusChanged = await _taskService.UpdateStatusOfTask(updateTaskStatusDto);
            if (!statusChanged)
            {
                _logger.LogWarning("Failed to update status of task with ID: {TaskId}", updateTaskStatusDto.TaskId);
                return NotFound(new { Message = "Task not found or unable to update status." });
            }

            return Ok(new
            {
                Message = "Task status updated successfully.",
                TaskId = updateTaskStatusDto.TaskId,
                NewStatus = updateTaskStatusDto.Status.ToString()
            });
        }

        [Authorize]
        [HttpPost("{id:int}/start-timer")]
        public async Task<IActionResult> StartTimer(int id)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                _logger.LogWarning("Unauthorized access to start timer");
                return Unauthorized();
            }

            _logger.LogInformation("Starting timer for task with ID: {TaskId} by user with ID: {UserId}", id, userId);

            await _timeTrackingService.StartTimerAsync(id, userId);
            return Ok("Timer started.");
        }

        [Authorize]
        [HttpPost("{id:int}/stop-timer")]
        public async Task<IActionResult> StopTimer(int id)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                _logger.LogWarning("Unauthorized access to stop timer");
                return Unauthorized();
            }

            _logger.LogInformation("Stopping timer for task with ID: {TaskId} by user with ID: {UserId}", id, userId);

            await _timeTrackingService.StopTimerAsync(id, userId);
            return Ok("Timer stopped.");
        }
        #endregion

        #region Delete
        [Authorize]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            _logger.LogInformation("Deleting task with ID: {TaskId}", id);

            var result = await _taskService.DeleteTaskAsync(id);
            if (!result)
            {
                _logger.LogWarning("Task with ID: {TaskId} not found", id);
                return NotFound();
            }

            return NoContent();
        }

        [Authorize]
        [HttpDelete("remove-user")]
        public async Task<IActionResult> RemoveUserFromTask([FromBody] RemoveUserFromTaskDto removeUserFromTaskDto)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                _logger.LogWarning("Unauthorized access to remove user from task");
                return Unauthorized();
            }

            _logger.LogInformation("Removing user with ID: {UserId} from task with ID: {TaskId}", removeUserFromTaskDto.UserId, removeUserFromTaskDto.TaskId);

            var result = await _taskService.RemoveUserFromTaskAsync(removeUserFromTaskDto);
            if (!result)
            {
                _logger.LogWarning("Failed to remove user with ID: {UserId} from task with ID: {TaskId}", removeUserFromTaskDto.UserId, removeUserFromTaskDto.TaskId);
                return BadRequest("User could not be removed from task.");
            }

            return Ok("User removed from task successfully.");
        }
        #endregion
    }
}