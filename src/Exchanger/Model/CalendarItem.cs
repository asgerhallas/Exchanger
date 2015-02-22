using System;
using NodaTime;

namespace Exchanger.Model
{
    public class CalendarItem : IEquatable<CalendarItem>
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

        public bool Equals(CalendarItem other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Id, other.Id) && 
                string.Equals(Title, other.Title) && 
                string.Equals(Note, other.Note) && 
                string.Equals(Location, other.Location) && 
                Start.Equals(other.Start) && 
                End.Equals(other.End) && 
                IsAllDay.Equals(other.IsAllDay);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((CalendarItem) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Id != null ? Id.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Title != null ? Title.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Note != null ? Note.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Location != null ? Location.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ Start.GetHashCode();
                hashCode = (hashCode*397) ^ End.GetHashCode();
                hashCode = (hashCode*397) ^ IsAllDay.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(CalendarItem left, CalendarItem right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(CalendarItem left, CalendarItem right)
        {
            return !Equals(left, right);
        }
    }
}