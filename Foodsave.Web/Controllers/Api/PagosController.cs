using Foodsave.Web.Data;
using Foodsave.Web.Helpers;
using Foodsave.Web.Models;
using Foodsave.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Foodsave.Web.Controllers.Api
{
    // Endpoint para registrar pagos de suscripciones.
    // Delega la lógica de negocio a RegistroPagoService.
    [Authorize]
    [ApiController]
    [Route("api/pagos")]
    [Produces("application/json")]
    [IgnoreAntiforgeryToken]
    public class PagosController : ControllerBase
    {
        private readonly RegistroPagoService _registroPagoService;
        private readonly ILogger<PagosController> _logger;

        public PagosController(
            RegistroPagoService registroPagoService,
            ILogger<PagosController> logger)
        {
            _registroPagoService = registroPagoService;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Register([FromBody] RegistrarPagoInputModel model)
        {
            // El controlador valida la entrada y delega el registro al servicio.
            if (!ModelState.IsValid)
                return BadRequest(ApiError.Validation(ModelState));

            if (model.FechaPago.Date > DateTime.Today)
                return BadRequest(ApiError.BadRequest(
                    "La fecha del pago no puede ser futura."));

            var (comercio, suscripcion) =
                await _registroPagoService.ObtenerComercioYSuscripcionAsync(
                    model.ComercioId, model.SuscripcionId);

            if (comercio is null || suscripcion is null)
                return NotFound(ApiError.NotFound(
                    "Comercio o suscripción no encontrados."));

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
                "API: Pago registrado ComercioId={Id} Monto={Monto}",
                model.ComercioId, model.Monto);

            return Created(string.Empty, new
            {
                message = "Pago registrado y suscripción actualizada.",
                comercioId = comercio.Id,
                suscripcionId = suscripcion.Id,
                monto = model.Monto,
                fechaPago = model.FechaPago
            });
        }
    }
}
