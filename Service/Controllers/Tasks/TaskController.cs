﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TMP.Application.DTOs.TaskDtos;
using TMPApplication.Interfaces.Tasks;

namespace TMPService.Controllers.Tasks
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TaskController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasks([FromQuery] int? projectId)
        {
            var tasks = await _taskService.GetTasksAsync(projectId);
            return Ok(tasks);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TaskDto>> GetTask(int id)
        {
            var task = await _taskService.GetTaskByIdAsync(id);
            if (task == null) return NotFound();

            return Ok(task);
        }

        [Authorize]
        [HttpGet("{id}/assigned-users")]
        public async Task<ActionResult<IEnumerable<UserDetailsDto>>> GetAssignedUsers(int id)
        {
            var users = await _taskService.GetAssignedUsersAsync(id);
            if (users == null || !users.Any())
                return NotFound("No users assigned to this task.");

            return Ok(users);
        }

        [Authorize]
        [HttpGet("my-tasks")]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasksByUser()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var tasks = await _taskService.GetTasksByUserIdAsync(userId);
            return Ok(tasks);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<TaskDto>> AddTask([FromBody] AddTaskDto newTask)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var task = await _taskService.AddTaskAsync(newTask);
            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
        }

        [Authorize]
        [HttpPost("assign-user")]
        public async Task<IActionResult> AssignUserToTask([FromBody] AssignUserToTaskDto assignUserToTaskDto)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var result = await _taskService.AssignUserToTaskAsync(assignUserToTaskDto);
            if (!result) return BadRequest("User could not be assigned to task.");

            return Ok("User assigned to task successfully.");
        }

        [Authorize]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] AddTaskDto updatedTask)
        {
            var result = await _taskService.UpdateTaskAsync(id, updatedTask);
            if (!result) return NotFound();

            return NoContent();
        }

        [HttpPatch("update-status")]
        public async Task<IActionResult> UpdateStatusOfTask([FromBody] UpdateTaskStatusDto updateTaskStatusDto)
        {
            var statusChanged = await _taskService.UpdateStatusOfTask(updateTaskStatusDto);
            if (!statusChanged) return NotFound(new { Message = "Task not found or unable to update status." });

            return Ok(new
            {
                Message = "Task status updated successfully.",
                TaskId = updateTaskStatusDto.TaskId,
                NewStatus = updateTaskStatusDto.Status.ToString()
            });
        }

        [Authorize]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var result = await _taskService.DeleteTaskAsync(id);
            if (!result) return NotFound();

            return NoContent();
        }

        [Authorize]
        [HttpDelete("remove-user")]
        public async Task<IActionResult> RemoveUserFromTask([FromBody] RemoveUserFromTaskDto removeUserFromTaskDto)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var result = await _taskService.RemoveUserFromTaskAsync(removeUserFromTaskDto);
            if (!result) return BadRequest("User could not be removed from task.");

            return Ok("User removed from task successfully.");
        }
    }
}