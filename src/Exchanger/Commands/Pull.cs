using System;
using d60.Cirqus.Commands;
using Exchanger.Model;

namespace Exchanger.Commands
{
    public class Pull : ExecutableCommand
    {
        readonly StoreFactory storeFactory;
        readonly Func<string> idFactory;

        public Pull(StoreFactory storeFactory, Func<string> idFactory)
        {
            this.storeFactory = storeFactory;
            this.idFactory = idFactory;
        }

        public string CalendarId { get; set; }
        public string RemoteId { get; set; }

        public override void Execute(ICommandContext context)
        {
            var local = context.Load<LocalCalendar>(CalendarId);
            local.Pull(storeFactory, idFactory, RemoteId).Wait();
        }
    }
}