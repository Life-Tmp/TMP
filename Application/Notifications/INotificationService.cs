using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Task = System.Threading.Tasks.Task;
using TMPDomain.Entities;

namespace TMPApplication.Notifications
{
    public interface INotificationService
    {
        Task CreateNotification(string userId,int? taskId, string message,string subject,string type);
        Task<List<Notification>> GetAllNotifications();

        Task MarksAsRead(int notificationId);
    }
}
