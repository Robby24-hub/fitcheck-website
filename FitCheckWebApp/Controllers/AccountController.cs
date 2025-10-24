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
    [NoCache]
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

                if (account.Role == "admin")
                {
                    return RedirectToAction("AdminHome", "Admin");
                }

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
            if (!Helpers.Helpers.IsBirthdayValid(model.Birthday))
            {
                ModelState.AddModelError("Birthday", "Birthday cannot be a future date.");
                return View(model);
            }

            model.Age = Helpers.Helpers.CalculateAge(model);

            if (model.Age < 0)
            {
                ModelState.AddModelError("Age", "Invalid age calculated from birthday.");
                return View(model);
            }

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
            TransactionManager.ExpireOldMemberships();

            if (!User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            int accountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var account = AccountManager.FindById(accountId);
            var transaction = TransactionManager.FindLatestActiveByAccount(accountId);


            if (account == null)
                return RedirectToAction("Login", "Account");

            MembershipPassViewModel model = new MembershipPassViewModel
            {
                FullName = $"{account.FirstName} {account.LastName}",
                MemberID = account.MemberID,

            };

            if (transaction != null)
            {
                model.MembershipPlan = account.MembershipPlan.ToString();
                model.TransactionDate = transaction!.TransactionDate;
                model.EndDate = transaction.EndDate;
                model.Status = transaction.Status.ToString();

                model.PaymentStatus = transaction.Status == TransactionStatus.Active ||
                          transaction.Status == TransactionStatus.Expired
                          ? "Paid"
                          : "Unpaid";

                if (transaction.EndDate <= DateTime.Now)
                {
                    model.WarningMessage = "Your membership has expired. Please renew to continue access.";
                }
                else if ((transaction.EndDate - DateTime.Now).TotalDays <= 3)
                {
                    model.WarningMessage = $"Your membership will expire in {(transaction.EndDate - DateTime.Now).Days} days.";
                }
                else
                {
                    model.WarningMessage = null;
                }

            }
            else
            {
                model.MembershipPlan = "N/A";
                model.TransactionDate = null;
                model.EndDate = null;
                model.Status = "N/A";
            }


            Console.WriteLine($"ACCOUNT: {account.Id}, PLAN: {account.MembershipPlan}");
            Console.WriteLine($"TRANSACTION: {(transaction == null ? "NULL" : transaction.MembershipPlan.ToString())}");

            return View(model);


        }


        public IActionResult ManageMembershipUser()
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

            var model = new MembershipPassViewModel
            {
                FullName = $"{account.FirstName} {account.LastName}",
                MemberID = account.MemberID,
            };

            if (transaction != null)
            {
                model.MembershipPlan = account.MembershipPlan.ToString();
                model.TransactionDate = transaction.TransactionDate;
                model.EndDate = transaction.EndDate;
                model.Status = transaction.Status.ToString();
            }
            else
            {
                model.MembershipPlan = "N/A";
                model.TransactionDate = DateTime.Now;
                model.EndDate = DateTime.Now.AddMonths(1);
                model.Status = "N/A";
            }

            return View(model);
        }

        [Authorize]
        public IActionResult ClassesUser() => View();




        public IActionResult AboutUs() => View();

        public IActionResult Classes() => View();
        public IActionResult Products() => View();

        public IActionResult AccountUser() => View();
        public IActionResult AboutFitcheckUser() => View();
        public IActionResult PrivacyPolicyUser() => View();
        public IActionResult TermsConditionsUser() => View();
        public IActionResult ChangePasswordUser() => View();


    }
}
