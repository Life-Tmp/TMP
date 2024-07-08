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
        public DateTime CreatedAt { get; set; }
        public List<User> Users { get; set; }
        public List<Task> Tasks { get; set; }

    }
}
