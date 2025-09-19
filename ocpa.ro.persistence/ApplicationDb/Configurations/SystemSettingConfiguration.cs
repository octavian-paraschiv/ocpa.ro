using Microsoft.EntityFrameworkCore;
using ocpa.ro.domain.Entities.Application;

namespace ocpa.ro.persistence.ApplicationDb.Configurations;

internal static class SystemSettingConfiguration
{
    internal static void Build(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SystemSetting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("systemsetting");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(65535);

            entity.Property(e => e.Value)
                .IsRequired()
                .HasMaxLength(65535);

            entity.Property(e => e.Type)
                .IsRequired()
                .HasMaxLength(65535);
        });
    }
}
