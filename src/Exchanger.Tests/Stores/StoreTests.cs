using System;
using System.Linq;
using Exchanger.Events;
using Exchanger.Model;
using Exchanger.Stores;
using NodaTime;
using Shouldly;
using Xunit;

namespace Exchanger.Tests.Stores
{
    public class StoreTests : IDisposable
    {
        readonly GoogleCalendarStore store;
        readonly ZonedDateTime localNow;

        public StoreTests()
        {
            store = GoogleCalendarStore.CreateForTestingWithTempCalendar("ahl@asgerhallas.dk").Result;
            
            var now = Instant.FromUtc(2014, 1, 18, 22, 50, 0);
            var timeZone = DateTimeZoneProviders.Tzdb.GetSystemDefault();
            localNow = new ZonedDateTime(now, timeZone);
        }

        [Fact]
        public void CanCreateAndGetEvents()
        {
            store.Apply(new CalendarItemCreated(
                "hash",
                new CalendarItem("test1")
                {
                    Title = "Hente skurpenge",
                    Note = "Bare der er ål",
                    Location = "Mårslet, DK",
                    Start = localNow,
                    End = localNow + Duration.FromHours(1),
                }));

            var @event = store.LoadItems().Single();
            @event.Id.ShouldBe("test1");
            @event.Title.ShouldBe("Hente skurpenge");
            @event.Note.ShouldBe("Bare der er ål");
            @event.Location.ShouldBe("Mårslet, DK");
            @event.Start.ShouldBe(localNow);
            @event.End.ShouldBe(localNow + Duration.FromHours(1));
        }

        [Fact]
        public void CanCreateAndGetAllDayEvents()
        {
            store.Apply(new CalendarItemCreated(
                "hash",
                new CalendarItem("test1")
                {
                    IsAllDay = true,
                    Start = localNow,
                    End = localNow + Duration.FromStandardDays(2),
                }));

            var @event = store.LoadItems().Single();
            @event.IsAllDay.ShouldBe(true);
            @event.Start.Date.ShouldBe(localNow.Date);
            @event.End.Date.ShouldBe((localNow + Duration.FromStandardDays(2)).Date);
        }

        public void Dispose()
        {
            store.Dispose();
        }
    }
}