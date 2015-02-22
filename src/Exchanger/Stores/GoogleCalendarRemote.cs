using Exchanger.Model;

namespace Exchanger.Stores
{
    public class GoogleCalendarRemote : RemoteCalendar
    {
        public string Username { get; set; }
        public string CalendarId { get; set; }
    }
}