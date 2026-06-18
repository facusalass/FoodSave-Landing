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
        public DbSet<Pago> Pagos { get; set; }
        public DbSet<SolicitudComercio> SolicitudesComercio { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Comercio>(entity =>
            {
                entity.Property(c => c.EstadoAdministrativo)
                    .HasMaxLength(20);
                entity.HasIndex(c => c.EstadoAdministrativo);
            });

            modelBuilder.Entity<Suscripcion>(entity =>
            {
                entity.Property(s => s.Plan)
                    .HasMaxLength(20);
                entity.Property(s => s.Estado)
                    .HasMaxLength(20);
                entity.Property(s => s.EstadoPago)
                    .HasMaxLength(20);
                entity.Property(s => s.MontoMensual)
                    .HasPrecision(12, 2);
                entity.Property(s => s.FechaInicio)
                    .HasColumnType("timestamp without time zone");
                entity.Property(s => s.FechaFin)
                    .HasColumnType("timestamp without time zone");
                entity.Property(s => s.FechaUltimoPago)
                    .HasColumnType("timestamp without time zone");
                entity.Property(s => s.FechaProximoVencimiento)
                    .HasColumnType("timestamp without time zone");
                entity.HasOne(s => s.Comercio)
                    .WithMany(c => c.Suscripciones)
                    .HasForeignKey(s => s.ComercioId);
                entity.HasIndex(s => s.EstadoPago);
                entity.HasIndex(s => s.FechaProximoVencimiento);
            });

            modelBuilder.Entity<Pago>(entity =>
            {
                entity.Property(p => p.Monto)
                    .HasPrecision(12, 2);
                entity.Property(p => p.FechaPago)
                    .HasColumnType("timestamp without time zone");
                entity.HasOne(p => p.Comercio)
                    .WithMany(c => c.Pagos)
                    .HasForeignKey(p => p.ComercioId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(p => p.Suscripcion)
                    .WithMany(s => s.Pagos)
                    .HasForeignKey(p => p.SuscripcionId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(p => p.ComercioId);
                entity.HasIndex(p => p.SuscripcionId);
                entity.HasIndex(p => p.FechaPago);
            });

            modelBuilder.Entity<SolicitudComercio>(entity =>
            {
                entity.Property(s => s.FechaSolicitud)
                    .HasColumnType("timestamp with time zone");
                entity.Property(s => s.FechaRevision)
                    .HasColumnType("timestamp with time zone");
                entity.HasIndex(s => s.Estado);
                entity.HasIndex(s => s.FechaSolicitud);
            });
        }
    }
}
