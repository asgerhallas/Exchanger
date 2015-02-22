using d60.Cirqus.Events;
using Exchanger.Model;

namespace Exchanger.Events
{
    public class RemoteCalendarAdded : DomainEvent<LocalCalendar>
    {
        public RemoteCalendarAdded(RemoteCalendar remote)
        {
            Remote = remote;
        }

        public RemoteCalendar Remote { get; private set; }
    }
}