using Microsoft.AspNetCore.Mvc;

namespace FUNewsManagement_FE.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
