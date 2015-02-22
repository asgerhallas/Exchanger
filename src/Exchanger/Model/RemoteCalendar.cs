using System.Collections.Generic;
using d60.Cirqus.Events;
using Exchanger.Events;

namespace Exchanger.Model
{
    public abstract class RemoteCalendar
    {
        protected RemoteCalendar()
        {
            Behind = new List<Change>();
        }

        public string Id { get; set; }
        public long Revision { get; set; }
        public List<Change> Behind { get; private set; }
    }
}