using Microsoft.EntityFrameworkCore;
using ocpa.ro.domain.Entities.Application;

namespace ocpa.ro.persistence.ApplicationDb.Configurations;

internal static class RegisteredDeviceConfiguration
{
    internal static void Build(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RegisteredDevice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("registereddevice");

            entity.HasIndex(e => e.DeviceId, "DeviceId").IsUnique();

            entity.Property(e => e.DeviceId)
                .IsRequired()
                .HasMaxLength(64);

            entity.Property(e => e.Id)
                .HasColumnType("int(11)");

            entity.Property(e => e.LastLoginGeoLocation)
                .HasMaxLength(1024)
                .HasDefaultValueSql("'NULL'");

            entity.Property(e => e.LastLoginId)
                .IsRequired()
                .HasMaxLength(64);

            entity.Property(e => e.LastLoginIpAddress)
                .HasMaxLength(64)
                .HasDefaultValueSql("'NULL'");

            entity.Property(e => e.LastLoginTimestamp)
                .HasColumnType("datetime");
        });
    }
}
