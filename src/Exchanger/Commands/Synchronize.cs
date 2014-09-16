using System;
using System.Collections.Generic;
using System.Linq;
using d60.Cirqus.Commands;
using Exchanger.Model;
using Exchanger.Stores;

namespace Exchanger.Commands
{
    public class Synchronize : Command<Calendar>
    {
        public Synchronize(Guid aggregateRootId) : base(aggregateRootId) {}

        public List<Remote> Remotes { get; set; }

        public override async void Execute(Calendar aggregateRoot)
        {
            foreach (var remote in Remotes)
            {
                var google = await remote.Connect();
                var items = google.LoadItems().ToList();
                aggregateRoot.Diff(items);
            }
        }
    }
}