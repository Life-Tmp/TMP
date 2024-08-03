using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Task = System.Threading.Tasks.Task;
using TMPApplication.DTOs.ReminderDtos;
using TMPDomain.Entities;

namespace TMPApplication.Interfaces.Reminders
{
    public interface IReminderService
    {
        Task<GetReminderDto> GetReminderAsync(int reminderId);
        Task<List<GetReminderDto>> GetRemindersForTask(int taskId);
        Task CreateReminderAsync(CreateReminderDto createReminderDto);
        Task ProcessReminder(int reminderId); //FOr testing only
        Task<bool> UpdateReminder(int reminderId, ReminderDto reminderdto);
        Task<bool> DeleteReminder(int reminderId);
    }
}
