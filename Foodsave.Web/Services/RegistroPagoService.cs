using Foodsave.Web.Data;
using Foodsave.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Foodsave.Web.Services
{
    public class RegistroPagoService
    {
        private readonly ApplicationDbContext _context;
        private readonly GestionSuscripcionesService _gestionSuscripciones;

        public RegistroPagoService(
            ApplicationDbContext context,
            GestionSuscripcionesService gestionSuscripciones)
        {
            _context = context;
            _gestionSuscripciones = gestionSuscripciones;
        }

        public async Task<Comercio?> ObtenerComercioConSuscripcionesAsync(int id)
        {
            return await _context.Comercios
                .AsNoTracking()
                .Include(c => c.Suscripciones)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<(
            Comercio? Comercio,
            Suscripcion? Suscripcion)> ObtenerComercioYSuscripcionAsync(
                int comercioId,
                int? suscripcionIdFilter = null)
        {
            var comercio = await _context.Comercios
                .Include(c => c.Suscripciones)
                .FirstOrDefaultAsync(c => c.Id == comercioId);

            var suscripcion = suscripcionIdFilter.HasValue
                ? comercio?.Suscripciones
                    .FirstOrDefault(s => s.Id == suscripcionIdFilter.Value)
                : _gestionSuscripciones.ObtenerSuscripcionActual(
                    comercio?.Suscripciones ?? new List<Suscripcion>(),
                    DateTime.Today);

            return (comercio, suscripcion);
        }

        public async Task RegistrarAsync(
            Comercio comercio,
            Suscripcion suscripcion,
            RegistroPagoCommand command)
        {
            var fechaPago = command.FechaPago.Date;

            _context.Pagos.Add(new Pago
            {
                ComercioId = comercio.Id,
                SuscripcionId = suscripcion.Id,
                Monto = command.Monto,
                FechaPago = fechaPago,
                Observacion = command.Observacion
            });

            suscripcion.FechaUltimoPago = fechaPago;
            suscripcion.FechaProximoVencimiento = fechaPago.AddDays(30);
            suscripcion.EstadoPago = EstadoPagoSuscripcion.AlDia;
            suscripcion.Estado = EstadoSuscripcion.Activa;

            if (comercio.EstadoAdministrativo == EstadoAdministrativo.PendientePago)
                comercio.EstadoAdministrativo = EstadoAdministrativo.Activo;

            await _context.SaveChangesAsync();
        }
    }
}
