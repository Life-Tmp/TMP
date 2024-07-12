using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMPDomain.Entities
{
    public class Comment
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public int TaskId { get; set; }
        public string UserId { get; set; }
        public DateTime CreatedAt { get; set; }

        public Task Task { get; set; }
        public User User { get; set; }
    }
}
