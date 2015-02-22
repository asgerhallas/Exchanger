using System;
using System.Collections.Generic;
using d60.Cirqus.Events;
using Exchanger.Commands;
using Exchanger.Events;
using Exchanger.Model;
using NodaTime;
using Xunit;

namespace Exchanger.Tests.Commands
{
    public class PullTests : ExchangerTests
    {
        readonly FakeStore store;
        readonly TestStoreFactory storeFactory;
        readonly DateTimeZone timeZone;

        public PullTests()
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
        public void RemoteHasNewItems()
        {
            var item = new CalendarItem("a")
            {
                Title = "Hej"
            };

            store.SetItems(item);

            When(new Pull(storeFactory, NewId<Change>)
            {
                CalendarId = Id<LocalCalendar>(),
                RemoteId = Id<RemoteCalendar>()
            });

            Then(new CalendarItemCreated(Id<Change>(), item));
        }

        [Fact]
        public void RemoteEventHasChangedTitle()
        {
            Emit(new CalendarItemCreated(
                     "hash",
                     new CalendarItem("a")
                     {
                         Title = "Hej"
                     }));

            store.SetItems(new CalendarItem("a") { Title = "Hund" });

            When(new Pull(storeFactory, NewId<Change>)
            {
                CalendarId = Id<LocalCalendar>(),
                RemoteId = Id<RemoteCalendar>()
            });

            Then(new CalendarItemChanged(Id<Change>(), "a", "Title", "Hund"));
        }

        [Fact]
        public void SourceEventHasChangedEverything()
        {
            Emit(new CalendarItemCreated(
                    "hash",
                    new CalendarItem("1234")
                    {
                        Title = "Karin",
                        Start = new ZonedDateTime(Instant.FromUtc(2014, 1, 19, 22, 50, 0), timeZone),
                        End = new ZonedDateTime(Instant.FromUtc(2014, 1, 19, 23, 50, 0), timeZone),
                        IsAllDay = true,
                        Location = "Over the rainbow",
                        Note = "I'm not so sure?",
                    }));

            store.SetItems(
                new CalendarItem("1234")
                {
                    Title = "Asger",
                    Start = new ZonedDateTime(Instant.FromUtc(2014, 1, 18, 22, 50, 0), timeZone),
                    End = new ZonedDateTime(Instant.FromUtc(2014, 1, 18, 23, 50, 0), timeZone),
                    IsAllDay = false,
                    Location = "Somewhere",
                    Note = "We do it!",
                });

            When(new Pull(storeFactory, NewId<Change>)
            {
                CalendarId = Id<LocalCalendar>(),
                RemoteId = Id<RemoteCalendar>()
            });

            Then(Id<LocalCalendar>(),
                 new CalendarItemChanged(Id<Change>(1), "1234", "Title", "Asger"),
                 new CalendarItemChanged(Id<Change>(2), "1234", "Note", "We do it!"),
                 new CalendarItemChanged(Id<Change>(3), "1234", "Location", "Somewhere"),
                 new CalendarItemChanged(Id<Change>(4), "1234", "Start", new ZonedDateTime(Instant.FromUtc(2014, 1, 18, 22, 50, 0), timeZone)),
                 new CalendarItemChanged(Id<Change>(5), "1234", "End", new ZonedDateTime(Instant.FromUtc(2014, 1, 18, 23, 50, 0), timeZone)),
                 new CalendarItemChanged(Id<Change>(6), "1234", "IsAllDay", false));
        }

        [Fact]
        public void NoChanges()
        {
            store.SetItems(
                new CalendarItem("1234")
                {
                    Title = "Asger",
                    Start = new ZonedDateTime(Instant.FromUtc(2014, 1, 18, 22, 50, 0), timeZone),
                    End = new ZonedDateTime(Instant.FromUtc(2014, 1, 18, 23, 50, 0), timeZone),
                    IsAllDay = false,
                    Location = "Somewhere",
                    Note = "We do it!",
                });

            Emit(new CalendarItemCreated(
                    "hash", 
                    new CalendarItem("1234")
                    {
                        Title = "Asger",
                        Start = new ZonedDateTime(Instant.FromUtc(2014, 1, 18, 22, 50, 0), timeZone),
                        End = new ZonedDateTime(Instant.FromUtc(2014, 1, 18, 23, 50, 0), timeZone),
                        IsAllDay = false,
                        Location = "Somewhere",
                        Note = "We do it!",
                    }));

            When(new Pull(storeFactory, NewId<Change>)
            {
                CalendarId = Id<LocalCalendar>(),
                RemoteId = Id<RemoteCalendar>()
            });

            ThenNo<DomainEvent>();
        }
    }
}