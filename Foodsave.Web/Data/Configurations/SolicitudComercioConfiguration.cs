using Foodsave.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Foodsave.Web.Data.Configurations
{
    public class SolicitudComercioConfiguration : IEntityTypeConfiguration<SolicitudComercio>
    {
        public void Configure(EntityTypeBuilder<SolicitudComercio> builder)
        {
            builder.Property(s => s.PlanInteres)
                .HasMaxLength(20)
                .HasConversion<string>();
            builder.Property(s => s.Estado)
                .HasMaxLength(20)
                .HasConversion<string>();

            builder.Property(s => s.FechaSolicitud)
                .HasColumnType("timestamp with time zone");
            builder.Property(s => s.FechaRevision)
                .HasColumnType("timestamp with time zone");

            builder.HasIndex(s => s.Estado);
            builder.HasIndex(s => s.FechaSolicitud);
            builder.HasIndex(s => s.EmailTitular);
        }
    }
}
