using AutoMapper;
using Hangfire;
using Mailjet.Client.Resources;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TMP.Application.Interfaces;
using TMPApplication.DTOs.ReminderDtos;
using TMPApplication.Interfaces.Reminders;
using TMPApplication.Notifications;
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


        public async Task<GetReminderDto> GetReminderAsync(int reminderId)
        {

            var reminder = await _unitOfWork.Repository<Reminder>().GetById(x => x.Id == reminderId).FirstOrDefaultAsync();
            if(reminder == null)
            {
                throw new Exception("Reminder not found");  //CustomEXception

            }
            var mappedReminder = _mapper.Map<GetReminderDto>(reminder);
            return mappedReminder;
        }

        public async Task<List<GetReminderDto>> GetRemindersForTask(int taskId)
        {
            var reminderList = await _unitOfWork.Repository<Reminder>().GetByCondition(x => x.TaskId == taskId).ToListAsync();
            
            if(reminderList == null)
            {
                throw new Exception("There is no reminders for this task");
            }

            var mappedReminders = _mapper.Map<List<GetReminderDto>>(reminderList);

            return mappedReminders;

        }

        public async Task CreateReminderAsync(string description, DateTime reminderDate, int taskId)
        {
            if (reminderDate < DateTime.UtcNow)
            {
                _logger.LogWarning("Reminder date cannot be in the past");
                throw new ArgumentException("Reminder date cannot be in the past");
            }
            var createdByUserId = _httpContextAccess.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;

            var task = await _unitOfWork.Repository<Taski>().GetById(x => x.Id == taskId).FirstOrDefaultAsync();

            if (task == null)
            {
                _logger.LogWarning($"Task with ID {taskId} not found");
                throw new ArgumentException("Task not found");
            }
            var reminder = new Reminder
            {

                Description = description,
                ReminderDateTime = reminderDate,
                TaskId = taskId,
                Task = task,
                CreatedByUserId = createdByUserId
            };

            _unitOfWork.Repository<Reminder>().Create(reminder);

            _unitOfWork.Complete();
            _logger.LogInformation($"Reminder created successfully for Task ID {task} by userId: {createdByUserId}");
            ScheduleReminder(reminder.Id, reminderDate);
            

        }

        private void ScheduleReminder(int reminderId, DateTime reminderDate)
        {
            // Schedule the job to run at the reminder date
            var jobId = BackgroundJob.Schedule(() => ProcessReminder(reminderId), reminderDate);

            // Save the job ID in the reminder entity
            var reminder = _unitOfWork.Repository<Reminder>().GetById(r => r.Id == reminderId).FirstOrDefault();
            if (reminder != null)
            {
                reminder.JobId = jobId;
                _unitOfWork.Repository<Reminder>().Update(reminder);
                _unitOfWork.Complete();
            }
            _logger.LogInformation($"Reminder with ID:{reminderId} successfully scheduled for: {reminderDate}");

        }

        public async Task ProcessReminder(int reminderId)
        {
            try
            {
                var reminder = _unitOfWork.Repository<Reminder>().GetById(x => x.Id == reminderId).FirstOrDefault();
                if (reminder == null)
                {
                    _logger.LogWarning($"Reminder with ID {reminderId} not found");
                    
                }
                var task = await _unitOfWork.Repository<Taski>().
                    GetById(x => x.Id == reminder.TaskId).
                    Include(x => x.AssignedUsers).
                    FirstOrDefaultAsync();

                if (task == null)
                {
                    _logger.LogWarning($"Task with ID {reminder.TaskId} for reminder {reminderId} not found.");
                    return;
                }

                var assignedUsers = task.AssignedUsers;
                var timeSpan = reminder.ReminderDateTime - task.DueDate ;
                var message =  $"{timeSpan.Days} days, {timeSpan.Hours} hours, {timeSpan.Minutes} minutes";

                foreach (var user in assignedUsers)
                {
                    
                    await _notificationService.CreateNotification(
                        user.Id, reminder.TaskId,
                        $"Description: {reminder.Description}\n" +
                        $" The task is due in: {message}",
                        $"Reminder for Task: {reminder.Task.Title}", "reminder");

                    _logger.LogInformation($"The notification for reminder {reminder.Id} created successfully");
                }
            }catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing reminder with ID {reminderId}");
            }
            
        }

        public async Task<bool> UpdateReminder(int reminderId,ReminderDto reminderDto)
        {
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
                return false;
            }

            if (!string.IsNullOrEmpty(reminderToUpdate.JobId))
            {
                BackgroundJob.Delete(reminderToUpdate.JobId);
            }


            if (reminderToUpdate.ReminderDateTime != reminderDto.ReminderDateTime)
            {
                ScheduleReminder(reminderToUpdate.Id, reminderDto.ReminderDateTime);
                _logger.LogInformation($"Reminder with ID: {reminderId} successfully rescheduled");
            }

            _mapper.Map(reminderDto, reminderToUpdate);

            _unitOfWork.Repository<Reminder>().Update(reminderToUpdate);
            _logger.LogInformation($"Reminder with ID: {reminderId} successfully updated");
            _unitOfWork.Complete();

           
            return true;
        }

        public async Task<bool> DeleteReminder(int reminderId)
        {
            var reminder = await _unitOfWork.Repository<Reminder>().GetById(x => x.Id == reminderId).FirstOrDefaultAsync();

            if(reminder == null)
            {
                return false;
            }
            if (!string.IsNullOrEmpty(reminder.JobId))
            {
                BackgroundJob.Delete(reminder.JobId);
                _logger.LogInformation($"Existing Hangfire job for reminder {reminderId} deleted.");
            }

            _unitOfWork.Repository<Reminder>().Delete(reminder);
            _logger.LogInformation($"Reminder with ID: {reminderId} successfully deleted");
            return _unitOfWork.Complete();
        }
    }
}
