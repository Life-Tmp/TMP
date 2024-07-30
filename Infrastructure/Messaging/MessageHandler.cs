using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMP.Application.Interfaces;
using TMPApplication.Interfaces;
using TMPDomain.Entities;
using TMPDomain.HelperModels;
using Tasku = TMPDomain.Entities.Task;
using Task = System.Threading.Tasks.Task;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Microsoft.AspNetCore.SignalR;
using TMPApplication.Hubs;

namespace TMPInfrastructure.Messaging
{
    
    public class MessageHandler
    {
        private readonly ILogger<MessageHandler> _logger;
        private readonly IEmailService _email;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<NotificationHub> _notificationHub;


        public MessageHandler(ILogger<MessageHandler> logger, IEmailService email, IUnitOfWork unitOfWork, IHubContext<NotificationHub> notificationHub)
        {
            _logger = logger;
            _email = email;
            _unitOfWork = unitOfWork;
            _notificationHub = notificationHub;
        }

        public async Task HandleNotificationAsync(string message)
        {
            var notification = JsonConvert.DeserializeObject<Notification>(message);
            var user = _unitOfWork.Repository<User>().GetByCondition(x => x.Id == notification.UserId).FirstOrDefault();
            Console.WriteLine(notification.Message);

            if(user == null)
            {

            }
            EmailMessage emaili = new EmailMessage 
            {
                Subject = notification.Subject,
                NotificationMessage = notification.Message,
                UserFirstName = user.FirstName,
                UserLastName = user.LastName,
                EmailAddress = user.Email,

            };
            await _notificationHub.Clients.User(notification.UserId).SendAsync("RecieveNotifications", notification.Message);
            _logger.LogInformation("this is cosumed");
            await _email.SendEmailNotification(emaili, Path.GetFullPath("..\\Infrastructure\\EmailTemplates\\EmailNotification.html"));
        }

        public async Task HandleTaskNotifications(string message)
        {
       
        var notification = JsonConvert.DeserializeObject<Notification>(message);
            var user = await _unitOfWork.Repository<User>().GetByCondition(x => x.Id == notification.UserId).FirstOrDefaultAsync();
            var task = await _unitOfWork.Repository<Tasku>().GetByCondition(x => x.Id == notification.TaskId).FirstOrDefaultAsync();
            Console.WriteLine(notification.Message);

            if (user == null)
            {

            }
            EmailMessage emaili = new EmailMessage
            {
                Subject = notification.Subject,
                NotificationMessage = notification.Message,
                UserFirstName = user.FirstName,
                UserLastName = user.LastName,
                EmailAddress = user.Email,
                TaskTitle = task.Title,
                TaskDescription = task.Description,
                TaskDueDate = task.DueDate
            };
            
            _logger.LogInformation("EmailMessage created successfully");
            await _email.SendEmailNotification(emaili, Path.GetFullPath("..\\Infrastructure\\EmailTemplates\\TaskAssignmentEmailNotification.html"));
        }
        
        public async Task HandleReminderNotificationAsync(string message)
        {
            var notification = JsonConvert.DeserializeObject<Notification>(message);
            var user = await _unitOfWork.Repository<User>().GetByCondition(x => x.Id == notification.UserId).FirstOrDefaultAsync();
            var task = await _unitOfWork.Repository<Tasku>().GetByCondition(x => x.Id == notification.TaskId).FirstOrDefaultAsync();

            if (user == null || task == null)
            {
                _logger.LogWarning($"User or Task not found for reminder notification: {notification.Id}");
                return;
            }

            var emailMessage = new EmailMessage
            {
                Subject = $"Reminder: {task.Title}",
                NotificationMessage = notification.Message,
                UserFirstName = user.FirstName,
                UserLastName = user.LastName,
                EmailAddress = user.Email,
                TaskTitle = task.Title,
                TaskDescription = task.Description,
                TaskDueDate = task.DueDate
            };
            
            await _email.SendEmailNotification(emailMessage, Path.GetFullPath("..\\Infrastructure\\EmailTemplates\\ReminderEmailNotification.html"));
            _logger.LogInformation("Reminder notification email sent successfully.");
        }
    }

    
}
