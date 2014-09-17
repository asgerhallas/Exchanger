using System.Threading.Tasks;
using Exchanger.Stores;

namespace Exchanger.Model
{
    public interface Remote
    {
        Task<Store> Connect();
    }
}