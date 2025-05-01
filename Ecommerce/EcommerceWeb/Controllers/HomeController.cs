using Microsoft.AspNetCore.Mvc;

namespace EcommerceWeb.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Terms()
        {
            return View();
        }
    }
}
