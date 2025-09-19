using Microsoft.EntityFrameworkCore;
using ocpa.ro.domain.Abstractions.Database;
using ocpa.ro.domain.Entities.Meteo;
using ocpa.ro.persistence.MeteoDb.Configurations;
using System.Linq;

namespace ocpa.ro.persistence.MeteoDb;

public partial class MeteoDbContext : BaseDbContext, IMeteoDbContext
{
    public MeteoDbContext(DbContextOptions<MeteoDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<MeteoDbData> Data { get; set; }

    IQueryable<MeteoDbData> IMeteoDbContext.Data => Data;


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        MeteoDataConfiguration.Build(modelBuilder);
    }
}
