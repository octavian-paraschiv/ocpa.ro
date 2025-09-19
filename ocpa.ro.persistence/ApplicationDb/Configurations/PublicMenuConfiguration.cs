using Microsoft.EntityFrameworkCore;
using ocpa.ro.domain.Entities.Application;

namespace ocpa.ro.persistence.ApplicationDb.Configurations;

internal static class PublicMenuConfiguration
{
    internal static void Build(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PublicMenu>(entity =>
        {
            entity.HasNoKey().ToView("publicmenu");

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
        });
    }
}