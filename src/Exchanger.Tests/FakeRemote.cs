using System.Threading.Tasks;
using Exchanger.Model;
using Exchanger.Stores;

namespace Exchanger.Tests
{
    public class TestStoreFactory : StoreFactory
    {
        readonly FakeStore store;

        public TestStoreFactory(FakeStore store)
        {
            this.store = store;
        }

        public async Task<Store> Connect(RemoteCalendar remote)
        {
            return store;
        }
    }

    public class FakeRemote : RemoteCalendar
    {
    }
}