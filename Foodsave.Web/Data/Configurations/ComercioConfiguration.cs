using Foodsave.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Foodsave.Web.Data.Configurations
{
    public class ComercioConfiguration : IEntityTypeConfiguration<Comercio>
    {
        public void Configure(EntityTypeBuilder<Comercio> builder)
        {
            builder.Property(c => c.Nombre).HasMaxLength(150);
            builder.Property(c => c.Rubro).HasMaxLength(100);
            builder.Property(c => c.Direccion).HasMaxLength(200);
            builder.Property(c => c.Telefono).HasMaxLength(40);

            builder.Property(c => c.Plan)
                .HasMaxLength(20)
                .HasConversion<string>();

            builder.Property(c => c.EstadoAdministrativo)
                .HasMaxLength(20)
                .HasConversion<string>();

            builder.HasOne(c => c.Titular)
                .WithMany()
                .HasForeignKey(c => c.TitularId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(c => c.EstadoAdministrativo);
            builder.HasIndex(c => c.Plan);
            builder.HasIndex(c => c.Nombre).IsUnique();
            builder.HasIndex(c => c.Rubro);
        }
    }
}
