using ocpa.ro.domain.Abstractions.Gateways;
using ocpa.ro.domain.Models.Meteo;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ocpa.ro.infrastructure.Gateways;

public class GeoLocationGateway : IGeoLocationGateway
{
    private readonly HttpClient _client;

    public GeoLocationGateway(IHttpClientFactory factory)
    {
        _client = factory.CreateClient("geolocation");
    }

    public Task<GeoLocation> GetGeoLocation(string ipAddress)
        => _client.GetFromJsonAsync<GeoLocation>($"{ipAddress}?fields=66846719");
}
