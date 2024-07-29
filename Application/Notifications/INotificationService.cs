using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Task = System.Threading.Tasks.Task;
using TMPDomain.Entities;
using TMPApplication.DTOs.NotificationDtos;

namespace TMPApplication.Notifications
{
    public interface INotificationService
    {
        Task CreateNotification(string userId,int? taskId, string message,string subject,string type);
        Task<List<NotificationDto>> GetAllNotifications();
        Task<List<NotificationDto>> GetLatestNotifications(int numberOfLatestNotifications);
        Task MarksAsRead(int notificationId);
    }
}
