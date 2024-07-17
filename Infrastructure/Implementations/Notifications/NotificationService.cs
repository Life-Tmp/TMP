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

        public void CreateNotification(string userId, string message)
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
            };
            //notification.User = null; //TODO: Find a way to serialize even the relationship
            _unitOfWork.Repository<Notification>().Create(notification);

            _unitOfWork.Complete();
           
            _rabbitMQConfig.PublishMessage(notification, "notification");

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

        public void MarksAsRead(int notificationId)
        {
            var notification = _unitOfWork.Repository<Notification>().GetById(x => x.Id == notificationId).FirstOrDefault();
            if (notification != null)
            {
                notification.IsRead = true;
                _unitOfWork.Repository<Notification>().Update(notification);
                _unitOfWork.Complete();
            }
                

        }
    }
}
