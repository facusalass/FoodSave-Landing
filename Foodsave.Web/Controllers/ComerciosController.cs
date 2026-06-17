using Foodsave.Web.Data;
using Foodsave.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Foodsave.Web.Controllers
{
    [Authorize]
    public class ComerciosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ComerciosController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var comercios = _context.Comercios
                .Include(c => c.Titular)
                .Include(c => c.Suscripciones)
                .ToList();

            return View(comercios);
        }

        public IActionResult Details(int id)
        {
            var comercio = _context.Comercios
                .Include(c => c.Titular)
                .Include(c => c.Suscripciones)
                .FirstOrDefault(c => c.Id == id);

            if (comercio == null)
            {
                return NotFound();
            }

            return View(comercio);
        }

        public IActionResult Create()
        {
            return View(new Comercio());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Comercio comercio, string planInicial)
        {
            if (!ModelState.IsValid)
            {
                return View(comercio);
            }

            var plan = string.IsNullOrWhiteSpace(planInicial) ? "Estandar" : planInicial;

            comercio.Suscripciones = new List<Suscripcion>
            {
                new Suscripcion
                {
                    Plan = plan,
                    Estado = "Activa",
                    FechaInicio = DateTime.Today,
                    FechaFin = DateTime.Today.AddMonths(1)
                }
            };

            _context.Comercios.Add(comercio);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}
