using Google.Apis.Calendar.v3.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMPApplication.Interfaces
{
    public interface IGoogleCalendarService
    {
        Task<Calendar> CreateCalendarAsync(string projectName, string description);
        Task<IList<Event>> GetEventsAsync(string calendarId);
        Task<Event> CreateEventAsync(string calendarId, Event newEvent);
    }
}
