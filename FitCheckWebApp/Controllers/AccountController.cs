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
        public IActionResult Login(LoginViewModel loginModel)
        {

            var account = AccountManager.FindByEmail(loginModel.Email!);

            if (account != null && Helpers.Helpers.verifyPassword(loginModel.Password!, account.PasswordHash!))
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError("", "Invalid email or password");
                return View();
            }

            
        }


        public IActionResult Register()
        {
            return View();
        }


        [HttpPost]
        [ActionName("Register")]
        public IActionResult RegisterPost(RegistrationViewModel registerModel)
        {

            if (!ModelState.IsValid)
            {
                return View(registerModel); 
            }

            var existingAccount = AccountManager.FindByEmail(registerModel.Email!);

            if (existingAccount != null)
            {

                ModelState.AddModelError("Email", "Account already exists.");
                return View(registerModel);
            }

            var account = new Account
            {

                Username = registerModel.Username,
                Email = registerModel.Email,
                PasswordHash = Helpers.Helpers.HashingPassword(registerModel.Password!),
                MembershipID = Helpers.Helpers.MapMemberShipToID(registerModel.MembershipPlan)

            };

            AccountManager.PostAccount(account);


            TempData["SuccessMessage"] = "Registration successful! Please log in.";
            return RedirectToAction("Login");
        }


        public IActionResult Membership()
        {
            return View();
        }

        public IActionResult TermsAndConditions()
        {
            return View();
        }

        public IActionResult PrivacyPolicy()
        {
            return View();
        }
    }
}