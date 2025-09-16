using ocpa.ro.domain.Models.Meteo;
using System.Threading.Tasks;

namespace ocpa.ro.domain.Abstractions.Gateways;

public interface IGeoLocationGateway
{
    Task<GeoLocation> GetGeoLocation(string ipAddress);
}
