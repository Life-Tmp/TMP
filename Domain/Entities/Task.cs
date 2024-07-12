using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPDomain.ValueObjects;

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
        public List<User> AssignedUsers { get; set; }
        public ICollection<Tag> Tags { get; set; } = new List<Tag>();
    }

}
