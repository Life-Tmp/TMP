using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Task = TMPDomain.Entities.Task;

namespace TMPDomain.Entities
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public User CreatedByUser { get; set; }
        public ICollection<Task> Tasks { get; set; }
        public ICollection<User> Users { get; set; } = new List<User>(); // Add this line
    }
}


