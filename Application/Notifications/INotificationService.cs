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
        void CreateNotification(string userId, string message);
        Task<List<Notification>> GetAllNotifications();

        void MarksAsRead(int notificationId);
    }
}
