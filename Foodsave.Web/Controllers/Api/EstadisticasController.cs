using Foodsave.Web.Models;
using Foodsave.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Foodsave.Web.Controllers.Api
{
    [Authorize]
    [ApiController]
    [Route("api/estadisticas")]
    [Produces("application/json")]
    public class EstadisticasController : ControllerBase
    {
        private readonly EstadisticasService _estadisticasService;

        public EstadisticasController(EstadisticasService estadisticasService)
        {
            _estadisticasService = estadisticasService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(EstadisticasViewModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get()
        {
            var model = await _estadisticasService.ObtenerEstadisticasAsync();
            return Ok(model);
        }
    }
}
