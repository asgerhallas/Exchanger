using System;
using d60.Cirqus.Events;
using Exchanger.Model;

namespace Exchanger.Events
{
    public class PushedToRemote : DomainEvent<LocalCalendar>
    {
        public PushedToRemote(string remoteId, string changeId)
        {
            RemoteId = remoteId;
            ChangeId = changeId;
        }

        public string RemoteId { get; private set; }
        public string ChangeId { get; private set; }
    }
}