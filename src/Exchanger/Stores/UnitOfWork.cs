using System;
using d60.Cirqus.Events;
using Exchanger.Events;
using Exchanger.Model;

namespace Exchanger.Stores
{
    public class UnitOfWork : IDisposable
    {
        readonly Store store;
        bool disposed;

        public UnitOfWork(Store store)
        {
            this.store = store;
        }

        public void Apply(params DomainEvent[] events)
        {
            if (disposed) throw new InvalidOperationException("UnitOfWork can only be used once");

            foreach (var @event in events)
            {
                var eventCreated = @event as CalendarItemCreated;
                if (eventCreated != null)
                {
                    store.Apply(eventCreated);
                    continue;
                }

                var eventChanged = @event as CalendarItemChanged;
                if (eventChanged != null)
                {
                    store.Apply(eventChanged);
                    continue;
                }
            }

            Dispose();
        }

        public void Dispose()
        {
            disposed = true;
        }
    }
}