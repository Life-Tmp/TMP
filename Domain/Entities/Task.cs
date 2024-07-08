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
        public List<User> AssignedUser { get; set; }

        public int ProjectId { get; set; }
        public Project Project { get; set; }

        public List<Tag> Tags { get; set; }
        public TaskPriority Priority { get; set; }

        public StatusOfTask Status { get; set; }

        public List<Attachment> Attachments { get; set; }
        public TaskDuration TaskDuration { get; set; }
        public List<Comment> Comments { get; set; }
        public DateTime CreatedAt {  get; set; }
        public DateTime DueDate { get; set; }

    }

}
