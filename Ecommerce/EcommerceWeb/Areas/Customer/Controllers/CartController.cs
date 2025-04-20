using Microsoft.AspNetCore.Mvc;

namespace EcommerceWeb.Areas.Customer.Controllers
{

    [Area("customer")]
    public class CartController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}