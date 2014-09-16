using System;
using d60.Cirqus.Events;
using Exchanger.Model;

namespace Exchanger.Events
{
    public class CalendarItemRemoved : DomainEvent<Calendar>
    {
        public CalendarItemRemoved(CalendarItem item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            Item = item;
        }

        public CalendarItem Item { get; private set; }

        public override string ToString()
        {
            return string.Format("Event {0} was removed.", Item.Id);
        }

        public bool Equals(CalendarItemRemoved other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Item.Id, other.Item.Id);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as CalendarItemRemoved);
        }

        public override int GetHashCode()
        {
            return (Item != null ? Item.GetHashCode() : 0);
        }
    }
}