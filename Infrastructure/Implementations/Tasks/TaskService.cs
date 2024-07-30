using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json;
using StackExchange.Redis;
using TMP.Application.DTOs.TaskDtos;
using TMP.Application.Interfaces;
using TMPApplication.Interfaces.Tasks;
using TMPApplication.Notifications;
using TMPDomain.Entities;
using Task = TMPDomain.Entities.Task;


namespace TMPInfrastructure.Implementations.Tasks
{
    public class TaskService : ITaskService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly StackExchange.Redis.IDatabase _cache;

        public TaskService(IUnitOfWork unitOfWork, IMapper mapper, INotificationService notificationService, IConnectionMultiplexer redis)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notificationService = notificationService;
            _cache = redis.GetDatabase();
        }

        public async Task<IEnumerable<TaskDto>> GetAllTasksAsync()
        {
            var cacheKey = "all_tasks";
            var cachedTasks = await _cache.StringGetAsync(cacheKey);

            if (cachedTasks.HasValue)
            {
                // Why newton json vs text.json
                var task = JsonConvert.DeserializeObject<IEnumerable<TaskDto>>(cachedTasks);
                return task;
            }

            var tasks = await _unitOfWork.Repository<Task>().GetAll()
                .Include(t => t.Project)
                .ToListAsync();

            var taskDtos = _mapper.Map<IEnumerable<TaskDto>>(tasks);

           
            await _cache.StringSetAsync(cacheKey, JsonConvert.SerializeObject(taskDtos), TimeSpan.FromMinutes(10));

            return taskDtos;
        }

        public async Task<IEnumerable<TaskDto>> GetTasksByProjectIdAsync(int projectId)
        {
            var tasks = await _unitOfWork.Repository<Task>().GetByCondition(t => t.ProjectId == projectId)
                .Include(t => t.Project)
                .ToListAsync();

            return _mapper.Map<IEnumerable<TaskDto>>(tasks);
        }

        public async Task<TaskDto> GetTaskByIdAsync(int id)
        {
            var task = await _unitOfWork.Repository<Task>().GetById(t => t.Id == id)
                .Include(t => t.Project)
                .FirstOrDefaultAsync();

            if (task == null) return null;

            return _mapper.Map<TaskDto>(task);
        }

        public async Task<IEnumerable<TaskDto>> GetTasksAsync(int? projectId)
        {
            IQueryable<Task> query = _unitOfWork.Repository<Task>().GetAll();

            if (projectId.HasValue)
            {
                query = query.Where(t => t.ProjectId == projectId.Value);
            }

            var tasks = await query.Include(t => t.Project).ToListAsync();
            return _mapper.Map<IEnumerable<TaskDto>>(tasks);
        }

        public async Task<IEnumerable<UserDetailsDto>> GetAssignedUsersAsync(int taskId)
        {
            var task = await _unitOfWork.Repository<Task>()
                .GetById(t => t.Id == taskId)
                .Include(t => t.AssignedUsers)
                .FirstOrDefaultAsync();

            if (task == null)
                return null;

            return task.AssignedUsers.Select(u => new UserDetailsDto
            {
                FirstName = u.FirstName,
                LastName = u.LastName
            }).ToList();
        }

        public async Task<IEnumerable<TaskDto>> GetTasksByUserIdAsync(string userId)
        {
            var tasks = await _unitOfWork.Repository<Task>()
                .GetByCondition(t => t.AssignedUsers.Any(u => u.Id == userId))
                .Include(t => t.Project)
                .ToListAsync();

            return _mapper.Map<IEnumerable<TaskDto>>(tasks);
        }

        public async Task<TaskDto> AddTaskAsync(AddTaskDto newTask)
        {
            var task = _mapper.Map<Task>(newTask);

            _unitOfWork.Repository<Task>().Create(task);
            await _unitOfWork.Repository<Task>().SaveChangesAsync();
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
            _unitOfWork.Repository<Task>().Update(task);
            await _unitOfWork.Repository<Task>().SaveChangesAsync();

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
            return _unitOfWork.Complete();
        }

        public async Task<bool> DeleteTaskAsync(int id)
        {
            var task = await _unitOfWork.Repository<Task>().GetById(t => t.Id == id).FirstOrDefaultAsync();
            if (task == null) return false;

            _unitOfWork.Repository<Task>().Delete(task);
            await _unitOfWork.Repository<Task>().SaveChangesAsync();
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

            return result;
        }
    }
}
