using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMP.Application.Interfaces;
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

        public ReminderService(IUnitOfWork unitOfWork, ILogger<ReminderService> logger, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _notificationService = notificationService;
        }

        public async Task CreateReminderAsync(string description, DateTime reminderDate, int taskId)
        {
            if (reminderDate < DateTime.UtcNow)
            {
                _logger.LogWarning("Reminder date cannot be in the past");
                throw new ArgumentException("Reminder date cannot be in the past");
            }

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
                Task = task
            };

            _unitOfWork.Repository<Reminder>().Create(reminder);
            

            _logger.LogInformation("Reminder created successfully for Task ID {TaskId}", taskId);
            ScheduleReminder(reminder.Id, reminderDate);
            _unitOfWork.Complete();

        }
        private void ScheduleReminder(int reminderId, DateTime reminderDate)
        {
            // Schedule the job to run at the reminder date
            BackgroundJob.Schedule(() => ProcessReminder(reminderId), reminderDate);
        }
        public async Task ProcessReminder(int reminderId)
        {
            var reminder = _unitOfWork.Repository<Reminder>().GetById(x => x.Id == reminderId).FirstOrDefault();
            if (reminder != null)
            {
                
                var task = await _unitOfWork.Repository<Taski>().GetById(x => x.Id == reminder.TaskId).Include(x => x.AssignedUsers).FirstOrDefaultAsync();
                var assignedUsers = task.AssignedUsers;

                foreach (var user in assignedUsers)
                {
                    //var notification = new Notification
                    //{
                    //    UserId = user.Id,
                    //    TaskId = task.Id,
                    //    Subject = $"Reminder for Task: {task.Title}",
                    //    Message = $"You have a reminder for the task: {task.Description}",
                    //    CreatedAt = DateTime.UtcNow,
                    //    IsRead = false,
                    //    NotificationType = "Reminder"
                    //};
                    Console.WriteLine(user.Id, reminder.TaskId, $"You have a reminder for the task: {reminder.Task.Description}", $"Reminder for Task: {reminder.Task.Title}", "reminder"); //TODO: remove this
                    await _notificationService.CreateNotification(user.Id, reminder.TaskId, $"You have a reminder for the task: {reminder.Task.Description}", $"Reminder for Task: {reminder.Task.Title}", "reminder");
                    _logger.LogInformation($"The notification for reminder {reminder.Id} created successfully");
                }

                // Publish the notification to RabbitMQ

            }

        }
    }
}
