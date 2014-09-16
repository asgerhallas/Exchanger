using d60.Cirqus.Events;
using Exchanger.Model;

namespace Exchanger.Events
{
    public class CalendarItemChanged : DomainEvent<Calendar>
    {
        public CalendarItemChanged(string itemId, string property, object newValue)
        {
            ItemId = itemId;
            Property = property;
            NewValue = newValue;
        }

        public string ItemId { get; private set; }
        public string Property { get; set; }
        public object NewValue { get; set; }
        public CalendarItem Original { get; set; }

        public override string ToString()
        {
            return string.Format("item {0} changed {1} to {2}.", ItemId, Property, NewValue);
        }

        protected bool Equals(CalendarItemChanged other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return string.Equals(ItemId, other.ItemId) &&
                   string.Equals(Property, other.Property) &&
                   Equals(NewValue, other.NewValue);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as CalendarItemChanged);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (ItemId != null ? ItemId.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Property != null ? Property.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (NewValue != null ? NewValue.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}