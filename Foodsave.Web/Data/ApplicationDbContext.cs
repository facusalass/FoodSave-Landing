using Microsoft.EntityFrameworkCore;
using Foodsave.Web.Models;

namespace Foodsave.Web.Data
{
    public class ApplicationDbContext : DbContext
    {
        // El constructor que recibe las configuraciones desde Program.cs
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Las tablas que se van a crear en la base de datos
        public DbSet<Comercio> Comercios { get; set; }
        public DbSet<Titular> Titulares { get; set; }
        public DbSet<Suscripcion> Suscripciones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Suscripcion>(entity =>
            {
                entity.Property(s => s.FechaInicio)
                    .HasColumnType("timestamp without time zone");
                entity.Property(s => s.FechaFin)
                    .HasColumnType("timestamp without time zone");
            });
        }
    }
}
