using Microsoft.EntityFrameworkCore;
using ocpa.ro.domain.Entities.Meteo;

namespace ocpa.ro.persistence.MeteoDb.Configurations;

internal static class MeteoDataConfiguration
{
    internal static void Build(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MeteoDbData>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("meteodata");

            entity.HasIndex(e => new { e.Dbi, e.R, e.C, e.RegionCode, e.Timestamp }, "RecordSearch").IsUnique();

            entity.Property(e => e.Id)
                .HasColumnType("int(11)");

            entity.Property(e => e.C)
                .HasColumnType("smallint(6)");

            entity.Property(e => e.C_00)
                .HasColumnType("mediumint(9)");

            entity.Property(e => e.Dbi)
                .HasColumnType("tinyint(4)");

            entity.Property(e => e.F_SI)
                .HasColumnType("mediumint(9)");

            entity.Property(e => e.L_00)
                .HasColumnType("mediumint(9)");

            entity.Property(e => e.N_00)
                .HasColumnType("mediumint(9)");

            entity.Property(e => e.N_DD)
                .HasColumnType("mediumint(9)");

            entity.Property(e => e.P_00)
                .HasColumnType("mediumint(9)");

            entity.Property(e => e.P_01)
                .HasColumnType("mediumint(9)");

            entity.Property(e => e.R)
                .HasColumnType("smallint(6)");

            entity.Property(e => e.R_00)
                .HasColumnType("mediumint(9)");

            entity.Property(e => e.R_DD)
                .HasColumnType("mediumint(9)");

            entity.Property(e => e.RegionCode)
                .IsRequired()
                .HasMaxLength(2);

            entity.Property(e => e.T_01)
                .HasColumnType("mediumint(9)");

            entity.Property(e => e.T_NH)
                .HasColumnType("mediumint(9)");

            entity.Property(e => e.T_NL)
                .HasColumnType("mediumint(9)");

            entity.Property(e => e.T_SH)
                .HasColumnType("mediumint(9)");

            entity.Property(e => e.T_SL)
                .HasColumnType("mediumint(9)");

            entity.Property(e => e.T_TE)
                .HasColumnType("mediumint(9)");

            entity.Property(e => e.T_TS)
                .HasColumnType("mediumint(9)");

            entity.Property(e => e.Timestamp)
                .HasColumnType("date");

            entity.Property(e => e.W_00)
                .HasColumnType("mediumint(9)");

            entity.Property(e => e.W_01)
                .HasColumnType("mediumint(9)");

            entity.Property(e => e.W_10)
                .HasColumnType("mediumint(9)");

            entity.Property(e => e.W_11)
                .HasColumnType("mediumint(9)");
        });
    }
}
