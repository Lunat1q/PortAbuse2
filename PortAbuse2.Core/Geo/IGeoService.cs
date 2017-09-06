using System.Threading.Tasks;
using PortAbuse2.Core.Result;

namespace PortAbuse2.Core.Geo
{
    public interface IGeoService
    {
        string Name { get;}
        Task<GeoData> GetLocationByIp(string ip);

        string ToString();
    }
}