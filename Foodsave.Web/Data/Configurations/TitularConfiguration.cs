using Foodsave.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Foodsave.Web.Data.Configurations
{
    public class TitularConfiguration : IEntityTypeConfiguration<Titular>
    {
        public void Configure(EntityTypeBuilder<Titular> builder)
        {
            builder.Property(t => t.Nombre).HasMaxLength(100);
            builder.Property(t => t.Apellido).HasMaxLength(100);
            builder.Property(t => t.Email).HasMaxLength(200);
            builder.Property(t => t.Telefono).HasMaxLength(40);

            builder.HasIndex(t => t.Email).IsUnique();
        }
    }
}
