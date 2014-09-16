using System.Threading.Tasks;

namespace Exchanger.Stores
{
    public class GoogleCalendarRemote : Remote
    {
        public string Username { get; set; }
        public string CalendarId { get; set; }
        
        public async Task<Store> Connect()
        {
            return await GoogleCalendarStore.Create(Username, CalendarId);
        }
    }
}