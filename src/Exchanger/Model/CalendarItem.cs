using NodaTime;

namespace Exchanger.Model
{
    public class CalendarItem
    {
        public CalendarItem(string id)
        {
            Id = id;
        }

        public string Id { get; private set; }
        public string Title { get; set; }
        public string Note { get; set; }
        public string Location { get; set; }
        public ZonedDateTime Start { get; set; }
        public ZonedDateTime End { get; set; }
        public bool IsAllDay { get; set; }
    }
}