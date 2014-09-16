using System;
using System.Collections.Generic;
using System.Linq;
using d60.Cirqus.Aggregates;
using d60.Cirqus.Events;
using Exchanger.Events;

namespace Exchanger.Model
{
    public class Calendar : AggregateRoot, 
        IEmit<CalendarItemCreated>,
        IEmit<CalendarItemRemoved>,
        IEmit<CalendarItemChanged>
    {
        readonly List<CalendarItem> items = new List<CalendarItem>();

        public IReadOnlyList<CalendarItem> Items
        {
            get { return items; }
        }

        public void Diff(IReadOnlyCollection<CalendarItem> remote)
        {
            var changes = new List<DomainEvent<Calendar>>();

            // new events
            changes.AddRange(
                from item in remote
                where !items.Contains(item, new EventIdComparer())
                select new CalendarItemCreated(item));

            // removed events
            changes.AddRange(
                from item in (IReadOnlyCollection<CalendarItem>) items
                where !remote.Contains(item, new EventIdComparer())
                select new CalendarItemRemoved(item));
            
            // changed events
            changes.AddRange(
                from remoteItem in remote
                from localItem in (IReadOnlyCollection<CalendarItem>) items
                where remoteItem.Id == localItem.Id
                from change in FindChanges(remoteItem, localItem)
                select change);

            foreach (var @event in changes)
            {
                Emit(@event);
            }
        }

        static IEnumerable<CalendarItemChanged> FindChanges(CalendarItem remoteItem, CalendarItem localItem)
        {
            if (remoteItem.Id != localItem.Id)
                throw new InvalidOperationException("Expects events with same id.");

            return from property in typeof(CalendarItem).GetProperties()
                   where property.Name != "Id"
                   let sourceValue = property.GetValue(remoteItem)
                   let targetValue = property.GetValue(localItem)
                   where !Equals(sourceValue, targetValue)
                   select new CalendarItemChanged(remoteItem.Id, property.Name, sourceValue);
        }

        public void Apply(CalendarItemCreated @event)
        {
            items.Add(@event.Item);
        }

        public void Apply(CalendarItemRemoved @event)
        {
            items.Remove(@event.Item);
        }

        public void Apply(CalendarItemChanged @event)
        {
            var changedItem = Items.Single(x => x.Id == @event.ItemId);
            typeof(CalendarItem)
                .GetProperty(@event.Property)
                .SetValue(changedItem, @event.NewValue);
        }

        class EventIdComparer : IEqualityComparer<CalendarItem>
        {
            public bool Equals(CalendarItem x, CalendarItem y)
            {
                return x.Id == y.Id;
            }

            public int GetHashCode(CalendarItem obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}