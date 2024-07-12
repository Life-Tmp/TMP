using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMPDomain.Entities
{
    public class TaskDuration
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int TaskId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration => EndTime - StartTime;
        public Task Task { get; set; }
        public User User { get; set; }
    }
}
