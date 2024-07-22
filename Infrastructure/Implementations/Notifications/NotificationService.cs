using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Task = System.Threading.Tasks.Task;
using TMP.Application.Interfaces;
using TMPApplication.Notifications;
using TMPDomain.Entities;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using TMPInfrastructure.Messaging;
using Amazon.Runtime.Internal.Util;
using Microsoft.Extensions.Logging;
using TMPCommon.Constants;

namespace TMPInfrastructure.Implementations.Notifications
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccess;
        private readonly RabbitMQService _rabbitMQConfig;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccess, RabbitMQService rabbitMQConfig,ILogger<NotificationService> logger)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _httpContextAccess = httpContextAccess;
            _rabbitMQConfig = rabbitMQConfig;
        }

        public async Task CreateNotification(string userId,int? taskId, string message,string subject,string type)
        {
            var notification = new Notification
            {
                UserId = userId,
                TaskId = taskId,
                Subject = subject,
                Message = message,
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                NotificationType = type
            };
            //notification.User = null; //TODO: Find a way to serialize even the relationship
            _unitOfWork.Repository<Notification>().Create(notification);
            Console.WriteLine("Created Notification");
            _unitOfWork.Complete();
            _logger.LogInformation($"Notification with ID: {notification.Id} created successfully");
            _rabbitMQConfig.PublishMessage(notification, type);

        }

        public async Task<List<Notification>> GetAllNotifications()
        {
            var userId = _httpContextAccess.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
            var allNotifications = await _unitOfWork.Repository<Notification>().GetByCondition(x => x.UserId == userId).ToListAsync();

            if(allNotifications == null)
            {
                return null; //TODO: Dont return null, try the other solution
            }

            return allNotifications;
        }

        public async Task MarksAsRead(int notificationId)
        {
            var notification = await _unitOfWork.Repository<Notification>().GetById(x => x.Id == notificationId).FirstOrDefaultAsync();
            if (notification != null)
            {
                notification.IsRead = true;
                _unitOfWork.Repository<Notification>().Update(notification);
               _unitOfWork.Complete();
            }
                

        }
    }
}
