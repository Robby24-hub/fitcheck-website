using Microsoft.AspNetCore.Mvc;

namespace FitCheckWebApp.Controllers
{
    public class AccountController : Controller
    {
        // Login page (GET)
        public IActionResult Login()
        {
            return View();
        }

        // Login form submission (POST)
        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            // TODO: Add authentication logic here
            return RedirectToAction("Index", "Home");
        }

        // Registration page (GET)
        public IActionResult Register()
        {
            return View();
        }

        // Registration form submission (POST)
        [HttpPost]
        [ActionName("Register")]
        public IActionResult RegisterPost(string email, string password, string confirmPassword)
        {
            // TODO: Add registration logic here
            return RedirectToAction("Login");
        }

        // Membership page
        public IActionResult Membership()
        {
            return View();
        }
    }
}