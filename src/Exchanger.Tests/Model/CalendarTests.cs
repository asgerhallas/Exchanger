using System;
using System.Collections.Generic;
using System.Linq;
using d60.Cirqus.Events;
using d60.Cirqus.TestHelpers;
using Exchanger.Events;
using Exchanger.Model;
using NodaTime;
using Shouldly;
using Xunit;

namespace Exchanger.Tests.Model
{
    public class CalendarTests
    {
        readonly Calendar calendar;
        readonly TestUnitOfWork unitOfWork;

        public CalendarTests()
        {
            unitOfWork = new TestContext().BeginUnitOfWork();
            calendar = unitOfWork.Get<Calendar>(Guid.NewGuid());
        }

        [Fact]
        public void SourceHasNewEvent()
        {
            var newItem = new CalendarItem("A");

            calendar.Diff(Yield(newItem));

            unitOfWork.EmittedEvents.Single().ShouldBe(new CalendarItemCreated(newItem));
        }

        [Fact]
        public void SourceEventHasChangedTitle()
        {
            calendar.Apply(new CalendarItemCreated(new CalendarItem("A") { Title = "Karin" }));

            calendar.Diff(Yield(
                new CalendarItem("A") { Title = "Asger" }));

            unitOfWork.EmittedEvents.Single().ShouldBe(new CalendarItemChanged("A", "Title", "Asger"));
        }

        [Fact]
        public void SourceEventHasChangedEverything()
        {
            var timeZone = DateTimeZoneProviders.Tzdb.GetSystemDefault();

            calendar.Apply(
                new CalendarItemCreated(
                    new CalendarItem("1234")
                    {
                        Title = "Karin",
                        Start = new ZonedDateTime(Instant.FromUtc(2014, 1, 19, 22, 50, 0), timeZone),
                        End = new ZonedDateTime(Instant.FromUtc(2014, 1, 19, 23, 50, 0), timeZone),
                        IsAllDay = true,
                        Location = "Over the rainbow",
                        Note = "I'm not so sure?",
                    }));

            calendar.Diff(Yield(
                new CalendarItem("1234")
                {
                    Title = "Asger",
                    Start = new ZonedDateTime(Instant.FromUtc(2014, 1, 18, 22, 50, 0), timeZone),
                    End = new ZonedDateTime(Instant.FromUtc(2014, 1, 18, 23, 50, 0), timeZone),
                    IsAllDay = false,
                    Location = "Somewhere",
                    Note = "We do it!",
                }));

            unitOfWork.EmittedEvents.ToList().ShouldBe(new List<DomainEvent>
            {
                new CalendarItemChanged("1234", "Title", "Asger"),
                new CalendarItemChanged("1234", "Note", "We do it!"),
                new CalendarItemChanged("1234", "Location", "Somewhere"),
                new CalendarItemChanged("1234", "Start", new ZonedDateTime(Instant.FromUtc(2014, 1, 18, 22, 50, 0), timeZone)),
                new CalendarItemChanged("1234", "End", new ZonedDateTime(Instant.FromUtc(2014, 1, 18, 23, 50, 0), timeZone)),
                new CalendarItemChanged("1234", "IsAllDay", false),
            });
        }

        [Fact]
        public void NoChanges()
        {
            var timeZone = DateTimeZoneProviders.Tzdb.GetSystemDefault();

            calendar.Apply(
                new CalendarItemCreated(new CalendarItem("1234")
                {
                    Title = "Asger",
                    Start = new ZonedDateTime(Instant.FromUtc(2014, 1, 18, 22, 50, 0), timeZone),
                    End = new ZonedDateTime(Instant.FromUtc(2014, 1, 18, 23, 50, 0), timeZone),
                    IsAllDay = false,
                    Location = "Somewhere",
                    Note = "We do it!",
                }));

            calendar.Diff(Yield(
                new CalendarItem("1234")
                {
                    Title = "Asger",
                    Start = new ZonedDateTime(Instant.FromUtc(2014, 1, 18, 22, 50, 0), timeZone),
                    End = new ZonedDateTime(Instant.FromUtc(2014, 1, 18, 23, 50, 0), timeZone),
                    IsAllDay = false,
                    Location = "Somewhere",
                    Note = "We do it!",
                }));

            unitOfWork.EmittedEvents.ShouldBeEmpty();
        }

        [Fact]
        public void CanApplyCalendarItemCreated()
        {
            var calendarItem = new CalendarItem("test");
            calendar.Apply(new CalendarItemCreated(calendarItem));
            calendar.Items.ShouldContain(calendarItem);
        }

        [Fact]
        public void CanApplyCalendarItemRemoved()
        {
            var calendarItem = new CalendarItem("test");
            calendar.Apply(new CalendarItemCreated(calendarItem));
            calendar.Apply(new CalendarItemRemoved(calendarItem));
            calendar.Items.Count.ShouldBe(0);
        }

        [Fact]
        public void CanApplyCalendarItemChanged()
        {
            var calendarItem = new CalendarItem("test")
            {
                Title = "Asger"
            };
            calendar.Apply(new CalendarItemCreated(calendarItem));
            calendar.Apply(new CalendarItemChanged("test", "Title", "Emil"));
            calendar.Items[0].Title.ShouldBe("Emil");
        }

        static IReadOnlyCollection<T> Yield<T>(params T[] items)
        {
            return items.ToList();
        }
    }
}