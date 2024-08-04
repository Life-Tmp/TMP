using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using TMPApplication.Interfaces;

namespace TMPInfrastructure.Implementations.CalendarApi
{
    public class GoogleCalendarService: IGoogleCalendarService
    {
        private readonly CalendarService _calendarService;

        public GoogleCalendarService()
        {
            
            var credential = GoogleCredential.FromFile("..\\Infrastructure\\CalendarApi\\googleApi.json")
                .CreateScoped(CalendarService.Scope.Calendar);

           
            _calendarService = new CalendarService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "TMP ",
            });
        }

        /// <summary>
        /// Creates a new event in the specified calendar.
        /// </summary>
        /// <param name="calendarId">The ID of the calendar where the event will be created.</param>
        /// <param name="newEvent">The event details to be created.</param>
        /// <returns>The created event.</returns>
        public async Task<Event> CreateEventAsync(string calendarId, Event newEvent)
        {
            try
            {
                return await _calendarService.Events.Insert(newEvent, calendarId).ExecuteAsync();
            }
            catch (Exception ex)
            {
                
                throw new ApplicationException("Error creating event in Google Calendar ", ex);
            }
        }
        /// <summary>
        /// Creates a new calendar.
        /// </summary>
        /// <param name="projectName">The name of the new calendar.</param>
        /// <returns>The created calendar.</returns>
        public async Task<Calendar> CreateCalendarAsync(string projectName, string description)
        {
            var calendar = new Calendar
            {
               
                Summary = projectName, //THis is the title
                Description = description,
                TimeZone = "Europe/Tirane"
            };
            try
            {
                return await _calendarService.Calendars.Insert(calendar).ExecuteAsync();   
            }
            catch (Exception ex)
            {
                
                throw new ApplicationException($"Error creating calendar in Google Calendar for this Project Name : {projectName}", ex);
            }
        }
        public async Task<IList<Event>> GetEventsAsync(string calendarId)
        {
            try
            {
                var request = _calendarService.Events.List(calendarId);
                request.TimeMinDateTimeOffset = DateTime.UtcNow; // only future events(tasks)
                request.ShowDeleted = false;
                request.SingleEvents = true; // dont shows recurring events
                request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime; // by chronolgical order
                var events = await request.ExecuteAsync();
                return events.Items;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error fetching events from calendar: {calendarId}", ex);
            }
        }

    }

}
