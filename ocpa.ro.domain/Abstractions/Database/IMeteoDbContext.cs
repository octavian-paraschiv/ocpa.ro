using ocpa.ro.domain.Entities.Meteo;
using System.Linq;

namespace ocpa.ro.domain.Abstractions.Database;

public interface IMeteoDbContext : IBaseDbContext
{
    IQueryable<MeteoDbData> Data { get; }
}
