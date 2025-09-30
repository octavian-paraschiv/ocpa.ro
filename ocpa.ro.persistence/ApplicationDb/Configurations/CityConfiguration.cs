using Microsoft.EntityFrameworkCore;
using ocpa.ro.domain.Entities.Application;

namespace ocpa.ro.persistence.ApplicationDb.Configurations;

internal static class CityConfiguration
{
    internal static void Build(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<City>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("city");

            entity.HasIndex(e => e.RegionId, "RegionId");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)");

            entity.Property(e => e.IsDefault)
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0")
                .HasSentinel(false);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.RegionId)
                .HasColumnType("int(11)");

            entity.Property(e => e.Subregion)
                .IsRequired()
                .HasMaxLength(255);

            entity.HasOne(d => d.Region)
                .WithMany(p => p.Cities)
                .HasForeignKey(d => d.RegionId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("city_ibfk_1");
        });
    }
}
