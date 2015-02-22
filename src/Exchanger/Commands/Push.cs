using d60.Cirqus.Commands;
using Exchanger.Model;

namespace Exchanger.Commands
{
    public class Push : ExecutableCommand
    {
        readonly StoreFactory storeFactory;

        public Push(StoreFactory storeFactory)
        {
            this.storeFactory = storeFactory;
        }

        public string CalendarId { get; set; }
        public string RemoteId { get; set; }

        public override void Execute(ICommandContext context)
        {
            var local = context.Load<LocalCalendar>(CalendarId);
            local.Push(storeFactory, RemoteId).Wait();
        }
    }
}