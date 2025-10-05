using FitCheckWebApp.DataAccess;
using FitCheckWebApp.Models;
using FitCheckWebApp.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FitCheckWebApp.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            // TODO: Add authentication logic here
            return RedirectToAction("Index", "Home");
        }


        public IActionResult Register()
        {
            return View();
        }


        [HttpPost]
        [ActionName("Register")]
        public IActionResult RegisterPost(RegistrationViewModel model)
        {

            if (!ModelState.IsValid)
            {
                return View(model); 
            }

            var account = new Account
            {

                Username = model.Username,
                Email = model.Email,
                PasswordHash = Helpers.Helpers.HashingPassword(model.Password!),
                MembershipID = Helpers.Helpers.MapMemberShipToID(model.MembershipPlan)

            };

            AccountManager.PostAccount(account);


            TempData["SuccessMessage"] = "Registration successful! Please log in.";
            return RedirectToAction("Login");
        }


        public IActionResult Membership()
        {
            return View();
        }
    }
}