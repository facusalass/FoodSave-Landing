using Foodsave.Web.Models;
using Foodsave.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Foodsave.Web.Controllers.Api
{
    // Controlador REST que expone indicadores generales del sistema.
    [Authorize]
    [ApiController]
    // Ruta para consultar estadísticas: GET /api/estadisticas.
    [Route("api/estadisticas")]
    // El resultado se devuelve como JSON.
    [Produces("application/json")]
    [IgnoreAntiforgeryToken]
    public class ApiEstadisticasController : ControllerBase
    {
        private readonly EstadisticasService _estadisticasService;

        public ApiEstadisticasController(EstadisticasService estadisticasService)
        {
            _estadisticasService = estadisticasService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(EstadisticasViewModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get()
        {
            // El servicio concentra el cálculo y el controlador devuelve 200 OK.
            var model = await _estadisticasService.ObtenerEstadisticasAsync();
            return Ok(model);
        }
    }
}
