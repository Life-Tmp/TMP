using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMPDomain.Entities
{
    public class Reminder
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string CreatedByUserId { get; set; }
        [Required]
        public DateTime ReminderDateTime {  get; set; }
        [Required]
        public int TaskId {  get; set; }

        public Task Task {  get; set; }

        public string JobId { get; set; }
        public bool IsCompleted { get; set; } = false;

    }
}
