using System;
using d60.Cirqus.Testing;
using d60.Cirqus.xUnit;
using Exchanger.Commands;
using Exchanger.Events;
using Exchanger.Model;
using NodaTime;
using Shouldly;
using Xunit;

namespace Exchanger.Tests.Commands
{
    public class ExchangerTests : CirqusTests
    {
        public ExchangerTests() : base(
            TestContext.With()
                .Options(x => x.UseCustomDomainEventSerializer(new EventSerializer()))
                .Create()) {}

        public string NewId<T>()
        {
            return NewId<T>(new object[0]);
        }
    }

    public class PushTests : ExchangerTests
    {
        readonly FakeStore store;
        readonly TestStoreFactory storeFactory;
        readonly DateTimeZone timeZone;

        public PushTests()
        {
            timeZone = DateTimeZoneProviders.Tzdb.GetSystemDefault();

            store = new FakeStore();
            storeFactory = new TestStoreFactory(store);

            Emit(NewId<LocalCalendar>(),
                 new RemoteCalendarAdded(
                     new FakeRemote
                     {
                         Id = NewId<RemoteCalendar>()
                     }));
        }

        [Fact]
        public void PushesChanges()
        {
            var created = new CalendarItemCreated("hash1", new CalendarItem("1234"));
            var changed = new CalendarItemChanged("hash2", "1234", "Title", "Asger");
            var removed = new CalendarItemRemoved("hash3", "1234");
            
            Emit(Id<LocalCalendar>(), created, changed, removed);

            When(new Push(storeFactory)
            {
                CalendarId = Id<LocalCalendar>(),
                RemoteId = Id<RemoteCalendar>()
            });

            Then(new PushedToRemote(Id<RemoteCalendar>(), created.ChangeId));
            Then(new PushedToRemote(Id<RemoteCalendar>(), changed.ChangeId));
            Then(new PushedToRemote(Id<RemoteCalendar>(), removed.ChangeId));
        }

        [Fact]
        public void AppliesChangesToStore()
        {
            var created = new CalendarItemCreated("hash1", new CalendarItem("1234"));
            var changed = new CalendarItemChanged("hash2", "1234", "Title", "Asger");
            var removed = new CalendarItemRemoved("hash3", "1234");

            Emit(Id<LocalCalendar>(), created, changed, removed);

            When(new Push(storeFactory)
            {
                CalendarId = Id<LocalCalendar>(),
                RemoteId = Id<RemoteCalendar>()
            });

            Then<PushedToRemote>();
            Then<PushedToRemote>();
            Then<PushedToRemote>();

            store.Events[0].ShouldBeOfType<CalendarItemCreated>().Item.Id.ShouldBe("1234");
            store.Events[1].ShouldBeOfType<CalendarItemChanged>().ItemId.ShouldBe("1234");
            store.Events[2].ShouldBeOfType<CalendarItemRemoved>().ItemId.ShouldBe("1234");
        }

        [Fact]
        public void PushesChangesSinceLastPush()
        {
            var created1 = new CalendarItemCreated("hash1", new CalendarItem("1234"));
            Emit(created1);

            Emit(new PushedToRemote(Id<RemoteCalendar>(), created1.ChangeId));

            var created2 = new CalendarItemCreated("hash2", new CalendarItem("4321"));
            Emit(created2);

            When(new Push(storeFactory)
            {
                CalendarId = Id<LocalCalendar>(),
                RemoteId = Id<RemoteCalendar>()
            });

            Then(new PushedToRemote(Id<RemoteCalendar>(), created2.ChangeId));
        }

        [Fact]
        public void PushesChangesSinceLastPushWhenMultipleRemotes()
        {
            Emit(new RemoteCalendarAdded(
                     new FakeRemote
                     {
                         Id = NewId<RemoteCalendar>()
                     }));

            var created1 = new CalendarItemCreated("hash1", new CalendarItem("1234"));
            Emit(created1);

            Emit(new PushedToRemote(Id<RemoteCalendar>(), created1.ChangeId));

            var created2 = new CalendarItemCreated("hash2", new CalendarItem("4321"));
            Emit(created2);

            When(new Push(storeFactory)
            {
                CalendarId = Id<LocalCalendar>(),
                RemoteId = Id<RemoteCalendar>(2)
            });

            Then(new PushedToRemote(Id<RemoteCalendar>(2), created1.ChangeId));
            Then(new PushedToRemote(Id<RemoteCalendar>(2), created2.ChangeId));
            
        }
    }
}