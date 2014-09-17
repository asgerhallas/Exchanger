using d60.Cirqus.Events;
using Exchanger.Model;

namespace Exchanger.Events
{
    public class RemoteAdded : DomainEvent<Calendar>
    {
        public Remote Remote { get; set; }
    }
}