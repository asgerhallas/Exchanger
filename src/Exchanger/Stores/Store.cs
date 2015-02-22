using System.Collections.Generic;
using Exchanger.Events;
using Exchanger.Model;

namespace Exchanger.Stores
{
    public interface Store
    {
        IEnumerable<CalendarItem> LoadItems(); 
        void Apply(CalendarItemCreated @event);
        void Apply(CalendarItemChanged @event);
        void Apply(CalendarItemRemoved @event);
    }
}