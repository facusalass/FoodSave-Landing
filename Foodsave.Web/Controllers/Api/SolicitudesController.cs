using Foodsave.Web.Data;
using Foodsave.Web.Helpers;
using Foodsave.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace Foodsave.Web.Controllers.Api
{
    // CRUD de solicitudes de comercios.
    // El POST para crear solicitudes es público ([AllowAnonymous]), el resto requiere auth.
    [Authorize]
    [ApiController]
    [Route("api/solicitudes")]
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
        [ProducesResponseType(typeof(RespuestaPaginada<SolicitudDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        // Ejemplo: GET /api/solicitudes?pagina=2&registrosPorPagina=10
        // [FromQuery] toma ambos valores desde la URL. Si no se envían,
        // se utilizan la primera página y diez registros por defecto.
        public async Task<IActionResult> GetAll(
            [FromQuery] int pagina = 1,
            [FromQuery] int registrosPorPagina = 10)
        {
            // Se validan los parámetros para evitar páginas inexistentes,
            // tamaños negativos o respuestas excesivamente grandes.
            if (pagina < 1)
                return BadRequest(ApiError.BadRequest(
                    "La página debe ser mayor o igual a 1."));

            if (registrosPorPagina is < 1 or > 100)
                return BadRequest(ApiError.BadRequest(
                    "Los registros por página deben estar entre 1 y 100."));

            // Fórmula del paginado: (página - 1) * tamaño.
            // Ejemplo: página 3 de 10 registros omite los primeros 20.
            var registrosAOmitir = (long)(pagina - 1) * registrosPorPagina;
            if (registrosAOmitir > int.MaxValue)
                return BadRequest(ApiError.BadRequest(
                    "El número de página solicitado es demasiado grande."));

            // La consulta se ordena antes de aplicar Skip y Take para que
            // los registros permanezcan en una página estable.
            var consulta = _context.SolicitudesComercio
                .AsNoTracking()
                .OrderBy(s => s.Estado != EstadoSolicitud.Pendiente)
                .ThenByDescending(s => s.FechaSolicitud);

            // CountAsync obtiene el total antes de recortar la consulta.
            // Ese valor permite calcular cuántas páginas existen.
            var totalRegistros = await consulta.CountAsync(
                HttpContext.RequestAborted);
            var totalPaginas = (int)Math.Ceiling(
                totalRegistros / (double)registrosPorPagina);

            // Skip omite los registros de páginas anteriores y Take limita
            // la cantidad devuelta en la página actual.
            var solicitudes = await consulta
                .Skip((int)registrosAOmitir)
                .Take(registrosPorPagina)
                .ToListAsync(HttpContext.RequestAborted);

            // La respuesta incluye los elementos de esta página y metadatos
            // para que el cliente pueda construir Anterior/Siguiente.
            return Ok(new RespuestaPaginada<SolicitudDto>
            {
                Items = solicitudes.Select(s => s.ToDto()).ToList(),
                PaginaActual = pagina,
                RegistrosPorPagina = registrosPorPagina,
                TotalRegistros = totalRegistros,
                TotalPaginas = totalPaginas
            });
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

        // Endpoint público: cualquiera puede enviar una solicitud sin estar logueado.
        [AllowAnonymous]
        [HttpPost]
        [EnableRateLimiting("SolicitudForm")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] SolicitudComercioInputModel model)
        {
            // [FromBody] convierte el JSON recibido directo a un objeto C#.
            if (!ModelState.IsValid)
                return BadRequest(ApiError.Validation(ModelState));

            // ToEntity() mapea el input model a la entidad de BD.
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
