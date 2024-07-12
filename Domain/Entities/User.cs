﻿using TMPDomain.ValueObjects;

namespace TMPDomain.Entities
{
    public class User
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ICollection<Project> ProjectsCreated { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public ICollection<Notification> Notifications { get; set; }
        public ICollection<TaskDuration> TaskDurations { get; set; }
        public ICollection<Task> AssignedTasks { get; set; }
        public ICollection<Project> Projects { get; set; } = new List<Project>(); // Add this line
    }

}
