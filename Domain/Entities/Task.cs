using TMPDomain.Enumerations;

namespace TMPDomain.Entities
{
    public class Task
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public TaskPriority Priority { get; set; }
        public StatusOfTask Status { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int ProjectId { get; set; }
        public Project Project { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public ICollection<Attachment> Attachments { get; set; }
        public ICollection<TaskDuration> TaskDurations { get; set; }
        public ICollection<Reminder> Reminders { get; set; }
        public ICollection<Subtask> Subtasks { get; set; } = new List<Subtask>();
        public List<User> AssignedUsers { get; set; }
        public ICollection<Tag> Tags { get; set; } = new List<Tag>();
        public ICollection<TimeTracking> TimeTrackings { get; set; } = new List<TimeTracking>();
    }
    public class TimeTracking
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public Task Task { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public TimeSpan Duration { get; set; }
    }
}
