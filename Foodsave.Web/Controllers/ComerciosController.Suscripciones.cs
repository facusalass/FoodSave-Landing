using Foodsave.Web.Data;
using Foodsave.Web.Helpers;
using Foodsave.Web.Models;
using Foodsave.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Foodsave.Web.Controllers
{
    public partial class ComerciosController
    {
        [HttpGet]
        public async Task<IActionResult> Suscripcion(int id)
        {
            var comercio = await _context.Comercios
                .AsNoTracking()
                .AsSplitQuery()
                .Include(c => c.Titular)
                .Include(c => c.Suscripciones)
                .Include(c => c.Pagos)
                    .ThenInclude(p => p.Suscripcion)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comercio is null)
                return NotFound();

            comercio.Suscripciones = comercio.Suscripciones
                .OrderByDescending(s => s.FechaInicio)
                .ToList();
            comercio.Pagos = comercio.Pagos
                .OrderByDescending(p => p.FechaPago)
                .ThenByDescending(p => p.Id)
                .ToList();

            var (suscripcionActual, estadoPago) =
                _gestionSuscripciones.ObtenerEstadoCompleto(
                    comercio.Suscripciones, DateTime.Today);

            ViewData["SuscripcionActual"] = suscripcionActual;
            ViewData["EstadoPago"] = estadoPago;

            return View(new ComercioAdministracionViewModel
            {
                Comercio = comercio,
                SuscripcionActual = suscripcionActual,
                EstadoPagoEfectivo = estadoPago
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarSuscripcion(
            int id,
            ActualizarSuscripcionInputModel model)
        {
            if (id != model.ComercioId)
                return BadRequest();

            var plan = PlanHelper.NormalizarPlan(model.Plan);
            if (plan is null)
                ModelState.AddModelError(nameof(model.Plan), "Plan inválido.");

            if (model.FechaProximoVencimiento == default)
                ModelState.AddModelError(nameof(model.FechaProximoVencimiento), "Ingresá la fecha.");

            if (!ModelState.IsValid)
                return await SuscripcionView(id);

            var suscripcion = await _context.Suscripciones
                .FirstOrDefaultAsync(s => s.Id == model.SuscripcionId && s.ComercioId == id);

            if (suscripcion is null)
                return NotFound();

            suscripcion.Plan = Enum.Parse<PlanSuscripcion>(plan!);
            suscripcion.MontoMensual = model.MontoMensual;
            suscripcion.FechaProximoVencimiento = model.FechaProximoVencimiento.Date;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Suscripción editada: Id={SusId}, Plan={Plan}", suscripcion.Id, plan);
            TempData["Success"] = "Suscripción actualizada.";
            return RedirectToAction(nameof(Suscripcion), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelarSuscripcion(int id, int suscripcionId)
        {
            var suscripcion = await _context.Suscripciones
                .FirstOrDefaultAsync(s => s.Id == suscripcionId && s.ComercioId == id);

            if (suscripcion is null)
                return NotFound();

            suscripcion.Estado = EstadoSuscripcion.Cancelada;
            suscripcion.FechaFin = DateTime.Today;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Suscripción cancelada: Id={SusId}", suscripcionId);
            TempData["Success"] = "Suscripción cancelada.";
            return RedirectToAction(nameof(Suscripcion), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearSuscripcion(
            int id,
            NuevaSuscripcionInputModel model)
        {
            if (id != model.ComercioId)
                return BadRequest();

            var plan = PlanHelper.NormalizarPlan(model.Plan);
            if (plan is null)
                ModelState.AddModelError(nameof(model.Plan), "Plan inválido.");

            if (!ModelState.IsValid)
                return await SuscripcionView(id);

            var comercio = await _context.Comercios
                .Include(c => c.Suscripciones)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comercio is null)
                return NotFound();

            _context.Suscripciones.Add(new Suscripcion
            {
                ComercioId = id,
                Plan = Enum.Parse<PlanSuscripcion>(plan!),
                Estado = EstadoSuscripcion.Activa,
                FechaInicio = model.FechaInicio,
                MontoMensual = model.MontoMensual,
                FechaProximoVencimiento = model.FechaInicio.AddDays(30),
                EstadoPago = EstadoPagoSuscripcion.Pendiente
            });

            if (comercio.EstadoAdministrativo == EstadoAdministrativo.Activo)
                comercio.EstadoAdministrativo = EstadoAdministrativo.PendientePago;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Nueva suscripción: ComercioId={Id}, Plan={Plan}", id, plan);
            TempData["Success"] = "Nueva suscripción creada.";
            return RedirectToAction(nameof(Suscripcion), new { id });
        }

        [HttpGet]
        public async Task<IActionResult> RegistrarPago(int id)
        {
            var comercio = await _registroPagoService
                .ObtenerComercioConSuscripcionesAsync(id);
            if (comercio is null) return NotFound();

            var suscripcion = _gestionSuscripciones.ObtenerSuscripcionActual(
                comercio.Suscripciones, DateTime.Today);
            if (suscripcion is null)
            {
                TempData["Error"] = "El comercio no tiene una suscripción para cobrar.";
                return RedirectToAction(nameof(Suscripcion), new { id });
            }

            return View(new RegistrarPagoInputModel
            {
                ComercioId = comercio.Id,
                SuscripcionId = suscripcion.Id,
                ComercioNombre = comercio.Nombre,
                Plan = suscripcion.Plan.ToString(),
                Monto = suscripcion.MontoMensual,
                FechaPago = DateTime.Today
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarPago(
            int id,
            RegistrarPagoInputModel model)
        {
            if (id != model.ComercioId) return BadRequest();

            if (model.FechaPago == default)
                ModelState.AddModelError(nameof(model.FechaPago), "Ingresá la fecha del pago.");
            else if (model.FechaPago.Date > DateTime.Today)
                ModelState.AddModelError(nameof(model.FechaPago), "La fecha del pago no puede ser futura.");

            var (comercio, suscripcion) =
                await _registroPagoService.ObtenerComercioYSuscripcionAsync(
                    id, model.SuscripcionId);

            if (comercio is null || suscripcion is null) return NotFound();

            if (!ModelState.IsValid)
            {
                model.ComercioNombre = comercio.Nombre;
                model.Plan = suscripcion.Plan.ToString();
                return View(model);
            }

            await _registroPagoService.RegistrarAsync(comercio, suscripcion,
                new RegistroPagoCommand
                {
                    Monto = model.Monto,
                    FechaPago = model.FechaPago,
                    Observacion = TextHelper.NormalizarOpcional(model.Observacion)
                });

            _logger.LogInformation("Pago registrado: ComercioId={Id}, Monto={Monto}", id, model.Monto);
            TempData["Success"] = "Pago registrado. La suscripción se extendió 30 días.";
            return RedirectToAction(nameof(Suscripcion), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcarPendientePago(int id)
        {
            var resultado = await ObtenerComercioYSuscripcionActual(id);
            if (resultado.Comercio is null) return NotFound();

            if (resultado.Suscripcion is null)
            {
                TempData["Error"] = "El comercio no tiene una suscripción para actualizar.";
                return RedirectToAction(nameof(Details), new { id });
            }

            resultado.Suscripcion.EstadoPago = EstadoPagoSuscripcion.Pendiente;
            if (resultado.Comercio.EstadoAdministrativo != EstadoAdministrativo.Inhabilitado)
                resultado.Comercio.EstadoAdministrativo = EstadoAdministrativo.PendientePago;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Comercio marcado pendiente de pago: Id={Id}", id);
            TempData["Success"] = "El comercio quedó pendiente de pago.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcarAlDia(int id)
        {
            var resultado = await ObtenerComercioYSuscripcionActual(id);
            if (resultado.Comercio is null) return NotFound();

            if (resultado.Suscripcion is null)
            {
                TempData["Error"] = "El comercio no tiene una suscripción para actualizar.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var hoy = DateTime.Today;
            resultado.Suscripcion.EstadoPago = EstadoPagoSuscripcion.AlDia;
            resultado.Suscripcion.Estado = EstadoSuscripcion.Activa;
            if (resultado.Suscripcion.FechaProximoVencimiento.Date < hoy)
                resultado.Suscripcion.FechaProximoVencimiento = hoy.AddDays(30);

            if (resultado.Comercio.EstadoAdministrativo == EstadoAdministrativo.PendientePago)
                resultado.Comercio.EstadoAdministrativo = EstadoAdministrativo.Activo;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Comercio marcado al día: Id={Id}", id);
            TempData["Success"] = "La suscripción fue marcada al día sin registrar un cobro.";
            return RedirectToAction(nameof(Details), new { id });
        }

        private async Task<IActionResult> SuscripcionView(int id)
        {
            var comercio = await _context.Comercios
                .AsNoTracking()
                .AsSplitQuery()
                .Include(c => c.Titular)
                .Include(c => c.Suscripciones)
                .Include(c => c.Pagos)
                    .ThenInclude(p => p.Suscripcion)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comercio is null) return NotFound();

            comercio.Suscripciones = comercio.Suscripciones
                .OrderByDescending(s => s.FechaInicio).ToList();
            comercio.Pagos = comercio.Pagos
                .OrderByDescending(p => p.FechaPago).ThenByDescending(p => p.Id).ToList();

            var (suscripcion, estadoPago) =
                _gestionSuscripciones.ObtenerEstadoCompleto(comercio.Suscripciones, DateTime.Today);

            ViewData["SuscripcionActual"] = suscripcion;
            ViewData["EstadoPago"] = estadoPago;

            return View("Suscripcion", new ComercioAdministracionViewModel
            {
                Comercio = comercio,
                SuscripcionActual = suscripcion,
                EstadoPagoEfectivo = estadoPago
            });
        }
    }
}
