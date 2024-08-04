using AutoMapper;
using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TMP.Application.Interfaces;
using TMPApplication.DTOs.ReminderDtos;
using TMPApplication.Interfaces.Reminders;
using TMPApplication.Notifications;
using TMPCommon.Constants;
using TMPDomain.Entities;
using TMPInfrastructure.Messaging;
using Task = System.Threading.Tasks.Task;
using Taski = TMPDomain.Entities.Task;

namespace TMPInfrastructure.Implementations.Reminders
{
    public class ReminderService : IReminderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ReminderService> _logger;
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccess;

        public ReminderService(IUnitOfWork unitOfWork,
            ILogger<ReminderService> logger,
            INotificationService notificationService,
            IMapper mapper,
            IHttpContextAccessor httpContextAccess)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _notificationService = notificationService;
            _mapper = mapper;
            _httpContextAccess = httpContextAccess;
        }

        #region Read
        public async Task<GetReminderDto> GetReminderAsync(int reminderId)
        {
            _logger.LogInformation("Fetching reminder with ID: {ReminderId}", reminderId);

            var reminder = await _unitOfWork.Repository<Reminder>().GetById(x => x.Id == reminderId).FirstOrDefaultAsync();
            if (reminder == null)
            {
                _logger.LogWarning("Reminder with ID: {ReminderId} not found", reminderId);
                throw new Exception("Reminder not found");
            }
            var mappedReminder = _mapper.Map<GetReminderDto>(reminder);
            return mappedReminder;
        }

        public async Task<List<GetReminderDto>> GetRemindersForTask(int taskId)
        {
            _logger.LogInformation("Fetching reminders for task with ID: {TaskId}", taskId);

            var reminderList = await _unitOfWork.Repository<Reminder>().GetByCondition(x => x.TaskId == taskId).ToListAsync();

            if (reminderList == null)
            {
                _logger.LogWarning("No reminders found for task with ID: {TaskId}", taskId);
                throw new Exception("There are no reminders for this task");
            }

            var mappedReminders = _mapper.Map<List<GetReminderDto>>(reminderList);
            return mappedReminders;
        }
        #endregion

        #region Create
        public async Task CreateReminderAsync(CreateReminderDto createReminderDto)
        {
            _logger.LogInformation("Creating reminder for task with ID: {TaskId}", createReminderDto.TaskId);

            if (createReminderDto.ReminderDate < DateTime.UtcNow)
            {
                _logger.LogWarning("Reminder date cannot be in the past");
                throw new ArgumentException("Reminder date cannot be in the past");
            }

            var createdByUserId = _httpContextAccess.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(createdByUserId))
            {
                _logger.LogWarning("User not authenticated");
                throw new UnauthorizedAccessException("User not authenticated");
            }

            var task = await _unitOfWork.Repository<Taski>().GetById(x => x.Id == createReminderDto.TaskId).FirstOrDefaultAsync();

            if (task == null)
            {
                _logger.LogWarning("Task with ID {TaskId} not found", createReminderDto.TaskId);
                throw new ArgumentException("Task not found");
            }

            var reminder = new Reminder
            {
                Description = createReminderDto.Description,
                ReminderDateTime = createReminderDto.ReminderDate,
                TaskId = createReminderDto.TaskId,
                Task = task,
                CreatedByUserId = createdByUserId
            };

            _unitOfWork.Repository<Reminder>().Create(reminder);
            _unitOfWork.Complete();

            _logger.LogInformation("Reminder created successfully for Task ID {TaskId} by user {UserId}", task.Id, createdByUserId);
            ScheduleReminder(reminder.Id, createReminderDto.ReminderDate);
        }

        private void ScheduleReminder(int reminderId, DateTime reminderDate)
        {
            _logger.LogInformation("Scheduling reminder with ID: {ReminderId} for date: {ReminderDate}", reminderId, reminderDate);

            // Schedule the job to run at the reminder date
            var jobId = BackgroundJob.Schedule(() => ProcessReminder(reminderId), reminderDate);

            var reminder = _unitOfWork.Repository<Reminder>().GetById(r => r.Id == reminderId).FirstOrDefault();
            if (reminder != null)
            {
                reminder.JobId = jobId;
                _unitOfWork.Repository<Reminder>().Update(reminder);
                _unitOfWork.Complete();
            }
            _logger.LogInformation("Reminder with ID: {ReminderId} successfully scheduled for: {ReminderDate}", reminderId, reminderDate);
        }

        public async Task ProcessReminder(int reminderId)
        {
            try
            {
                _logger.LogInformation("Processing reminder with ID: {ReminderId}", reminderId);

                var reminder = _unitOfWork.Repository<Reminder>().GetById(x => x.Id == reminderId).FirstOrDefault();
                if (reminder == null)
                {
                    _logger.LogWarning("Reminder with ID {ReminderId} not found", reminderId);
                    return;
                }
                var task = await _unitOfWork.Repository<Taski>()
                    .GetById(x => x.Id == reminder.TaskId)
                    .Include(x => x.AssignedUsers)
                    .FirstOrDefaultAsync();

                if (task == null)
                {
                    _logger.LogWarning("Task with ID {TaskId} for reminder {ReminderId} not found", reminder.TaskId, reminderId);
                    return;
                }

                var assignedUsers = task.AssignedUsers;
                var timeSpan = reminder.ReminderDateTime - task.DueDate;
                var message = $"Description: {reminder.Description} \n The task is due in: {timeSpan.Days} days, {timeSpan.Hours} hours, {timeSpan.Minutes} minutes";
                var subject = $"Reminder for Task: {reminder.Task.Title}";

                foreach (var user in assignedUsers)
                {
                    await _notificationService.CreateNotification(user.Id, reminder.TaskId, message, subject, NotificationType.Reminders);
                    _logger.LogInformation("Notification for reminder {ReminderId} created successfully", reminder.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing reminder with ID {ReminderId}", reminderId);
            }
        }
        #endregion

        #region Update
        public async Task<bool> UpdateReminder(int reminderId, ReminderDto reminderDto)
        {
            _logger.LogInformation("Updating reminder with ID: {ReminderId}", reminderId);

            if (reminderDto.ReminderDateTime < DateTime.UtcNow)
            {
                _logger.LogWarning("Reminder date cannot be in the past");
                throw new ArgumentException("Reminder date cannot be in the past");
            }

            var reminderToUpdate = await _unitOfWork.Repository<Reminder>()
                .GetByCondition(r => r.Id == reminderId)
                .FirstOrDefaultAsync();

            if (reminderToUpdate == null)
            {
                _logger.LogWarning("Reminder with ID: {ReminderId} not found", reminderId);
                return false;
            }

            if (!string.IsNullOrEmpty(reminderToUpdate.JobId))
            {
                BackgroundJob.Delete(reminderToUpdate.JobId);
                _logger.LogInformation("Existing Hangfire job for reminder {ReminderId} deleted", reminderId);
            }

            if (reminderToUpdate.ReminderDateTime != reminderDto.ReminderDateTime)
            {
                ScheduleReminder(reminderToUpdate.Id, reminderDto.ReminderDateTime);
                _logger.LogInformation("Reminder with ID: {ReminderId} successfully rescheduled", reminderId);
            }

            _mapper.Map(reminderDto, reminderToUpdate);

            _unitOfWork.Repository<Reminder>().Update(reminderToUpdate);
            _unitOfWork.Complete();

            _logger.LogInformation("Reminder with ID: {ReminderId} successfully updated", reminderId);
            return true;
        }
        #endregion

        #region Delete
        public async Task<bool> DeleteReminder(int reminderId)
        {
            _logger.LogInformation("Deleting reminder with ID: {ReminderId}", reminderId);

            var reminder = await _unitOfWork.Repository<Reminder>().GetById(x => x.Id == reminderId).FirstOrDefaultAsync();

            if (reminder == null)
            {
                _logger.LogWarning("Reminder with ID: {ReminderId} not found", reminderId);
                return false;
            }
            if (!string.IsNullOrEmpty(reminder.JobId))
            {
                BackgroundJob.Delete(reminder.JobId);
                _logger.LogInformation("Existing Hangfire job for reminder {ReminderId} deleted", reminderId);
            }

            _unitOfWork.Repository<Reminder>().Delete(reminder);
            _unitOfWork.Complete();

            _logger.LogInformation("Reminder with ID: {ReminderId} successfully deleted", reminderId);
            return true;
        }
        #endregion
    }
}
