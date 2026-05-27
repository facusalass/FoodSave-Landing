using Foodsave.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; 
using System.Linq;

namespace Foodsave.Web.Controllers
{
    public class ComerciosController : Controller
    {
        // 1. Variable privada para guardar la sesión de la base de datos
        private readonly ApplicationDbContext _context;

        // 2. El Constructor: C# nos inyecta la base de datos automáticamente acá
        public ComerciosController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // 3. Vamos a la tabla real de Comercios.
            // Usamos Include para traer también la info del Titular y sus Suscripciones.
            var comercios = _context.Comercios
                .Include(c => c.Titular)
                .Include(c => c.Suscripciones)
                .ToList();

            return View(comercios);
        }

        public IActionResult Details(int id)
        {
            // 4. Buscamos en la base de datos el comercio específico con sus relaciones.
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
    }
}