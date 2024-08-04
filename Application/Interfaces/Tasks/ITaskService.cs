using TMP.Application.DTOs.CommentDtos;
using TMP.Application.DTOs.SubtaskDtos;
using TMP.Application.DTOs.TaskDtos;

namespace TMPApplication.Interfaces.Tasks
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskDto>> GetTasksAsync();
        Task<TaskDto> GetTaskByIdAsync(int id);
        Task<IEnumerable<UserDetailsDto>> GetAssignedUsersAsync(int taskId);
        Task<IEnumerable<TaskDto>> GetTasksByUserIdAsync(string userId);
        Task<TaskDto> AddTaskAsync(AddTaskDto newTask);
        Task<bool> AssignUserToTaskAsync(AssignUserToTaskDto assignUserToTaskDto);
        Task<bool> UpdateTaskAsync(int id, UpdateTaskDto updatedTask);
        Task<bool> UpdateStatusOfTask(UpdateTaskStatusDto updateTaskStatusDto);
        Task<bool> DeleteTaskAsync(int id);
        Task<bool> RemoveUserFromTaskAsync(RemoveUserFromTaskDto removeUserFromTaskDto);
        Task<TimeSpan?> GetTaskDurationAsync(int taskId);
        Task<IEnumerable<CommentDto>> GetCommentsByTaskIdAsync(int taskId);
        Task<IEnumerable<SubtaskDto>> GetSubtasksByTaskIdAsync(int taskId);
    }
}