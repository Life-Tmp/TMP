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
        /// <summary>
        /// Retrieves a list of all tasks.
        /// </summary>
        /// <returns>200 OK with a list of tasks.</returns>
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasks()
        {
            _logger.LogInformation("Fetching tasks");

            var tasks = await _taskService.GetTasksAsync();
            return Ok(tasks);
        }

        /// <summary>
        /// Retrieves a task by its ID.
        /// </summary>
        /// <param name="id">The ID of the task to retrieve.</param>
        /// <returns>200 OK with the task details; 404 Not Found if the task does not exist.</returns>
        [HttpGet("{id:int}")]
        [Authorize]
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

        /// <summary>
        /// Retrieves the users assigned to a specific task.
        /// </summary>
        /// <param name="id">The ID of the task.</param>
        /// <returns>200 OK with a list of assigned users; 404 Not Found if no users are assigned.</returns>
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

        /// <summary>
        /// Retrieves the tasks assigned to the currently logged-in user.
        /// </summary>
        /// <returns>200 OK with a list of tasks assigned to the user; 401 Unauthorized if the user is not authenticated.</returns>
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

        /// <summary>
        /// Retrieves the total time spent on a task by the currently logged-in user.
        /// </summary>
        /// <param name="id">The ID of the task.</param>
        /// <returns>200 OK with total time spent; 401 Unauthorized if the user is not authenticated.</returns>
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

        /// <summary>
        /// Retrieves the time spent on a task by all users.
        /// </summary>
        /// <param name="id">The ID of the task.</param>
        /// <returns>200 OK with time spent by users; 401 Unauthorized if the user is not authenticated; 403 Forbidden if an error occurs.</returns>
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

        /// <summary>
        /// Retrieves the comments for a task.
        /// </summary>
        /// <param name="id">The ID of the task.</param>
        /// <returns>200 OK with a list of comments; 404 Not Found if no comments are found.</returns>
        [HttpGet("{id}/comments")]
        [Authorize]
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

        /// <summary>
        /// Retrieves the subtasks for a task.
        /// </summary>
        /// <param name="id">The ID of the task.</param>
        /// <returns>200 OK with a list of subtasks; 404 Not Found if no subtasks are found.</returns>
        [HttpGet("{id}/subtasks")]
        [Authorize]
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

        /// <summary>
        /// Retrieves the duration of a task.
        /// </summary>
        /// <param name="taskid">The ID of the task.</param>
        /// <returns>200 OK with the duration; 404 Not Found if the task is not done yet or does not exist.</returns>
        [HttpGet("{taskid:int}/duration")]
        [Authorize]
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

        /// <summary>
        /// Searches for tasks based on a search term.
        /// </summary>
        /// <param name="searchTerm">The term to search for.</param>
        /// <returns>200 OK with a list of tasks matching the search term.</returns>
        [HttpGet("search")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<TaskDto>>> SearchTasks([FromQuery] string searchTerm)
        {
            _logger.LogInformation("Searching tasks with search term: {SearchTerm}", searchTerm);

            var tasks = await _searchService.SearchDocumentAsync(searchTerm, "tasks");
            return Ok(tasks);
        }
        #endregion

        #region Create

        /// <summary>
        /// Adds a new task.
        /// </summary>
        /// <param name="newTask">The details of the task to add.</param>
        /// <returns>The created task.</returns>
        [HttpPost]
        [Authorize]
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

        /// <summary>
        /// Assigns a user to a task. Requires authentication.
        /// </summary>
        /// <param name="assignUserToTaskDto">DTO containing the task ID and user ID.</param>
        /// <returns>An IActionResult indicating success or failure.</returns>
        /// <response code="200">User assigned to task successfully.</response>
        /// <response code="400">Failed to assign user to task.</response>
        [HttpPost("assign-user")]
        [Authorize]
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

        /// <summary>
        /// Updates a task.
        /// </summary>
        /// <param name="id">The ID of the task to update.</param>
        /// <returns>No content if successful, otherwise a bad request.</returns>
        [HttpPut("{id:int}")]
        [Authorize]
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

        /// <summary>
        /// Updates the status of a specified task. Requires authentication.
        /// </summary>
        /// <param name="updateTaskStatusDto">The DTO containing the task ID and the new status for the task.</param>
        /// <returns>An IActionResult indicating success or failure.</returns>
        /// <response code="200">Task status updated successfully.</response>
        /// <response code="404">Task not found or unable to update status.</response>
        [HttpPatch("update-status")]
        [Authorize]
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

        /// <summary>
        /// Starts a timer for a specified task. Requires authentication.
        /// </summary>
        /// <param name="id">The ID of the task to start the timer for.</param>
        /// <returns>An IActionResult indicating success or failure.</returns>
        /// <response code="200">Timer started successfully.</response>
        [HttpPost("{id:int}/start-timer")]
        [Authorize]
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

        /// <summary>
        /// Stops the timer for a specified task. Requires authentication.
        /// </summary>
        /// <param name="id">The ID of the task to stop the timer for.</param>
        /// <returns>An IActionResult indicating success or failure.</returns>
        /// <response code="200">Timer stopped successfully.</response>
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

        /// <summary>
        /// Deletes a task by its ID.
        /// </summary>
        /// <param name="id">The ID of the task to delete.</param>
        /// <returns>204 No Content if the deletion is successful; 404 Not Found if the task does not exist.</returns>
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

        /// <summary>
        /// Removes a user from a specified task. Requires authentication.
        /// </summary>
        /// <param name="removeUserFromTaskDto">The DTO containing the task ID and user ID to be removed.</param>
        /// <returns>An IActionResult indicating success or failure.</returns>
        /// <response code="200">User removed from task successfully.</response>
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

        /// <summary>
        /// Adds a task to the calendar.
        /// </summary>
        /// <param name="taskId">The ID of the task to add to the calendar.</param>
        /// <returns>200 OK if the task is successfully added to the calendar; 400 Bad Request if the task ID is invalid.</returns>
        [Authorize]
        [HttpPost("add-to-calendar")]
        public async Task<IActionResult> AddTaskToCalendar(int taskId)
        {
            var eventToAdd = await _taskService.AddTaskAsEventInCalendar(taskId);

            if (eventToAdd == null)
            {
                return BadRequest();
            }
            return Ok(eventToAdd);
        }
        #endregion

    }
}