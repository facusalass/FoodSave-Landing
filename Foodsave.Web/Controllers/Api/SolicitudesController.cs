using Foodsave.Web.Data;
using Foodsave.Web.Helpers;
using Foodsave.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace Foodsave.Web.Controllers.Api
{
    // Controlador REST para administrar las solicitudes enviadas por comercios.
    [Authorize]
    [ApiController]
    // Todas sus acciones comienzan con /api/solicitudes.
    [Route("api/solicitudes")]
    // La API responde datos en formato JSON, no vistas HTML.
    [Produces("application/json")]
    [IgnoreAntiforgeryToken]
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
            // Consulta las solicitudes sin modificar la base de datos.
            var solicitudes = await _context.SolicitudesComercio
                .AsNoTracking()
                .OrderBy(s => s.Estado != EstadoSolicitud.Pendiente)
                .ThenByDescending(s => s.FechaSolicitud)
                .ToListAsync();

            // 200 OK: devuelve al cliente una lista JSON mediante DTOs.
            return Ok(solicitudes.Select(s => s.ToDto()));
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(SolicitudDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            // El id proviene de la URL: GET /api/solicitudes/{id}.
            var solicitud = await _context.SolicitudesComercio
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);

            if (solicitud is null)
                // 404 cuando el recurso solicitado no existe.
                return NotFound(ApiError.NotFound("Solicitud no encontrada."));

            // 200 OK con los datos del recurso solicitado.
            return Ok(solicitud.ToDto());
        }

        [AllowAnonymous]
        [HttpPost]
        [EnableRateLimiting("SolicitudForm")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] SolicitudComercioInputModel model)
        {
            // [FromBody] convierte el JSON recibido en un modelo de C#.
            if (!ModelState.IsValid)
                // 400 cuando faltan datos o no cumplen las validaciones.
                return BadRequest(ApiError.Validation(ModelState));

            // Se transforma el modelo de entrada en entidad y se guarda en la BD.
            var solicitud = model.ToEntity();
            _context.SolicitudesComercio.Add(solicitud);
            await _context.SaveChangesAsync();

            _logger.LogInformation("API: Solicitud recibida {Nombre}", solicitud.NombreComercio);

            // 201 Created: informa que se creó el recurso y dónde consultarlo.
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
            // PATCH actualiza solo parte del recurso: estado y observación.
            if (!Enum.TryParse<EstadoSolicitud>(model.Estado, out var estado) ||
                (estado != EstadoSolicitud.Aceptada && estado != EstadoSolicitud.Rechazada))
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
