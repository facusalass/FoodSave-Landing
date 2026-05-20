using Foodsave.Web.Data;
using Microsoft.AspNetCore.Mvc;

namespace Foodsave.Web.Controllers
{
    public class ComerciosController : Controller
    {
        public IActionResult Index()
        {
            var comercios = FoodSaveMockData.ObtenerComercios();

            return View(comercios);
        }

        public IActionResult Details(int id)
        {
            var comercios = FoodSaveMockData.ObtenerComercios();
            var comercio = comercios.FirstOrDefault(c => c.Id == id);

            if (comercio == null)
            {
                return NotFound();
            }

            return View(comercio);
        }
    }
}
