using Foodsave.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Foodsave.Web.Controllers
{
    [Authorize]
    public class EstadisticasController : Controller
    {
        private readonly EstadisticasService _estadisticasService;

        public EstadisticasController(EstadisticasService estadisticasService)
        {
            _estadisticasService = estadisticasService;
            ViewData["ActivePage"] = "Estadisticas";
        }

        public async Task<IActionResult> Index()
        {
            var model = await _estadisticasService.ObtenerEstadisticasAsync();
            return View(model);
        }
    }
}
