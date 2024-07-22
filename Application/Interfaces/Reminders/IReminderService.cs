using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMPApplication.Interfaces.Reminders
{
    public interface IReminderService
    {
        Task CreateReminderAsync(string description, DateTime reminderDate, int taskId);
        Task ProcessReminder(int reminderId); //FOr testing only
    }
}
