using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using TMP.Application.Interfaces;
using TMPApplication.DTOs.NotificationDtos;
using TMPApplication.Notifications;
using TMPDomain.Entities;
using TMPInfrastructure.Messaging;
using Task = System.Threading.Tasks.Task;
using Amazon.Runtime.Internal.Util;
using Microsoft.Extensions.Logging;
using TMPCommon.Constants;
using TMPApplication.DTOs.NotificationDtos;
using AutoMapper;
using TMPApplication.Hubs;
using Microsoft.AspNetCore.SignalR;


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

        public NotificationService(IUnitOfWork unitOfWork,
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
            _unitOfWork.Repository<Notification>().Create(notification);
            Console.WriteLine("Created Notification");
            _unitOfWork.Complete();

            await _notificationHub.Clients.Group(userId).SendAsync("ReceiveNotifications", message);

            _logger.LogInformation($"Notification with ID: {notification.Id} created successfully");
            _rabbitMQConfig.PublishMessage(notification, type);

        }

        public async Task<List<NotificationDto>> GetAllNotifications()
        {
            var userId = _httpContextAccess.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
            var allNotifications = await _unitOfWork.Repository<Notification>().GetByCondition(x => x.UserId == userId).ToListAsync();

            if(allNotifications == null)
            {
                return null; //TODO: Dont return null, try the other solution
            }

            return _mapper.Map<List<NotificationDto>>(allNotifications);
        }

        public async Task<List<NotificationDto>> GetLatestNotifications(int numberOfLatestNotifications)
        {
            var userId = _httpContextAccess.HttpContext.User.Claims.
                                                    FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;

            var allNotifications = await _unitOfWork.Repository<Notification>()
                                                    .GetByCondition(x => x.UserId == userId)
                                                    .OrderByDescending(x => x.CreatedAt) 
                                                    .Take(numberOfLatestNotifications) 
                                                    .ToListAsync();

            return _mapper.Map<List<NotificationDto>>(allNotifications);
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
