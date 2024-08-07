﻿using AutoMapper;
using FluentValidation;
using Google.Apis.Calendar.v3.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TMP.Application.DTOs.CommentDtos;
using TMP.Application.DTOs.SubtaskDtos;
using TMP.Application.DTOs.TaskDtos;
using TMP.Application.Interfaces;
using TMPApplication.Hubs;
using TMPApplication.Interfaces;
using TMPApplication.Interfaces.Tasks;
using TMPApplication.Notifications;
using TMPCommon.Constants;
using TMPDomain.Entities;
using TMPDomain.Enumerations;
using TMPDomain.Validations;
using TMPInfrastructure.Implementations.CalendarApi;
using Task = TMPDomain.Entities.Task;

namespace TMPInfrastructure.Implementations.Tasks
{
    public class TaskService : ITaskService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly IHubContext<NotificationHub> _notificationHub;
        private readonly ICacheService _cache;
        private readonly ISearchService<TaskDto> _searchService;
        private readonly ILogger<TaskService> _logger;
        //private readonly IGoogleCalendarService _googleCalendarService;
        private readonly IValidator<Task> _taskValidator;
        private readonly IValidator<TaskDuration> _taskDurationValidator;

        public TaskService(IUnitOfWork unitOfWork,
            IMapper mapper,
            INotificationService notificationService,
            IHubContext<NotificationHub> notificationHub,
            ICacheService cache,
            ISearchService<TaskDto> searchService,
            ILogger<TaskService> logger,
            IValidator<TaskDuration> taskDurationValidator,
            IValidator<Task> taskValidator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notificationService = notificationService;
            _notificationHub = notificationHub;
            _cache = cache;
            _searchService = searchService;
            _logger = logger;
            _taskDurationValidator = taskDurationValidator;
            _taskValidator = taskValidator;
            //_googleCalendarService = googleCalendarService;
        }

        #region Read
        public async Task<TaskDto> GetTaskByIdAsync(int id)
        {
            _logger.LogInformation("Fetching task with ID: {TaskId}", id);

            var cacheKey = string.Format(TasksConstants.TaskById, id);
            var cachedTask = await _cache.GetAsync<TaskDto>(cacheKey);

            if (cachedTask != null)
            {
                _logger.LogInformation("Task with ID: {TaskId} found in cache", id);
                return cachedTask;
            }

            var task = await _unitOfWork.Repository<Task>().GetById(t => t.Id == id)
                .Include(t => t.Project)
                .Include(t => t.Tags)
                .FirstOrDefaultAsync();

            if (task == null)
            {
                _logger.LogWarning("Task with ID: {TaskId} not found", id);
                return null;
            }

            var taskDto = _mapper.Map<TaskDto>(task);
            await _cache.SetAsync(cacheKey, taskDto);

            _logger.LogInformation("Task with ID: {TaskId} fetched successfully", id);
            return taskDto;
        }

        public async Task<IEnumerable<TaskDto>> GetTasksAsync()
        {
            _logger.LogInformation("Fetching tasks");

            var cacheKey = TasksConstants.AllTasks;
            var cachedTasks = await _cache.GetAsync<IEnumerable<TaskDto>>(cacheKey);
            if (cachedTasks != null)
            {
                _logger.LogInformation("All tasks found in cache");
                return cachedTasks;
            }

            var tasks = await _unitOfWork.Repository<Task>().GetAll()
                                    .Include(t => t.Project)
                                    .Include(t => t.Tags)
                                    .ToListAsync();
            var taskDtos = _mapper.Map<IEnumerable<TaskDto>>(tasks);

            await _cache.SetAsync(cacheKey, taskDtos, TimeSpan.FromMinutes(60));

            _logger.LogInformation("Tasks fetched successfully");
            return taskDtos;
        }

        public async Task<IEnumerable<UserDetailsDto>> GetAssignedUsersAsync(int taskId)
        {
            _logger.LogInformation("Fetching assigned users for task with ID: {TaskId}", taskId);

            var cacheKey = string.Format(TasksConstants.UsersByTask, taskId);
            var cachedUsers = await _cache.GetAsync<IEnumerable<UserDetailsDto>>(cacheKey);
            if (cachedUsers != null)
            {
                _logger.LogInformation("Assigned users for task with ID: {TaskId} found in cache", taskId);
                return cachedUsers;
            }

            var task = await _unitOfWork.Repository<Task>()
                .GetById(t => t.Id == taskId)
                .Include(t => t.AssignedUsers)
                .FirstOrDefaultAsync();

            if (task == null)
            {
                _logger.LogWarning("Task with ID: {TaskId} not found", taskId);
                return null;
            }

            var usersDto = task.AssignedUsers.Select(u => new UserDetailsDto
            {
                UserId = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                ProfilePicture = u.ProfilePicture
            }).ToList();

            await _cache.SetAsync(cacheKey, usersDto);

            _logger.LogInformation("Assigned users for task with ID: {TaskId} fetched successfully", taskId);
            return usersDto;
        }

        public async Task<IEnumerable<TaskDto>> GetTasksByUserIdAsync(string userId)
        {
            _logger.LogInformation("Fetching tasks for user with ID: {UserId}", userId);

            var cacheKey = string.Format(TasksConstants.TasksByUser, userId);
            var cachedTasks = await _cache.GetAsync<IEnumerable<TaskDto>>(cacheKey);

            if (cachedTasks != null)
            {
                _logger.LogInformation("Tasks for user with ID: {UserId} found in cache", userId);
                return cachedTasks;
            }

            var tasks = await _unitOfWork.Repository<Task>()
                .GetByCondition(t => t.AssignedUsers.Any(u => u.Id == userId))
                .Include(t => t.Project)
                .Include(t => t.Tags)
                .ToListAsync();

            var tasksDto = _mapper.Map<IEnumerable<TaskDto>>(tasks);
            await _cache.SetAsync(cacheKey, tasksDto);

            _logger.LogInformation("Tasks for user with ID: {UserId} fetched successfully", userId);
            return tasksDto;
        }

        public async Task<IEnumerable<CommentDto>> GetCommentsByTaskIdAsync(int taskId)
        {
            var cacheKey = $"task_{taskId}_comments";
            var cachedComments = await _cache.GetAsync<IEnumerable<CommentDto>>(cacheKey);

            if (cachedComments != null)
            {
                return cachedComments;
            }
            _logger.LogInformation("Fetching comments for task with ID: {TaskId}", taskId);

            var comments = await _unitOfWork.Repository<Comment>()
                .GetByCondition(c => c.TaskId == taskId)
                .ToListAsync();

            var mappedComments = _mapper.Map<IEnumerable<CommentDto>>(comments);

            await _cache.SetAsync(cacheKey, mappedComments);
            return mappedComments;
        }

        public async Task<IEnumerable<SubtaskDto>> GetSubtasksByTaskIdAsync(int taskId)
        {
            var cacheKey = $"task_{taskId}_subtasks";
            var cachedSubtasks = await _cache.GetAsync<IEnumerable<SubtaskDto>>(cacheKey);

            _logger.LogInformation("Fetching subtasks for task with ID: {TaskId}", taskId);

            var subtasks = await _unitOfWork.Repository<Subtask>()
                .GetByCondition(st => st.TaskId == taskId)
                .ToListAsync();

            var mappedSubtasks = _mapper.Map<IEnumerable<SubtaskDto>>(subtasks);
            await _cache.SetAsync(cacheKey, mappedSubtasks);

            return mappedSubtasks;
        }

        public async Task<TimeSpan?> GetTaskDurationAsync(int taskId)
        {
            _logger.LogInformation("Fetching task duration for task with ID: {TaskId}", taskId);

            var task = await _unitOfWork.Repository<Task>()
                .GetById(x => x.Id == taskId)
                .Include(t => t.TaskDurations)
                .FirstOrDefaultAsync();

            if (task == null || task.TaskDurations == null || !task.TaskDurations.Any())
            {
                _logger.LogWarning("Task with ID: {TaskId} not found or has no task durations", taskId);
                return null;
            }

            var totalDuration = task.TaskDurations
                .Where(td => td.EndTime != default) // Only consider durations with an end time set
                .Sum(td => td.Duration.TotalSeconds); // Summing durations in seconds for precision

            var timeSpan = TimeSpan.FromSeconds(totalDuration);

            _logger.LogInformation("Task duration for task with ID: {TaskId} fetched successfully", taskId);
            return timeSpan;
        }
        #endregion

        #region Create
        public async Task<TaskDto> AddTaskAsync(AddTaskDto newTask)
        {
            _logger.LogInformation("Adding new task");

            var task = _mapper.Map<Task>(newTask);
            task.ProjectId = newTask.ProjectId;

            var validationResult = await _taskValidator.ValidateAsync(task);
            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogWarning("Task validation failed: {Errors}", errors);
                throw new ValidationException(errors);
            }

            if (newTask.Tags != null && newTask.Tags.Any())
            {
                task.Tags = new List<Tag>();
                foreach (var tagName in newTask.Tags)
                {
                    var existingTag = await _unitOfWork.Repository<Tag>().GetByCondition(t => t.Name == tagName).FirstOrDefaultAsync();
                    if (existingTag != null)
                    {
                        task.Tags.Add(existingTag);
                    }
                    else
                    {
                        var newTag = new Tag { Name = tagName };
                        _unitOfWork.Repository<Tag>().Create(newTag);
                        await _unitOfWork.Repository<Tag>().SaveChangesAsync();

                        task.Tags.Add(newTag);
                    }
                }
            }

            _unitOfWork.Repository<Task>().Create(task);
            await _unitOfWork.Repository<Task>().SaveChangesAsync();

            var taskDto = _mapper.Map<TaskDto>(task);
            await _searchService.IndexDocumentAsync(taskDto, "tasks");

            await _cache.DeleteKeyAsync(string.Format(TasksConstants.TaskById, task.Id));
            await _cache.DeleteKeyAsync(string.Format(TasksConstants.AllTasks));

            _logger.LogInformation("Task added successfully with ID: {TaskId}", task.Id);
            return taskDto;
        }

        public async Task<bool> AssignUserToTaskAsync(AssignUserToTaskDto assignUserToTaskDto)
        {
            _logger.LogInformation("Assigning user with ID: {UserId} to task with ID: {TaskId}", assignUserToTaskDto.UserId, assignUserToTaskDto.TaskId);

            var task = await _unitOfWork.Repository<Task>().GetById(t => t.Id == assignUserToTaskDto.TaskId)
                .Include(t => t.AssignedUsers)
                .FirstOrDefaultAsync();

            if (task == null)
            {
                _logger.LogWarning("Task with ID: {TaskId} not found", assignUserToTaskDto.TaskId);
                return false;
            }

            var user = await _unitOfWork.Repository<User>().GetById(u => u.Id == assignUserToTaskDto.UserId).FirstOrDefaultAsync();
            if (user == null)
            {
                _logger.LogWarning("User with ID: {UserId} not found", assignUserToTaskDto.UserId);
                return false;
            }

            task.AssignedUsers.Add(user);

            var message = $"You have been assigned the Task: {task.Title}";
            var subject = "Task Assignment";
            await _notificationService.CreateNotification(user.Id, task.Id, message, subject, NotificationType.TaskNotifications);

            _unitOfWork.Repository<Task>().Update(task);
            await _unitOfWork.Repository<Task>().SaveChangesAsync();

            await _cache.DeleteKeyAsync(string.Format(TasksConstants.UsersByTask, task.Id));
            await _cache.DeleteKeyAsync(string.Format(TasksConstants.TasksByUser, user.Id));

            _logger.LogInformation("User with ID: {UserId} assigned to task with ID: {TaskId} successfully", assignUserToTaskDto.UserId, assignUserToTaskDto.TaskId);
            return true;
        }
        #endregion

        #region Update
        public async Task<bool> UpdateTaskAsync(int id, UpdateTaskDto updatedTask)
        {
            _logger.LogInformation("Updating task with ID: {TaskId}", id);

            var task = await _unitOfWork.Repository<Task>().GetById(t => t.Id == id)
                        .Include(t => t.Tags)
                        .FirstOrDefaultAsync();
            if (task == null)
            {
                _logger.LogWarning("Task with ID: {TaskId} not found", id);
                return false;
            }

            _mapper.Map(updatedTask, task);
            task.UpdatedAt = DateTime.UtcNow;

            var validationResult = await _taskValidator.ValidateAsync(task);
            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogWarning("Task validation failed: {Errors}", errors);
                throw new ValidationException(errors);
            }

            // Handle tags
            var existingTags = task.Tags.Select(t => t.Name).ToList();
            var updatedTags = updatedTask.Tags ?? new List<string>();

            // Remove tags that are no longer in the updated list
            var tagsToRemove = existingTags.Except(updatedTags).ToList();
            foreach (var tagToRemove in tagsToRemove)
            {
                var tagEntity = task.Tags.FirstOrDefault(t => t.Name == tagToRemove);
                if (tagEntity != null)
                {
                    task.Tags.Remove(tagEntity);
                }
            }

            // Add new tags that are not already in the existing list
            var tagsToAdd = updatedTags.Except(existingTags).ToList();
            foreach (var tagToAdd in tagsToAdd)
            {
                var tagEntity = await _unitOfWork.Repository<Tag>().GetById(t => t.Name == tagToAdd).FirstOrDefaultAsync();
                if (tagEntity == null)
                {
                    tagEntity = new Tag { Name = tagToAdd };
                    _unitOfWork.Repository<Tag>().Create(tagEntity);
                }
                task.Tags.Add(tagEntity);
            }

            _unitOfWork.Repository<Task>().Update(task);
            await _unitOfWork.Repository<Task>().SaveChangesAsync();
            await _searchService.IndexDocumentAsync(_mapper.Map<TaskDto>(task), "tasks");

            await _cache.DeleteKeyAsync(string.Format(TasksConstants.TaskById, task.Id));
            await _cache.DeleteKeyAsync(string.Format(TasksConstants.AllTasks));

            _logger.LogInformation("Task with ID: {TaskId} updated successfully", id);
            return true;
        }

        public async Task<bool> UpdateStatusOfTask(UpdateTaskStatusDto updateTaskStatusDto)
        {
            _logger.LogInformation("Updating status of task with ID: {TaskId} to {Status}", updateTaskStatusDto.TaskId, updateTaskStatusDto.Status);

            var task = await _unitOfWork.Repository<Task>()
                .GetById(x => x.Id == updateTaskStatusDto.TaskId)
                .Include(t => t.TaskDurations)
                .FirstOrDefaultAsync();

            if (task == null)
            {
                _logger.LogWarning("Task with ID: {TaskId} not found", updateTaskStatusDto.TaskId);
                return false;
            }

            if (updateTaskStatusDto.Status == StatusOfTask.InProgress)
            {
                var taskDuration = new TaskDuration
                {
                    TaskId = task.Id,
                    UserId = updateTaskStatusDto.UserId,
                    StartTime = DateTime.UtcNow
                };

                var durationValidationResult = await _taskDurationValidator.ValidateAsync(taskDuration);
                if (!durationValidationResult.IsValid)
                {
                    var errors = string.Join("; ", durationValidationResult.Errors.Select(e => e.ErrorMessage));
                    _logger.LogWarning("Task duration validation failed: {Errors}", errors);
                    throw new ValidationException(errors);
                }

                _unitOfWork.Repository<TaskDuration>().Create(taskDuration);
            }
            else if (updateTaskStatusDto.Status == StatusOfTask.Done)
            {
                var taskDuration = task.TaskDurations
                    .Where(td => td.EndTime == DateTime.MinValue && td.UserId == updateTaskStatusDto.UserId)
                    .OrderByDescending(td => td.StartTime)
                    .FirstOrDefault();

                if (taskDuration != null)
                {
                    taskDuration.EndTime = DateTime.UtcNow;

                    var durationValidationResult = await _taskDurationValidator.ValidateAsync(taskDuration);
                    if (!durationValidationResult.IsValid)
                    {
                        var errors = string.Join("; ", durationValidationResult.Errors.Select(e => e.ErrorMessage));
                        _logger.LogWarning("Task duration validation failed: {Errors}", errors);
                        throw new ValidationException(errors);
                    }

                    _unitOfWork.Repository<TaskDuration>().Update(taskDuration);
                }
            }

            var oldStatus = task.Status.ToString();
            task.Status = updateTaskStatusDto.Status;
            _unitOfWork.Repository<Task>().Update(task);

            var message = $"The status of {task.Title} has been changed from {oldStatus} to {task.Status.ToString()}";

            if (task.AssignedUsers != null)
            {
                foreach (var user in task.AssignedUsers)
                {
                    await _notificationHub.Clients.Group(user.Id).SendAsync("ReceiveNotifications", message);
                }
            }

            await _cache.DeleteKeyAsync(string.Format(TasksConstants.TaskById, task.Id));
            await _cache.DeleteKeyAsync(string.Format(TasksConstants.AllTasks));

            bool result = _unitOfWork.Complete();
            if (!result)
            {
                _logger.LogWarning("Failed to update the status of task with ID: {TaskId}", updateTaskStatusDto.TaskId);
                return false;
            }

            _logger.LogInformation("Status of task with ID: {TaskId} successfully updated to {Status}", updateTaskStatusDto.TaskId, updateTaskStatusDto.Status);
            return true;
        }
        #endregion

        #region Delete
        public async Task<bool> DeleteTaskAsync(int id)
        {
            _logger.LogInformation("Deleting task with ID: {TaskId}", id);

            var task = await _unitOfWork.Repository<Task>().GetById(t => t.Id == id)
                                                           .Include(x => x.AssignedUsers)
                                                           .FirstOrDefaultAsync();

            if (task == null)
            {
                _logger.LogWarning("Task with ID: {TaskId} not found", id);
                return false;
            }

            _unitOfWork.Repository<Task>().Delete(task);
            await _unitOfWork.Repository<Task>().SaveChangesAsync();

            await _searchService.DeleteDocumentAsync(id.ToString(), "tasks");

            // Remove related cache entries
            await _cache.DeleteKeyAsync(string.Format(TasksConstants.TaskById, task.Id));
            await _cache.DeleteKeyAsync(string.Format(TasksConstants.AllTasks));
            await _cache.DeleteKeyAsync(string.Format(TasksConstants.UsersByTask, task.Id));

            if (task.AssignedUsers != null)
            {
                foreach (var user in task.AssignedUsers)
                {
                    await _cache.DeleteKeyAsync(string.Format(TasksConstants.TasksByUser, user.Id));
                }
            }

            _logger.LogInformation("Task with ID: {TaskId} deleted successfully", id);
            return true;
        }

        public async Task<bool> RemoveUserFromTaskAsync(RemoveUserFromTaskDto removeUserFromTaskDto)
        {
            _logger.LogInformation("Removing user with ID: {UserId} from task with ID: {TaskId}", removeUserFromTaskDto.UserId, removeUserFromTaskDto.TaskId);

            var task = await _unitOfWork.Repository<Task>()
                .GetById(t => t.Id == removeUserFromTaskDto.TaskId)
                .Include(t => t.AssignedUsers)
                .FirstOrDefaultAsync();

            if (task == null)
            {
                _logger.LogWarning("Task with ID: {TaskId} not found", removeUserFromTaskDto.TaskId);
                return false;
            }

            var user = await _unitOfWork.Repository<User>()
                .GetById(u => u.Id == removeUserFromTaskDto.UserId)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                _logger.LogWarning("User with ID: {UserId} not found", removeUserFromTaskDto.UserId);
                return false;
            }

            if (!task.AssignedUsers.Any(u => u.Id == user.Id))
            {
                _logger.LogWarning("User with ID: {UserId} is not assigned to task with ID: {TaskId}", user.Id, task.Id);
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

            _logger.LogInformation("User with ID: {UserId} removed from task with ID: {TaskId} successfully", user.Id, task.Id);
            return result;
        }
        #endregion

        #region CalendarAPI

        /*        public async Task<Event> AddTaskAsEventInCalendar(int taskId)
                {
                    var taskToAdd = await _unitOfWork.Repository<Task>().GetById(x => x.Id == taskId).Include(x => x.Project).FirstOrDefaultAsync();
                    if (taskToAdd == null)
                        throw new Exception("Task not found");

                    if (taskToAdd.Project == null || string.IsNullOrEmpty(taskToAdd.Project.CalendarId))
                    {
                        throw new Exception($"The project with ID {taskToAdd.ProjectId} does not have a calendar.");
                    }

                    var calendarEvent = new Event
                    {
                        Summary = taskToAdd.Title,
                        Description = taskToAdd.Description,
                        Start = new EventDateTime
                        {
                            DateTime = taskToAdd.DueDate,
                            TimeZone = "Europe/Tirane"
                        },
                        End = new EventDateTime
                        {
                            DateTimeDateTimeOffset = taskToAdd.DueDate,
                            TimeZone = "Europe/Tirane"
                        }
                    };

                    try
                    {
                        // Add the event to the specified calendar
                        var createdEvent = await _googleCalendarService.CreateEventAsync(taskToAdd.Project.CalendarId, calendarEvent);
                        return createdEvent;
                    }
                    catch (Exception ex)
                    {
                        throw new ApplicationException($"Error adding task {taskId} as event in Google Calendar.", ex);
                    }
                }*/
        #endregion
    }
}