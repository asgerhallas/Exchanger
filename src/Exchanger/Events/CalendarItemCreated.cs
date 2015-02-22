using System;
using Exchanger.Model;

namespace Exchanger.Events
{
    public class CalendarItemCreated : Change
    {
        public CalendarItemCreated(string changeId, CalendarItem item) : base(changeId)
        {
            Item = item;
        }

        public CalendarItem Item { get; private set; }

        public override string ToString()
        {
            return string.Format("item {0} was created.", Item.Id);
        }
    }
}