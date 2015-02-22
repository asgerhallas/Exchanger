using System.Collections.Generic;
using System.Linq;
using d60.Cirqus.Events;
using Exchanger.Events;
using Exchanger.Model;
using Exchanger.Stores;

namespace Exchanger.Tests
{
    public class FakeStore : Store
    {
        List<CalendarItem> items;

        public FakeStore()
        {
            items = new List<CalendarItem>();
            Events = new List<DomainEvent>();
        }

        public List<DomainEvent> Events { get; private set; }

        public void SetItems(params CalendarItem[] newitems)
        {
            items = newitems.ToList();
        }

        public IEnumerable<CalendarItem> LoadItems()
        {
            return items;
        }

        public void Apply(CalendarItemCreated @event)
        {
            Events.Add(@event);
        }

        public void Apply(CalendarItemChanged @event)
        {
            Events.Add(@event);
        }

        public void Apply(CalendarItemRemoved @event)
        {
            Events.Add(@event);
        }
    }
}