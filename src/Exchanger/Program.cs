using System;
using System.Threading.Tasks;
using d60.Cirqus;
using d60.Cirqus.Ntfs.Config;

namespace Exchanger
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                new Program().Run(args).Wait();
            }
            catch (AggregateException ex)
            {
                foreach (var e in ex.InnerExceptions)
                {
                    Console.WriteLine("ERROR: " + e.Message);
                }
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        async Task Run(string[] args)
        {
            var processor = CommandProcessor.With()
                .EventStore(x => x.UseFiles("."))
                .Create();


            //add remote
            //add remote
            //sync remotes
        }
    }
}
