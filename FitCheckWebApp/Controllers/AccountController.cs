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
            if (!Helpers.Helpers.IsBirthdayValid(model.BirthDate))
            {
                ModelState.AddModelError("BirthDate", "Birth date cannot be a future date.");
                return View(model);
            }

            model.Age = Helpers.Helpers.CalculateAge(model.BirthDate);

            if (model.Age < 0)
            {
                ModelState.AddModelError("Age", "Invalid age calculated from birth date.");
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
                LastName = model.LastName,
                BirthDate = model.BirthDate,
                Age = model.Age,
                Gender = model.Gender,
                ContactNumber = model.ContactNumber,
                EmergencyName = model.FullName,
                EmergencyContact = model.EmergencyContactNumber
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


        [Authorize]
        public IActionResult ManageMembershipUser()
        {
            if (!User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            int accountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var account = AccountManager.FindById(accountId);

            if (account == null)
                return RedirectToAction("Login", "Account");

            // Fetch the latest active transaction for the account
            var transaction = TransactionManager.FindLatestActiveByAccount(accountId);

            var model = new MembershipPassViewModel
            {
                FullName = $"{account.FirstName} {account.LastName}",
                MemberID = account.MemberID,
            };

            if (transaction != null)
            {
                model.TransactionId = transaction.TransactionID; 
                model.MembershipPlan = transaction.MembershipPlan.ToString();
                model.TransactionDate = transaction.TransactionDate;
                model.EndDate = transaction.EndDate;
                model.Status = transaction.Status.ToString();

                if (transaction.EndDate <= DateTime.Now)
                {
                    model.WarningMessage = "Your membership has expired. Please renew to continue access.";
                }
                else if ((transaction.EndDate - DateTime.Now).TotalDays <= 3)
                {
                    model.WarningMessage = $"Your membership will expire in {(transaction.EndDate - DateTime.Now).Days} days.";
                }
            }
            else
            {
                model.MembershipPlan = "N/A";
                model.TransactionDate = null;
                model.EndDate = null;
                model.Status = "N/A";
                model.WarningMessage = "You do not have an active membership.";
            }

            return View(model);
        }


        [Authorize]
        public IActionResult ClassesUser()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var currentUser = AccountManager.FindById(userId);

            var allClasses = ClassManager.GetAllClasses();

            // Group classes by day
            var classesByDay = allClasses
                .GroupBy(c => c.Day)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(c => new ClassDisplayViewModel
                    {
                        Id = c.Id,
                        Type = FormatClassType(c.Type),
                        InstructorName = c.InstructorName ?? "Unassigned",
                        Time = c.Time,
                        DurationMinutes = c.DurationMinutes,
                        ParticipantLimit = c.ParticipantLimit,
                        ParticipantsCount = c.ParticipantsCount
                    })
                    .OrderBy(c => c.Time)
                    .ToList()
                );

            // Check if user has active membership
            bool hasActiveMembership = false;
            string? membershipPlan = null;

            if (currentUser != null)
            {
                // Check if user has a membership plan
                hasActiveMembership = currentUser.MembershipPlan != null && currentUser.MembershipPlan != MembershipPlan.None;
                membershipPlan = currentUser.MembershipPlan.ToString();

                // Optionally: Also check if their membership transaction is active and not expired
                var activeTransaction = TransactionManager.GetActiveTransactionByAccountId(userId);
                if (activeTransaction == null || activeTransaction.Status != TransactionStatus.Active || activeTransaction.EndDate < DateTime.Now)
                {
                    hasActiveMembership = false;
                }
            }

            var model = new ClassesUserViewModel
            {
                ClassesByDay = classesByDay,
                HasActiveMembership = hasActiveMembership,
                MembershipPlan = membershipPlan
            };

            return View(model);
        }


        [HttpPost]
        [Authorize]
        public IActionResult JoinClass([FromBody] JoinClassRequest request)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                var currentUser = AccountManager.FindById(userId);

                // Check if user has active membership
                if (currentUser == null || currentUser.MembershipPlan == null)
                {
                    return Json(new { success = false, message = "You need an active membership to join classes. Please purchase a membership first." });
                }

                // Check if membership is active and not expired
                var activeTransaction = TransactionManager.GetActiveTransactionByAccountId(userId);
                if (activeTransaction == null || activeTransaction.Status != TransactionStatus.Active || activeTransaction.EndDate < DateTime.Now)
                {
                    return Json(new { success = false, message = "Your membership has expired. Please renew your membership to join classes." });
                }

                // Check if class exists
                var classToJoin = ClassManager.GetClassById(request.ClassId);

                if (classToJoin == null)
                    return Json(new { success = false, message = "Class not found" });

                // Check if already full
                if (classToJoin.ParticipantsCount >= classToJoin.ParticipantLimit)
                    return Json(new { success = false, message = "Class is full" });

                // Try to increment participant count
                bool joined = ClassManager.IncrementParticipantCount(request.ClassId);

                if (joined)
                {
                    // Get updated class info
                    var updatedClass = ClassManager.GetClassById(request.ClassId);
                    bool isFull = updatedClass != null && updatedClass.ParticipantsCount >= updatedClass.ParticipantLimit;

                    return Json(new
                    {
                        success = true,
                        message = "Successfully joined the class!",
                        isFull = isFull,
                        participantsCount = updatedClass?.ParticipantsCount ?? 0,
                        participantLimit = updatedClass?.ParticipantLimit ?? 0
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Class is full or no longer available" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error joining class: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while joining the class" });
            }
        }

        // Add this class at the bottom of your controller or in a separate file
        public class JoinClassRequest
        {
            public int ClassId { get; set; }
        }


        private string FormatClassType(ClassType type)
        {
            return System.Text.RegularExpressions.Regex.Replace(type.ToString(), "([a-z])([A-Z])", "$1 $2");
        }


        public IActionResult AboutUs() => View();

        public IActionResult Classes() => View();
        public IActionResult Products() => View();

        public IActionResult AccountUser() => View();
        public IActionResult AboutFitcheckUser() => View();
        public IActionResult PrivacyPolicyUser() => View();
        public IActionResult TermsConditionsUser() => View();
        public IActionResult ChangePasswordUser() => View();

        [Authorize]
        public IActionResult UserProfileUser()
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var account = AccountManager.FindByEmail(userEmail!);
            if (account == null)
                return RedirectToAction("Login");

            account.Age = Helpers.Helpers.CalculateAge(account.BirthDate);



            return View(account);
        }


    }
}
