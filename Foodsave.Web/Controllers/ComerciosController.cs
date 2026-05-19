using Microsoft.AspNetCore.Mvc;

namespace Foodsave.Web.Controllers
{
    public class ComerciosController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
