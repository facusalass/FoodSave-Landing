using Foodsave.Web.Data;
using Foodsave.Web.Helpers;
using Foodsave.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace Foodsave.Web.Controllers.Api
{
    [Authorize]
    [ApiController]
    [Route("api/solicitudes")]
    [Produces("application/json")]
    public class SolicitudesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SolicitudesController> _logger;

        public SolicitudesController(
            ApplicationDbContext context,
            ILogger<SolicitudesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<SolicitudDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var solicitudes = await _context.SolicitudesComercio
                .AsNoTracking()
                .OrderBy(s => s.Estado != EstadoSolicitud.Pendiente)
                .ThenByDescending(s => s.FechaSolicitud)
                .ToListAsync();

            return Ok(solicitudes.Select(s => s.ToDto()));
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(SolicitudDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var solicitud = await _context.SolicitudesComercio
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);

            if (solicitud is null)
                return NotFound(ApiError.NotFound("Solicitud no encontrada."));

            return Ok(solicitud.ToDto());
        }

        [AllowAnonymous]
        [HttpPost]
        [EnableRateLimiting("SolicitudForm")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] SolicitudComercioInputModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiError.Validation(ModelState));

            var solicitud = model.ToEntity();
            _context.SolicitudesComercio.Add(solicitud);
            await _context.SaveChangesAsync();

            _logger.LogInformation("API: Solicitud recibida {Nombre}", solicitud.NombreComercio);

            return CreatedAtAction(
                nameof(GetById),
                new { id = solicitud.Id },
                solicitud.ToDto());
        }

        [HttpPatch("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Review(
            int id,
            [FromBody] ReviewSolicitudInput model)
        {
            if (!Enum.TryParse<EstadoSolicitud>(model.Estado, out var estado))
                return BadRequest(ApiError.BadRequest(
                    "Estado inválido. Usar: Aceptada, Rechazada."));

            var solicitud = await _context.SolicitudesComercio.FindAsync(id);
            if (solicitud is null)
                return NotFound(ApiError.NotFound("Solicitud no encontrada."));

            if (solicitud.Estado != EstadoSolicitud.Pendiente)
                return BadRequest(ApiError.BadRequest("La solicitud ya fue revisada."));

            var observacion = TextHelper.NormalizarOpcional(model.Observacion);
            if (observacion?.Length > 1000)
                return BadRequest(ApiError.BadRequest(
                    "La observación no puede superar los 1000 caracteres."));

            solicitud.Estado = estado;
            solicitud.FechaRevision = DateTime.UtcNow;
            solicitud.ObservacionAdmin = observacion;
            await _context.SaveChangesAsync();

            _logger.LogInformation("API: Solicitud Id={Id} {Estado}", id, estado);

            return Ok(solicitud.ToDto());
        }
    }
}
