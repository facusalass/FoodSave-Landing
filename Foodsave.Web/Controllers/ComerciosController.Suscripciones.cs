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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcarPendientePago(int id)
        {
            var resultado = await ObtenerComercioYSuscripcionActual(id);
            if (resultado.Comercio is null)
                return NotFound();

            if (resultado.Suscripcion is null)
            {
                TempData["Error"] =
                    "El comercio no tiene una suscripción para actualizar.";
                return RedirectToAction(nameof(Details), new { id });
            }

            resultado.Suscripcion.EstadoPago = EstadoPagoSuscripcion.Pendiente;
            if (resultado.Comercio.EstadoAdministrativo != EstadoAdministrativo.Inhabilitado)
            {
                resultado.Comercio.EstadoAdministrativo = EstadoAdministrativo.PendientePago;
            }

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
            if (resultado.Comercio is null)
                return NotFound();

            if (resultado.Suscripcion is null)
            {
                TempData["Error"] =
                    "El comercio no tiene una suscripción para actualizar.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var hoy = DateTime.Today;
            resultado.Suscripcion.EstadoPago = EstadoPagoSuscripcion.AlDia;
            resultado.Suscripcion.Estado = EstadoSuscripcion.Activa;
            if (resultado.Suscripcion.FechaProximoVencimiento.Date < hoy)
            {
                resultado.Suscripcion.FechaProximoVencimiento =
                    hoy.AddMonths(1);
            }

            if (resultado.Comercio.EstadoAdministrativo == EstadoAdministrativo.PendientePago)
            {
                resultado.Comercio.EstadoAdministrativo = EstadoAdministrativo.Activo;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Comercio marcado al día: Id={Id}", id);
            TempData["Success"] =
                "La suscripción fue marcada al día sin registrar un cobro.";

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarSuscripcion(
            int id,
            ActualizarSuscripcionInputModel model)
        {
            if (id != model.ComercioId)
                return BadRequest();

            var plan = PlanHelper.NormalizarPlan(model.Plan);
            if (plan is null)
            {
                ModelState.AddModelError(
                    nameof(model.Plan),
                    "Seleccioná un plan válido.");
            }

            if (model.FechaProximoVencimiento == default)
            {
                ModelState.AddModelError(
                    nameof(model.FechaProximoVencimiento),
                    "Ingresá el próximo vencimiento.");
            }

            if (!ModelState.IsValid)
                return await DetailsView(id);

            var suscripcion = await _context.Suscripciones
                .FirstOrDefaultAsync(
                    s => s.Id == model.SuscripcionId &&
                         s.ComercioId == model.ComercioId);
            if (suscripcion is null)
                return NotFound();

            suscripcion.Plan = Enum.Parse<PlanSuscripcion>(plan!);
            suscripcion.MontoMensual = model.MontoMensual;
            suscripcion.FechaProximoVencimiento =
                model.FechaProximoVencimiento.Date;
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Suscripción actualizada: ComercioId={ComercioId}, Plan={Plan}",
                model.ComercioId, plan);

            TempData["Success"] = "La suscripción fue actualizada.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpGet]
        public async Task<IActionResult> RegistrarPago(int id)
        {
            var comercio = await _registroPagoService
                .ObtenerComercioConSuscripcionesAsync(id);
            if (comercio is null)
                return NotFound();

            var suscripcion = _gestionSuscripciones.ObtenerSuscripcionActual(
                comercio.Suscripciones, DateTime.Today);
            if (suscripcion is null)
            {
                TempData["Error"] =
                    "El comercio no tiene una suscripción para cobrar.";
                return RedirectToAction(nameof(Details), new { id });
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
            if (id != model.ComercioId)
                return BadRequest();

            if (model.FechaPago == default)
            {
                ModelState.AddModelError(
                    nameof(model.FechaPago),
                    "Ingresá la fecha del pago.");
            }
            else if (model.FechaPago.Date > DateTime.Today)
            {
                ModelState.AddModelError(
                    nameof(model.FechaPago),
                    "La fecha del pago no puede ser futura.");
            }

            var (comercio, suscripcion) =
                await _registroPagoService.ObtenerComercioYSuscripcionAsync(
                    id, model.SuscripcionId);

            if (comercio is null || suscripcion is null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                model.ComercioNombre = comercio.Nombre;
                model.Plan = suscripcion.Plan.ToString();
                return View(model);
            }

            await _registroPagoService.RegistrarAsync(
                comercio,
                suscripcion,
                new RegistroPagoCommand
                {
                    Monto = model.Monto,
                    FechaPago = model.FechaPago,
                    Observacion = TextHelper.NormalizarOpcional(model.Observacion)
                });

            _logger.LogInformation(
                "Pago registrado: ComercioId={ComercioId}, Monto={Monto}",
                id, model.Monto);

            TempData["Success"] =
                "Pago registrado y suscripción actualizada.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
