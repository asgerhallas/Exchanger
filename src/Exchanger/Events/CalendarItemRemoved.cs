using System;

namespace Exchanger.Events
{
    public class CalendarItemRemoved : Change
    {
        public CalendarItemRemoved(string changeId, string itemId) : base(changeId)
        {
            ItemId = itemId;
        }

        public string ItemId { get; private set; }

        public override string ToString()
        {
            return string.Format("Item {0} was removed.", ItemId);
        }
    }
}