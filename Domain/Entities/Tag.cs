using System.Collections.Generic;

namespace TMPDomain.Entities
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Color { get; set; } 
        public ICollection<Task> Tasks { get; set; } = new List<Task>(); 
    }
}
