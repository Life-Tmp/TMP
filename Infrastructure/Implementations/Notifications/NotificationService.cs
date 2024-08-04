using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using TMP.Application.Interfaces;
using TMPApplication.DTOs.NotificationDtos;
using TMPApplication.Hubs;
using TMPApplication.Notifications;
using TMPDomain.Entities;
using TMPInfrastructure.Messaging;
using Task = System.Threading.Tasks.Task;
using System.Collections.Generic;
using System.Linq;

namespace TMPInfrastructure.Implementations.Notifications
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccess;
        private readonly RabbitMQService _rabbitMQConfig;
        private readonly ILogger<NotificationService> _logger;
        private readonly IHubContext<NotificationHub> _notificationHub;
        private readonly IMapper _mapper;

        public NotificationService(
            IUnitOfWork unitOfWork,
            IHttpContextAccessor httpContextAccess,
            RabbitMQService rabbitMQConfig,
            ILogger<NotificationService> logger,
            IHubContext<NotificationHub> notificationHub,
            IMapper mapper)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _httpContextAccess = httpContextAccess;
            _rabbitMQConfig = rabbitMQConfig;
            _notificationHub = notificationHub;
            _mapper = mapper;
        }

        #region Create
        public async Task CreateNotification(string userId, int? taskId, string message, string subject, string type)
        {
            _logger.LogInformation("Creating notification for user {UserId} with task ID {TaskId}", userId, taskId);

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

            _unitOfWork.Repository<Notification>().Create(notification);
            _unitOfWork.Complete();

            _logger.LogInformation("Notification with ID: {NotificationId} created successfully", notification.Id);

            await _notificationHub.Clients.Group(userId).SendAsync("ReceiveNotifications", message);
            _rabbitMQConfig.PublishMessage(notification, type);
        }
        #endregion

        #region Read
        public async Task<List<NotificationDto>> GetAllNotifications()
        {
            var userId = _httpContextAccess.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                _logger.LogWarning("User ID not found in claims");
                return new List<NotificationDto>();
            }

            _logger.LogInformation("Fetching all notifications for user {UserId}", userId);

            var allNotifications = await _unitOfWork.Repository<Notification>().GetByCondition(x => x.UserId == userId).ToListAsync();

            if (allNotifications == null)
            {
                _logger.LogWarning("No notifications found for user {UserId}", userId);
                return new List<NotificationDto>();
            }

            return _mapper.Map<List<NotificationDto>>(allNotifications);
        }

        public async Task<List<NotificationDto>> GetLatestNotifications(int numberOfLatestNotifications)
        {
            var userId = _httpContextAccess.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                _logger.LogWarning("User ID not found in claims");
                return new List<NotificationDto>();
            }

            _logger.LogInformation("Fetching the latest {Number} notifications for user {UserId}", numberOfLatestNotifications, userId);

            var allNotifications = await _unitOfWork.Repository<Notification>()
                                                    .GetByCondition(x => x.UserId == userId)
                                                    .OrderByDescending(x => x.CreatedAt)
                                                    .Take(numberOfLatestNotifications)
                                                    .ToListAsync();

            return _mapper.Map<List<NotificationDto>>(allNotifications);
        }
        #endregion

        #region Update
        public async Task MarksAsRead(int notificationId)
        {
            _logger.LogInformation("Marking notification {NotificationId} as read", notificationId);

            var notification = await _unitOfWork.Repository<Notification>().GetById(x => x.Id == notificationId).FirstOrDefaultAsync();
            if (notification != null)
            {
                notification.IsRead = true;
                _unitOfWork.Repository<Notification>().Update(notification);
                _unitOfWork.Complete();
                _logger.LogInformation("Notification {NotificationId} marked as read", notificationId);
            }
            else
            {
                _logger.LogWarning("Notification {NotificationId} not found", notificationId);
            }
        }
        #endregion
    }
}
