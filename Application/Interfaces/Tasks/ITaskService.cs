using TMP.Application.DTOs.TaskDtos;

namespace TMPApplication.Interfaces.Tasks
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskDto>> GetAllTasksAsync();
        Task<IEnumerable<TaskDto>> GetTasksByProjectIdAsync(int projectId);
        Task<TaskDto> GetTaskByIdAsync(int id);
        Task<IEnumerable<TaskDto>> GetTasksAsync(int? projectId);
        Task<IEnumerable<UserDetailsDto>> GetAssignedUsersAsync(int taskId);
        Task<IEnumerable<TaskDto>> GetTasksByUserIdAsync(string userId);
        Task<TaskDto> AddTaskAsync(AddTaskDto newTask);
        Task<bool> AssignUserToTaskAsync(AssignUserToTaskDto assignUserToTaskDto);
        Task<bool> UpdateTaskAsync(int id, AddTaskDto updatedTask);
        Task<bool> UpdateStatusOfTask(UpdateTaskStatusDto updateTaskStatusDto);
        Task<bool> DeleteTaskAsync(int id);
        Task<bool> RemoveUserFromTaskAsync(RemoveUserFromTaskDto removeUserFromTaskDto);

        
    }
}
