using FitCheckWebApp.DataAccess;
using FitCheckWebApp.Models;
using FitCheckWebApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using static FitCheckWebApp.Helpers.Helpers;

namespace FitCheckWebApp.Controllers
{
    public class AccountController : Controller
    {
        // ===== LOGIN =====
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            var account = AccountManager.FindByEmail(model.Email!);

            if (account != null && verifyPassword(model.Password!, account.PasswordHash!))
                return RedirectToAction("Index", "Home");

            ModelState.AddModelError("", "Invalid email or password");
            return View(model);
        }

        // ===== REGISTER =====
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegistrationViewModel model)
        {
            model.Age = CalculateAge(model);

            if (!ModelState.IsValid)
                return View(model);

            if (AccountManager.FindByEmail(model.Email!) != null)
            {
                ModelState.AddModelError("Email", "Account already exists.");
                return View(model);
            }

            var account = new Account
            {
                Username = model.Username,
                Email = model.Email,
                PasswordHash = HashingPassword(model.Password!),
                MembershipID = MapMemberShipToID(model.MembershipPlan)
            };

            AccountManager.PostAccount(account);

            TempData["SuccessMessage"] = "Registration successful! Please log in.";
            return RedirectToAction("Login");
        }

        // ===== PAGES =====
        public IActionResult Membership() => View();

        public IActionResult TermsAndConditions() => View();

        public IActionResult PrivacyPolicy() => View();
    }
}
