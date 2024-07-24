using System.Collections.Generic;
using System.Threading.Tasks;
using TMP.Application.DTOs.TaskDtos;
using TMPDomain.ValueObjects;

namespace TMP.Application.Tasks
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskDto>> GetAllTasksAsync();
        Task<IEnumerable<TaskDto>> GetTasksByProjectIdAsync(int projectId);
        Task<TaskDto> GetTaskByIdAsync(int id);
        Task<TaskDto> AddTaskAsync(AddTaskDto newTask);
        Task<bool> UpdateTaskAsync(int id, AddTaskDto updatedTask);
        Task<bool> DeleteTaskAsync(int id);
        Task<IEnumerable<TaskDto>> GetTasksAsync(int? projectId);
        Task<bool> AssignUserToTask(int taskId, string userId);
        Task<bool> UpdateStatusOfTask(int taskId, StatusOfTask newState);
    }
}
