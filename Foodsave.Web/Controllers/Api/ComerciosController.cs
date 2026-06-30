using Foodsave.Web.Data;
using Foodsave.Web.Helpers;
using Foodsave.Web.Models;
using Foodsave.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Foodsave.Web.Controllers.Api
{
    [Authorize]
    [ApiController]
    [Route("api/comercios")]
    [Produces("application/json")]
    public class ApiComerciosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly GestionSuscripcionesService _gestionSuscripciones;
        private readonly ILogger<ApiComerciosController> _logger;

        public ApiComerciosController(
            ApplicationDbContext context,
            GestionSuscripcionesService gestionSuscripciones,
            ILogger<ApiComerciosController> logger)
        {
            _context = context;
            _gestionSuscripciones = gestionSuscripciones;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<ComercioDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var comercios = await _context.Comercios
                .AsNoTracking()
                .Include(c => c.Titular)
                .Include(c => c.Suscripciones)
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            return Ok(comercios.Select(c => c.ToDto()));
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ComercioDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
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
                return NotFound(ApiError.NotFound("Comercio no encontrado."));

            return Ok(comercio.ToDto());
        }

        [HttpPost]
        [ProducesResponseType(typeof(ComercioDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] SolicitudComercioInputModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiError.Validation(ModelState));

            var hoy = DateTime.Today;
            var comercio = new Comercio
            {
                Nombre = model.NombreComercio.Trim(),
                Rubro = model.Rubro.Trim(),
                Direccion = TextHelper.NormalizarOpcional(model.Direccion) ?? "",
                Telefono = model.TelefonoComercio.Trim(),
                EstadoAdministrativo = EstadoAdministrativo.PendientePago,
                Titular = new Titular
                {
                    Nombre = model.NombreTitular.Trim(),
                    Apellido = TextHelper.NormalizarOpcional(model.ApellidoTitular) ?? "",
                    Email = model.EmailTitular.Trim().ToLowerInvariant(),
                    Telefono = TextHelper.NormalizarOpcional(model.TelefonoTitular) ?? ""
                },
                Suscripciones =
                {
                    new Suscripcion
                    {
                        Plan = PlanSuscripcion.Estandar,
                        Estado = EstadoSuscripcion.Activa,
                        FechaInicio = hoy,
                        FechaFin = hoy.AddMonths(1),
                        MontoMensual = 0,
                        FechaProximoVencimiento = hoy,
                        EstadoPago = EstadoPagoSuscripcion.Pendiente
                    }
                }
            };

            _context.Comercios.Add(comercio);
            await _context.SaveChangesAsync();

            _logger.LogInformation("API: Comercio creado Id={Id}", comercio.Id);

            return CreatedAtAction(
                nameof(GetById), new { id = comercio.Id }, comercio.ToDto());
        }

        [HttpPatch("{id:int}/estado")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateEstado(
            int id,
            [FromBody] UpdateEstadoInput model)
        {
            if (!Enum.TryParse<EstadoAdministrativo>(model.Estado, out var estado))
                return BadRequest(ApiError.BadRequest(
                    "Estado inválido. Usar: Activo, Inhabilitado, PendientePago."));

            var comercio = await _context.Comercios.FindAsync(id);
            if (comercio is null)
                return NotFound(ApiError.NotFound("Comercio no encontrado."));

            comercio.EstadoAdministrativo = estado;
            await _context.SaveChangesAsync();

            _logger.LogInformation("API: Estado comercio Id={Id} actualizado a {Estado}", id, estado);

            return Ok(new { message = $"Comercio actualizado a {estado}." });
        }

        [HttpGet("{id:int}/pagos")]
        [ProducesResponseType(typeof(List<PagoDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPagos(int id)
        {
            var pagos = await _context.Pagos
                .AsNoTracking()
                .Include(p => p.Suscripcion)
                .Where(p => p.ComercioId == id)
                .OrderByDescending(p => p.FechaPago)
                .ToListAsync();

            return Ok(pagos.Select(p => p.ToDto()));
        }
    }
}
