using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitCheckWebApp.Controllers
{
    public class AdminController : Controller
    {
        [Authorize(Roles = "admin")]
        public IActionResult AdminHome() => View();



        [Authorize(Roles = "admin")]
        public IActionResult AdminPayment() => View();

    }
}
