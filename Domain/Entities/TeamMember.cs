using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPDomain.Enumerations;

namespace TMPDomain.Entities
{
    public class TeamMember
    {
        public int TeamId { get; set; }
        public Team Team { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public MemberRole Role { get; set; }
    }
}
