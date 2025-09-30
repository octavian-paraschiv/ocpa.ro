using Microsoft.EntityFrameworkCore;
using ocpa.ro.domain.Entities.Application;

namespace ocpa.ro.persistence.ApplicationDb.Configurations;

internal static class MenuConfiguration
{
    internal static void Build(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Menu>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("menu");

            entity.HasIndex(e => e.DisplayModeId, "DisplayModeId");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)");

            entity.Property(e => e.Builtin)
                .HasColumnType("tinyint(1)")
                .HasDefaultValueSql("0")
                .HasSentinel(false);

            entity.Property(e => e.DisplayModeId)
                .HasDefaultValueSql("'NULL'")
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

            entity.HasOne(d => d.DisplayMode)
                .WithMany(p => p.Menus)
                .HasForeignKey(d => d.DisplayModeId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("menu_ibfk_1");
        });
    }
}

