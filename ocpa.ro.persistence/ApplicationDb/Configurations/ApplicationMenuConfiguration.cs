using Microsoft.EntityFrameworkCore;
using ocpa.ro.domain.Entities.Application;

namespace ocpa.ro.persistence.ApplicationDb.Configurations;

internal static class ApplicationMenuConfiguration
{
    internal static void Build(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApplicationMenu>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("applicationmenu");

            entity.HasIndex(e => e.ApplicationId, "ApplicationId");

            entity.HasIndex(e => e.MenuId, "MenuId");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)");

            entity.Property(e => e.ApplicationId)
                .HasDefaultValueSql("'NULL'")
                .HasColumnType("int(11)");

            entity.Property(e => e.MenuId)
                .HasColumnType("int(11)");

            entity.HasOne(d => d.Application)
                .WithMany(p => p.ApplicationMenus)
                .HasForeignKey(d => d.ApplicationId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("applicationmenu_ibfk_3");

            entity.HasOne(d => d.Menu)
                .WithMany(p => p.ApplicationMenus)
                .HasForeignKey(d => d.MenuId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("applicationmenu_ibfk_2");
        });
    }
}
