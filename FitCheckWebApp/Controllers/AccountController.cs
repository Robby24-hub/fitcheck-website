using System.Security.Claims;
using FitCheckWebApp.DataAccess;
using FitCheckWebApp.Models;
using FitCheckWebApp.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
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
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            var account = AccountManager.FindByEmail(model.Email!);

            if (account != null && verifyPassword(model.Password!, account.PasswordHash!))
            {
                
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
                    new Claim(ClaimTypes.Name, account.Username ?? ""),
                    new Claim(ClaimTypes.Email, account.Email ?? ""),
                    new Claim(ClaimTypes.Role, account.Role)
                };

                
                var claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true, 
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1)
                };

                
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                return RedirectToAction("UserHome");
            }

            ModelState.AddModelError("", "Invalid email or password");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
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
                FirstName = model.FirstName,
                LastName = model.LastName
            };

            AccountManager.PostAccount(account);

            return RedirectToAction("Login");
        }

        // ===== PAGES =====

        public IActionResult TermsAndConditions() => View();

        public IActionResult PrivacyPolicy() => View();


        [Authorize]
        public IActionResult PaymentMethod() => View();

        [Authorize]
        public IActionResult UserHome()
        {
            if (!User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            int accountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var account = AccountManager.FindById(accountId); 
            var transaction = TransactionManager.FindById(accountId); 

            if (account == null)
                return RedirectToAction("Login", "Account");

            if (transaction == null)
            {
                return RedirectToAction("Membership", "Transaction");
            }

            MembershipPassViewModel model = new MembershipPassViewModel
            {
                FullName = $"{account.FirstName} {account.LastName}",
                MemberID = account.MemberID,
                MembershipPlan = account.MembershipPlan.ToString(),
                TransactionDate = transaction!.TransactionDate,
                EndDate = transaction.EndDate,
                Status = transaction.Status.ToString()
            };


            return View(model);
        }


        [Authorize]
        public IActionResult ClassesUser() => View();


    }
}
