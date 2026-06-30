using Foodsave.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Foodsave.Web.Data.Configurations
{
    public class PagoConfiguration : IEntityTypeConfiguration<Pago>
    {
        public void Configure(EntityTypeBuilder<Pago> builder)
        {
            builder.Property(p => p.Monto)
                .HasPrecision(12, 2);
            builder.Property(p => p.FechaPago)
                .HasColumnType("timestamp without time zone");

            builder.HasOne(p => p.Comercio)
                .WithMany(c => c.Pagos)
                .HasForeignKey(p => p.ComercioId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.Suscripcion)
                .WithMany(s => s.Pagos)
                .HasForeignKey(p => p.SuscripcionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(p => new { p.ComercioId, p.SuscripcionId, p.FechaPago });
        }
    }
}
