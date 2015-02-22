using System;
using d60.Cirqus.Events;
using Exchanger.Model;

namespace Exchanger.Events
{
    public abstract class Change : DomainEvent<LocalCalendar>
    {
        protected Change(string changeId)
        {
            ChangeId = changeId;
        }

        public string ChangeId { get; private set; }
    }
}