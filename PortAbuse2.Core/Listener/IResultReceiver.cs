using System.Threading.Tasks;
using PortAbuse2.Core.Result;

namespace PortAbuse2.Core.Listener
{
    public interface IResultReceiver
    {
        void Reset();

        void Add(ConnectionInformation result);

        void AddAsync(ConnectionInformation result);
    }
}