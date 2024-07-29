using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMPApplication.DTOs.ReminderDtos
{
    public class GetReminderDto
    {
        
        
        public string Description { get; set; }
        public DateTime ReminderDateTime { get; set; }
        public int TaskId { get; set; }

    }
}
