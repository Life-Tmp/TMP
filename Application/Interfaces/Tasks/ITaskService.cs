using TMP.Application.DTOs.TaskDtos;

namespace TMPApplication.Interfaces.Tasks
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
        Task<bool> AssignUserToTaskAsync(AssignUserToTaskDto assignUserToTaskDto);
        Task<bool> RemoveUserFromTaskAsync(RemoveUserFromTaskDto removeUserFromTaskDto);
        Task<bool> UpdateStatusOfTask(UpdateTaskStatusDto updateTaskStatusDto);
        Task<IEnumerable<TaskDto>> GetTasksByUserIdAsync(string userId);


    }
}
