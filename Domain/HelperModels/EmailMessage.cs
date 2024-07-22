using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMPDomain.HelperModels
{
    public class EmailMessage
    {
        public string Subject { get; set; }
        public string NotificationMessage { get; set; }
        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }
        public string EmailAddress { get; set; }
        public string? TaskTitle {  get; set; }
        public string? TaskDescription { get; set; }
        public DateTime? TaskDueDate { get; set; }
    }
}
