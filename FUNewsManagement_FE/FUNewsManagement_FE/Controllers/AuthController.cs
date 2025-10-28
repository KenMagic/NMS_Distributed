using Microsoft.AspNetCore.Mvc;
using FUNewsManagement_FE.Services;

namespace FUNewsManagement_FE.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var success = await _authService.LoginAsync(email, password);

            if (success)
                return RedirectToAction("Index", "Home");

            TempData["Error"] = "Login failed! Please check your credentials.";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
