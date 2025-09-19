using Microsoft.EntityFrameworkCore;
using ocpa.ro.domain.Entities.Application;

namespace ocpa.ro.persistence.ApplicationDb.Configurations;

internal static class MenuDisplayModeConfiguration
{
    internal static void Build(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MenuDisplayMode>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("menudisplaymode");

            entity.HasIndex(e => e.Code, "Code").IsUnique();

            entity.Property(e => e.Id)
                .HasColumnType("int(11)");

            entity.Property(e => e.Code)
                .IsRequired()
                .HasMaxLength(5);

            entity.Property(e => e.Description)
                .HasMaxLength(16)
                .HasDefaultValueSql("'NULL'");
        });
    }
}
