using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Exchanger.Events;
using Exchanger.Model;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using NodaTime;
using Calendar = Google.Apis.Calendar.v3.Data.Calendar;

namespace Exchanger.Stores
{
    public class GoogleCalendarStore : Store, IDisposable
    {
        readonly CalendarService service;
        readonly string calendarId;
        readonly Action dispose;

        GoogleCalendarStore(CalendarService service, string calendarId, Action dispose)
        {
            this.service = service;
            this.calendarId = calendarId;
            this.dispose = dispose;
        }

        public static async Task<Store> Create(string username, string calendarId)
        {
            var service = await CreateInternal(username);
            return new GoogleCalendarStore(service, calendarId, () => { });
        }

        public static async Task<GoogleCalendarStore> CreateForTestingWithTempCalendar(string username)
        {
            var service = await CreateInternal(username);

            var calendar = service.Calendars.Insert(
                new Calendar
                {
                    Summary = "ExchangerTestCalendar",
                    Description = "For testing"
                }).Execute();

            var calendarId = calendar.Id;

            return new GoogleCalendarStore(
                service, calendarId,
                () => service.Calendars.Delete(calendarId).Execute());
        }

        static async Task<CalendarService> CreateInternal(string username)
        {
            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets
                {
                    ClientId = "1090817542797-3nf6u64imjooeb3sdhij457rau4417o2.apps.googleusercontent.com",
                    ClientSecret = GetSecret()
                },
                new[] {CalendarService.Scope.Calendar},
                username,
                CancellationToken.None,
                new FileDataStore("Exchanger"));

            return new CalendarService(
                new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Exchanger",
                });
        }

        public IEnumerable<CalendarItem> LoadItems()
        {
            var listRequest = service.Events.List(calendarId);
            var items = listRequest.Execute();

            return
                from item in items.Items
                let isAllDay = item.Start.Date != null
                select new CalendarItem(item.ICalUID)
                {
                    Title = item.Summary,
                    Note = item.Description,
                    Location = item.Location,
                    IsAllDay = isAllDay,
                    Start = FromGoogleEventDateTime(item.Start, isAllDay),
                    End = FromGoogleEventDateTime(item.End, isAllDay)
                };
        }

        public void Apply(CalendarItemCreated @event)
        {
            service.Events.Insert(new Event
            {
                ICalUID = @event.Item.Id,
                Summary = @event.Item.Title,
                Description = @event.Item.Note,
                Location = @event.Item.Location,
                Start = ToGoogleEventDateTime(@event.Item.Start, @event.Item.IsAllDay),
                End = ToGoogleEventDateTime(@event.Item.End, @event.Item.IsAllDay)
            }, calendarId).Execute();
        }

        public void Apply(CalendarItemChanged @event)
        {
            service.Events.Update(new Event(), calendarId, @event.ItemId).Execute();
        }

        public void Apply(CalendarItemRemoved @event)
        {
            throw new NotImplementedException();
        }

        static ZonedDateTime FromGoogleEventDateTime(EventDateTime dateTime, bool isAllDay)
        {
            var instant = isAllDay
                ? Instant.FromDateTimeUtc(DateTime.SpecifyKind(DateTime.Parse(dateTime.Date), DateTimeKind.Utc))
                : Instant.FromDateTimeOffset(dateTime.DateTime.Value);
            
            var timezone = isAllDay
                ? DateTimeZoneProviders.Tzdb.GetSystemDefault()
                : DateTimeZoneProviders.Tzdb[dateTime.TimeZone];

            return instant.InZone(timezone);
        }

        static EventDateTime ToGoogleEventDateTime(ZonedDateTime dateTime, bool isAllDay)
        {
            return isAllDay
                ? new EventDateTime
                {
                    Date = dateTime.Date.ToString("yyyy-MM-dd", new DateTimeFormatInfo())
                }
                : new EventDateTime
                {
                    DateTime = dateTime.ToDateTimeUtc(),
                    TimeZone = dateTime.Zone.Id
                };
        }

        static string GetSecret()
        {
            using (var secretRessource = typeof (GoogleCalendarStore).Assembly.GetManifestResourceStream("Exchanger.googlecalendar.secret"))
            {
                if (secretRessource == null)
                    throw new InvalidOperationException("Missing googlecalendar.secret embedded ressource.");

                using (var reader = new StreamReader(secretRessource))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        public void Dispose()
        {
            dispose();
        }
    }
}