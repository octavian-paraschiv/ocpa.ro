using Microsoft.EntityFrameworkCore;
using ocpa.ro.domain.Entities.Application;

namespace ocpa.ro.persistence.ApplicationDb.Configurations;

internal static class AppMenuConfiguration
{
    internal static void Build(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppMenu>(entity =>
        {
            entity.HasNoKey().ToView("appmenu");

            entity.Property(e => e.AdminMode)
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0")
                .HasSentinel(false);

            entity.Property(e => e.AppName)
                .HasMaxLength(16);

            entity.Property(e => e.DisplayMode)
                .HasDefaultValueSql("'0'")
                .HasColumnType("int(11)");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)");

            entity.Property(e => e.MenuIcon)
                .HasMaxLength(64)
                .HasDefaultValueSql("'NULL'");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(64);

            entity.Property(e => e.Url)
                .IsRequired()
                .HasMaxLength(128);

            entity.Property(e => e.UserId)
                .HasDefaultValueSql("'0'")
                .HasColumnType("int(11)");
        });
    }
}