using Microsoft.EntityFrameworkCore;
using ocpa.ro.domain.Entities.Application;

namespace ocpa.ro.persistence.ApplicationDb.Configurations;

internal static class RegionConfiguration
{
    internal static void Build(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Region>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("region");

            entity.HasIndex(e => e.Code, "Code")
                .IsUnique();

            entity.HasIndex(e => e.Name, "Name")
                .IsUnique();

            entity.Property(e => e.Id)
                .HasColumnType("int(11)");

            entity.Property(e => e.Code)
                .IsRequired()
                .HasMaxLength(5);

            entity.Property(e => e.Name)
                .IsRequired();
        });
    }
}
