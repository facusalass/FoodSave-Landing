using Foodsave.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Foodsave.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            ViewData["ActivePage"] = "Home";
            return View();
        }

        public IActionResult Privacy()
        {
            ViewData["ActivePage"] = "Home";
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Route("/404")]
        public IActionResult NotFoundPage()
        {
            return View("NotFound");
        }
    }
}
