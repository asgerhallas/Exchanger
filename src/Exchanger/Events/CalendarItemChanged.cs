namespace Exchanger.Events
{
    public class CalendarItemChanged : Change
    {
        public CalendarItemChanged(string changeId, string itemId, string property, object newValue) : base(changeId)
        {
            ItemId = itemId;
            Property = property;
            NewValue = newValue;
        }

        public string ItemId { get; private set; }
        public string Property { get; set; }
        public object NewValue { get; set; }

        public override string ToString()
        {
            return string.Format("item {0} changed {1} to {2}.", ItemId, Property, NewValue);
        }
    }
}