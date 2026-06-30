using Foodsave.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Foodsave.Web.Data.Configurations
{
    public class SuscripcionConfiguration : IEntityTypeConfiguration<Suscripcion>
    {
        public void Configure(EntityTypeBuilder<Suscripcion> builder)
        {
            builder.Property(s => s.Plan)
                .HasMaxLength(20)
                .HasConversion<string>();
            builder.Property(s => s.Estado)
                .HasMaxLength(20)
                .HasConversion<string>();
            builder.Property(s => s.EstadoPago)
                .HasMaxLength(20)
                .HasConversion<string>();

            builder.Property(s => s.MontoMensual)
                .HasPrecision(12, 2);

            builder.Property(s => s.FechaInicio)
                .HasColumnType("timestamp without time zone");
            builder.Property(s => s.FechaFin)
                .HasColumnType("timestamp without time zone");
            builder.Property(s => s.FechaUltimoPago)
                .HasColumnType("timestamp without time zone");
            builder.Property(s => s.FechaProximoVencimiento)
                .HasColumnType("timestamp without time zone");

            builder.HasOne(s => s.Comercio)
                .WithMany(c => c.Suscripciones)
                .HasForeignKey(s => s.ComercioId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(s => s.Estado);
            builder.HasIndex(s => s.EstadoPago);
            builder.HasIndex(s => s.FechaProximoVencimiento);
        }
    }
}
