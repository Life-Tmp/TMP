using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMPDomain.Entities
{
    public class Invitation
    {

        public int Id{ get; set; }
        public string Email { get; set; } 
        public int ProjectId { get; set; } 
        public string Token { get; set; } 
        public DateTime CreatedAt { get; set; } 
        public DateTime ExpiresAt { get; set; } 
        public bool IsAccepted { get; set; } 
    }
}
