using ocpa.ro.domain.Models.Meteo;
using System.Collections.Generic;
using System.Threading.Tasks;
using ThorusCommon.SQLite;

namespace ocpa.ro.domain.Abstractions.Services;

public interface IMeteoDataService
{
    Task DropPreviewDatabase(int dbi);
    Task SavePreviewDatabase(int dbi, byte[] data);
    Task PromotePreviewDatabase(int dbi);
    Task<MeteoData> GetMeteoData(int dbi, GridCoordinates gc, string region, int skip, int take);
    Task<IEnumerable<MeteoDbInfo>> GetDatabases();
}
