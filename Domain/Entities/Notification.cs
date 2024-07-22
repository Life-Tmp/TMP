using System;
using System.Reflection.Metadata;

namespace TMPDomain.Entities
{
    public class Notification
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int? TaskId {  get; set; }
        public string Subject {  get; set; }
        public string Message { get; set; }
        public string NotificationType {  get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public User User { get; set; }

       
    }
}
