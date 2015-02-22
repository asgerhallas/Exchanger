using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using d60.Cirqus.Aggregates;
using d60.Cirqus.Events;
using d60.Cirqus.Extensions;
using Exchanger.Events;
using Exchanger.Stores;

namespace Exchanger.Model
{
    public class LocalCalendar : AggregateRoot, 
        IEmit<CalendarItemCreated>,
        IEmit<CalendarItemRemoved>,
        IEmit<CalendarItemChanged>,
        IEmit<PushedToRemote>,
        IEmit<RemoteCalendarAdded>
    {
        readonly List<CalendarItem> items = new List<CalendarItem>();
        readonly List<RemoteCalendar> remotes = new List<RemoteCalendar>();

        public IReadOnlyList<CalendarItem> Items
        {
            get { return items; }
        }

        public void AddRemote(RemoteCalendar remote)
        {
            Emit(new RemoteCalendarAdded(remote));
        }

        public async Task Pull(StoreFactory factory, Func<string> idFactory, string remoteId)
        {
            var remote = remotes.SingleOrDefault(x => x.Id == remoteId);

            if (remote == null)
            {
                throw new InvalidOperationException(string.Format("Remote with id {0} was not found", remoteId));
            }

            var store = await factory.Connect(remote);
            
            Diff(idFactory, store.LoadItems().ToList());
        }

        public async Task Push(StoreFactory factory, string remoteId)
        {
            var remote = remotes.SingleOrDefault(x => x.Id == remoteId);

            if (remote == null)
            {
                throw new InvalidOperationException(string.Format("Remote with id {0} was not found", remoteId));
            }

            var store = await factory.Connect(remote);

            foreach (var change in remote.Behind.ToList())
            {
                store.Apply((dynamic)change);
                Emit(new PushedToRemote(remote.Id, change.ChangeId));
            }
        }

        internal void Diff(Func<string> idFactory, IReadOnlyCollection<CalendarItem> remote)
        {
            var changes = new List<DomainEvent<LocalCalendar>>();

            // new events
            changes.AddRange(
                from item in remote
                where !items.Contains(item, new EventIdComparer())
                select new CalendarItemCreated(idFactory(), item));

            // removed events
            changes.AddRange(
                from item in items
                where !remote.Contains(item, new EventIdComparer())
                select new CalendarItemRemoved(idFactory(), item.Id));
            
            // changed events
            changes.AddRange(
                from remoteItem in remote
                from localItem in items
                where remoteItem.Id == localItem.Id
                from change in FindChanges(idFactory, remoteItem, localItem)
                select change);

            foreach (var @event in changes)
            {
                Emit(@event);
            }
        }

        public void Apply(RemoteCalendarAdded e)
        {
            remotes.Add(e.Remote);
        }

        public void Apply(CalendarItemCreated @event)
        {
            AddToUncommitted(@event);

            items.Add(@event.Item);
        }

        public void Apply(CalendarItemRemoved @event)
        {
            AddToUncommitted(@event);

            var item = items.Single(x => x.Id == @event.ItemId);
            items.Remove(item);
        }

        public void Apply(CalendarItemChanged @event)
        {
            AddToUncommitted(@event);

            var changedItem = items.Single(x => x.Id == @event.ItemId);
            typeof(CalendarItem)
                .GetProperty(@event.Property)
                .SetValue(changedItem, @event.NewValue);
        }

        void AddToUncommitted(Change @event)
        {
            foreach (var remote in remotes)
            {
                remote.Behind.Add(@event);
            }
        }

        public void Apply(PushedToRemote @event)
        {
            var remote = remotes.Single(x => x.Id == @event.RemoteId);
            var change = remote.Behind.Single(x => x.ChangeId == @event.ChangeId);
            remote.Behind.Remove(change);
        }

        static IEnumerable<CalendarItemChanged> FindChanges(Func<string> idFactory, CalendarItem remoteItem, CalendarItem localItem)
        {
            if (remoteItem.Id != localItem.Id)
                throw new InvalidOperationException("Expects events with same id.");

            return from property in typeof(CalendarItem).GetProperties()
                   where property.Name != "Id"
                   let sourceValue = property.GetValue(remoteItem)
                   let targetValue = property.GetValue(localItem)
                   where !Equals(sourceValue, targetValue)
                   select new CalendarItemChanged(idFactory(), remoteItem.Id, property.Name, sourceValue);
        }

        class EventIdComparer : IEqualityComparer<CalendarItem>
        {
            public bool Equals(CalendarItem x, CalendarItem y)
            {
                return x.Id == y.Id;
            }

            public int GetHashCode(CalendarItem obj)
            {
                return obj.GetHashCode();
            }
        }

    }

    public interface IdFactory
    {
        Guid Get();
    }

    public interface StoreFactory 
    {
        Task<Store> Connect(RemoteCalendar remote);
    }

    class DefaultStoreFactory : StoreFactory 
    {
        public async Task<Store> Connect(RemoteCalendar remote)
        {
            var google = remote as GoogleCalendarRemote;
            if (google != null)
            {
                return await GoogleCalendarStore.Create(google.Username, google.CalendarId);
            }

            throw new ArgumentOutOfRangeException("remote");
        }
    }
}