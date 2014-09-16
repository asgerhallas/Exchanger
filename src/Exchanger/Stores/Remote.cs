using System.Threading.Tasks;

namespace Exchanger.Stores
{
    public interface Remote
    {
        Task<Store> Connect();
    }
}