using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json;
using StackExchange.Redis;
using TMP.Application.DTOs.TaskDtos;
using TMP.Application.Interfaces;
using TMPApplication.Interfaces;
using TMPApplication.Interfaces.Tasks;
using TMPApplication.Notifications;
using TMPCommon.Constants;
using TMPDomain.Entities;
using Task = TMPDomain.Entities.Task;


namespace TMPInfrastructure.Implementations.Tasks
{
    public class TaskService : ITaskService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly ICacheService _cache;

        public TaskService(IUnitOfWork unitOfWork, IMapper mapper, INotificationService notificationService, ICacheService cache)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notificationService = notificationService;
            _cache = cache;
        }

        public async Task<IEnumerable<TaskDto>> GetAllTasksAsync()
        {
            var cacheKey = TasksConstants.AllTasks;
            var cachedTasks = await _cache.GetAsync<IEnumerable<TaskDto>>(cacheKey);
            if (cachedTasks != null)
                return cachedTasks;

            var tasks = await _unitOfWork.Repository<Task>().GetAll()
                .Include(t => t.Project)
                .ToListAsync();

            var taskDtos = _mapper.Map<IEnumerable<TaskDto>>(tasks);

            await _cache.SetAsync(cacheKey, taskDtos, TimeSpan.FromMinutes(60));

            return taskDtos;
        }

        public async Task<IEnumerable<TaskDto>> GetTasksByProjectIdAsync(int projectId)
        {
            var chacheKey = string.Format(TasksConstants.TasksByProject, projectId);
            var cachedTasks = await _cache.GetAsync<IEnumerable<TaskDto>>(chacheKey);
            if (cachedTasks != null)
                return cachedTasks;

            var tasks = await _unitOfWork.Repository<Task>().GetByCondition(t => t.ProjectId == projectId)
                .Include(t => t.Project)
                .ToListAsync();

            var tasksDto =  _mapper.Map<IEnumerable<TaskDto>>(tasks);

            await _cache.SetAsync(chacheKey, tasksDto);

            return tasksDto;
        }

        public async Task<TaskDto> GetTaskByIdAsync(int id)
        {
            var cacheKey = string.Format(TasksConstants.TaskById, id);
            var cachedTask = await _cache.GetAsync<TaskDto>(cacheKey);

            if(cachedTask != null) { return cachedTask;}

            var task = await _unitOfWork.Repository<Task>().GetById(t => t.Id == id)
                .Include(t => t.Project)
                .FirstOrDefaultAsync();

            if (task == null) return null;

            var taskDto = _mapper.Map<TaskDto>(task);

            await _cache.SetAsync(cacheKey, taskDto);

            return taskDto;
        }

        public async Task<IEnumerable<TaskDto>> GetTasksAsync(int? projectId) //CHECK
        {
            var cacheKey = string.Format(TasksConstants.TasksByProject, projectId);
            var cachedTask = await _cache.GetAsync<IEnumerable<TaskDto>>(cacheKey);
            if (cachedTask != null)
                return cachedTask;

            IQueryable<Task> query = _unitOfWork.Repository<Task>().GetAll();

            if (projectId.HasValue)
            {
                query = query.Where(t => t.ProjectId == projectId.Value);
            }

            var tasks = await query.Include(t => t.Project).ToListAsync();
            var tasksDto = _mapper.Map<IEnumerable<TaskDto>>(tasks);

            await _cache.SetAsync(cacheKey, tasksDto);

            return tasksDto;
        }

        public async Task<IEnumerable<UserDetailsDto>> GetAssignedUsersAsync(int taskId)
        {
            var cacheKey = string.Format(TasksConstants.UsersByTask,taskId);
            var cachedUsers = await _cache.GetAsync<IEnumerable<UserDetailsDto>>(cacheKey);
            if (cachedUsers != null)
                return cachedUsers;

            var task = await _unitOfWork.Repository<Task>()
                .GetById(t => t.Id == taskId)
                .Include(t => t.AssignedUsers)
                .FirstOrDefaultAsync();

            if (task == null)
                return null;

            var usersDto =  task.AssignedUsers.Select(u => new UserDetailsDto
            {
                FirstName = u.FirstName,
                LastName = u.LastName
            }).ToList();

            await _cache.SetAsync(cacheKey, usersDto);

            return usersDto;
        }

        public async Task<IEnumerable<TaskDto>> GetTasksByUserIdAsync(string userId)
        {
            var cacheKey = string.Format(TasksConstants.TasksByUser);
            var cachedTasks = await _cache.GetAsync<IEnumerable<TaskDto>>(cacheKey);

            if (cachedTasks != null)
                return cachedTasks;

            var tasks = await _unitOfWork.Repository<Task>()
                .GetByCondition(t => t.AssignedUsers.Any(u => u.Id == userId))
                .Include(t => t.Project)
                .ToListAsync();

            var tasksDto = _mapper.Map<IEnumerable<TaskDto>>(tasks);
            await _cache.SetAsync(cacheKey, tasksDto);

            return tasksDto;
        }

        public async Task<TaskDto> AddTaskAsync(AddTaskDto newTask)
        {
            var task = _mapper.Map<Task>(newTask);

            _unitOfWork.Repository<Task>().Create(task);
            await _unitOfWork.Repository<Task>().SaveChangesAsync();

            await _cache.DeleteKeyAsync(string.Format(TasksConstants.TaskById, task.Id));
            await _cache.DeleteKeyAsync(string.Format(TasksConstants.TasksByProject, task.ProjectId));
            await _cache.DeleteKeyAsync(string.Format(TasksConstants.AllTasks));

            return _mapper.Map<TaskDto>(task);
        }

        public async Task<bool> AssignUserToTaskAsync(AssignUserToTaskDto assignUserToTaskDto)
        {
            var task = await _unitOfWork.Repository<Task>().GetById(t => t.Id == assignUserToTaskDto.TaskId)
                .Include(t => t.AssignedUsers)
                .FirstOrDefaultAsync();

            if (task == null) return false;

            var user = await _unitOfWork.Repository<User>().GetById(u => u.Id == assignUserToTaskDto.UserId).FirstOrDefaultAsync();
            if (user == null) return false;

            task.AssignedUsers.Add(user);

            var message = "";
            var subject = "Task Assignment";
            await _notificationService.CreateNotification(user.Id,task.Id,message,subject,NotificationType.TaskNotifications);

            _unitOfWork.Repository<Task>().Update(task);
            await _unitOfWork.Repository<Task>().SaveChangesAsync();

            await _cache.DeleteKeyAsync(string.Format(TasksConstants.UsersByTask,task.Id));
            await _cache.DeleteKeyAsync(string.Format(TasksConstants.TasksByUser,user.Id)); //Because a user gets a new task 
            
            return true;
        }

        public async Task<bool> UpdateTaskAsync(int id, AddTaskDto updatedTask)
        {
            var task = await _unitOfWork.Repository<Task>().GetById(t => t.Id == id).FirstOrDefaultAsync();
            if (task == null) return false;

            _mapper.Map(updatedTask, task);
            task.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<Task>().Update(task);
            await _unitOfWork.Repository<Task>().SaveChangesAsync();

            await _cache.DeleteKeyAsync(string.Format(TasksConstants.TaskById,task.Id));
            await _cache.DeleteKeyAsync(string.Format(TasksConstants.TasksByProject,task.ProjectId));
            await _cache.DeleteKeyAsync(string.Format(TasksConstants.AllTasks));

            return true;
        }

        public async Task<bool> UpdateStatusOfTask(UpdateTaskStatusDto updateTaskStatusDto)
        {
            var task = await _unitOfWork.Repository<Task>()
                .GetById(x => x.Id == updateTaskStatusDto.TaskId)
                .FirstOrDefaultAsync();

            if (task == null) return false;

            task.Status = updateTaskStatusDto.Status;
            _unitOfWork.Repository<Task>().Update(task);

            await _cache.DeleteKeyAsync(string.Format(TasksConstants.TaskById, task.Id));
            await _cache.DeleteKeyAsync(string.Format(TasksConstants.TasksByProject, task.ProjectId));
            await _cache.DeleteKeyAsync(string.Format(TasksConstants.AllTasks)); //Check -- maybe remove these one, cause it will be needed only to count the nr tasks

            return _unitOfWork.Complete();
        }

        public async Task<bool> DeleteTaskAsync(int id)
        {
            var task = await _unitOfWork.Repository<Task>().GetById(t => t.Id == id).FirstOrDefaultAsync();
            if (task == null) return false;

            _unitOfWork.Repository<Task>().Delete(task);
            await _unitOfWork.Repository<Task>().SaveChangesAsync();

            await _cache.DeleteKeyAsync(string.Format(TasksConstants.TaskById, task.Id));
            await _cache.DeleteKeyAsync(string.Format(TasksConstants.TasksByProject, task.ProjectId));
            await _cache.DeleteKeyAsync(string.Format(TasksConstants.AllTasks));
            await _cache.DeleteKeyAsync(string.Format(TasksConstants.UsersByTask, task.Id));
            foreach (var user in task.AssignedUsers)
            {
                await _cache.DeleteKeyAsync(string.Format(TasksConstants.TasksByUser, user.Id));
            }

            return true;
        }

        public async Task<bool> RemoveUserFromTaskAsync(RemoveUserFromTaskDto removeUserFromTaskDto)
        {
            var task = await _unitOfWork.Repository<Task>()
                .GetById(t => t.Id == removeUserFromTaskDto.TaskId)
                .Include(t => t.AssignedUsers)
                .FirstOrDefaultAsync();

            if (task == null) return false;

            var user = await _unitOfWork.Repository<User>()
                .GetById(u => u.Id == removeUserFromTaskDto.UserId)
                .FirstOrDefaultAsync();

            if (user == null) return false;


            if (!task.AssignedUsers.Any(u => u.Id == user.Id))
            {
                return false;
            }

            task.AssignedUsers.Remove(user);

            _unitOfWork.Repository<Task>().Update(task);
            var result = _unitOfWork.Complete();

            await _cache.DeleteKeyAsync(string.Format(TasksConstants.UsersByTask, task.Id));
            foreach (var theUser in task.AssignedUsers)
            {
                await _cache.DeleteKeyAsync(string.Format(TasksConstants.TasksByUser, theUser.Id));
            }

            return result;
        }
    }
}
