using System.Security.Claims;
using FitCheckWebApp.DataAccess;
using FitCheckWebApp.Helpers;
using FitCheckWebApp.Models;
using FitCheckWebApp.ViewModels;
using FitCheckWebApp.ViewModels.Account;
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
        // ==========================
        // ===== LOGIN & LOGOUT =====
        // ==========================
        #region Login & Logout
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
                if (!account.IsActive)
                {
                    ModelState.AddModelError("", "This account has been deactivated. Please contact support.");
                    return View(model);
                }

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
                else if (account.Role == "trainer")
                {
                    return RedirectToAction("TrainerClass", "Trainer");
                }

                return RedirectToAction("UserHome", "Account");
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
        #endregion


        // ==========================
        // ===== REGISTRATION =======
        // ==========================
        #region Registration
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegistrationViewModel model)
        {

            if (!model.AgreeTerms)
            {
                ModelState.AddModelError(nameof(model.AgreeTerms), "You must agree to the Terms and Conditions.");
            }

            if (!model.AgreePrivacy)
            {
                ModelState.AddModelError(nameof(model.AgreePrivacy), "You must agree to the Privacy Policy.");
            }

            if (!ModelState.IsValid)
                return View(model);

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
        #endregion


        // ==========================
        // ===== STATIC PAGES =======
        // ==========================
        #region Static Pages
        public IActionResult TermsAndConditions() => View();
        public IActionResult PrivacyPolicy() => View();
        public IActionResult Products() => View();
        public IActionResult AccountUser() => View();
        public IActionResult AboutFitcheckUser() => View();
        public IActionResult PrivacyPolicyUser() => View();
        public IActionResult TermsConditionsUser() => View();
        #endregion


        // ==========================
        // ===== USER PAGES ========
        // ==========================
        #region User Pages

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
                return RedirectToAction("Login", "Account");

            int accountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var account = AccountManager.FindById(accountId);

            if (account == null)
                return RedirectToAction("Login", "Account");

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

                model.HasActiveMembership = transaction.Status == TransactionStatus.Active && transaction.EndDate > DateTime.Now;
                model.CanRenew = transaction.Status == TransactionStatus.Expired ||
                                 (transaction.EndDate <= DateTime.Now && transaction.Status != TransactionStatus.Cancelled);



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

                model.HasActiveMembership = false;
                model.CanRenew = false;


            }

            Console.WriteLine($"[DEBUG] Status: {transaction?.Status}");
            Console.WriteLine($"[DEBUG] EndDate: {transaction?.EndDate}");
            Console.WriteLine($"[DEBUG] Now: {DateTime.Now}");
            Console.WriteLine($"[DEBUG] HasActiveMembership: {model.HasActiveMembership}");
            Console.WriteLine($"[DEBUG] CanRenew: {model.CanRenew}");



            return View(model);
        }


        [Authorize]
        public IActionResult ClassesUser()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var currentUser = AccountManager.FindById(userId);
            var allClasses = ClassManager.GetAllClasses();

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

            bool hasActiveMembership = false;
            string? membershipPlan = null;

            if (currentUser != null)
            {
                hasActiveMembership = currentUser.MembershipPlan != null && currentUser.MembershipPlan != MembershipPlan.None;
                membershipPlan = currentUser.MembershipPlan.ToString();

                var activeTransaction = TransactionManager.GetActiveTransactionByAccountId(userId);
                if (activeTransaction == null || activeTransaction.Status != TransactionStatus.Active || activeTransaction.EndDate < DateTime.Now)
                    hasActiveMembership = false;
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

                if (currentUser == null || currentUser.MembershipPlan == null)
                {
                    return Json(new { success = false, message = "You need an active membership to join classes. Please purchase a membership first." });
                }

                var activeTransaction = TransactionManager.GetActiveTransactionByAccountId(userId);
                if (activeTransaction == null || activeTransaction.Status != TransactionStatus.Active || activeTransaction.EndDate < DateTime.Now)
                {
                    return Json(new { success = false, message = "Your membership has expired. Please renew your membership to join classes." });
                }

                var classToJoin = ClassManager.GetClassById(request.ClassId);

                if (classToJoin == null)
                    return Json(new { success = false, message = "Class not found" });

                if (classToJoin.ParticipantsCount >= classToJoin.ParticipantLimit)
                    return Json(new { success = false, message = "Class is full" });

                bool joined = ClassManager.IncrementParticipantCount(request.ClassId);

                if (joined)
                {
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
        #endregion


        // ==========================
        // ===== PROFILE & PASSWORD ==
        // ==========================
        #region Profile & Password
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

        [Authorize]
        public IActionResult ChangePasswordUser() => View();

        [HttpPost]
        [Authorize]
        public IActionResult SendVerificationCode([FromBody] VerificationContext? context)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                var user = AccountManager.FindById(userId);

                if (user == null)
                    return Json(new { success = false, message = "User not found" });

                string code = VerificationCodeManager.GenerateCode();
                VerificationCodeManager.SaveCode(user.Email, code);

                string contextType = context?.Type ?? "password";

                if (contextType == "profile")
                {
                    Console.WriteLine("Sending PROFILE UPDATE email");
                    EmailHelper.SendProfileUpdateVerificationCode(user.Email, code, user.FirstName ?? "User");
                }
                else
                {
                    Console.WriteLine("Sending PASSWORD RESET email");
                    EmailHelper.SendVerificationCode(user.Email, code, user.FirstName ?? "User");
                }

                return Json(new { success = true, message = "Verification code sent to your email" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending verification code: {ex.Message}");
                return Json(new { success = false, message = "Failed to send verification code" });
            }
        }

        [HttpPost]
        [Authorize]
        public IActionResult VerifyCode([FromBody] VerifyCodeViewModel model)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                var user = AccountManager.FindById(userId);

                if (user == null)
                    return Json(new { success = false, message = "User not found" });

                bool isValid = VerificationCodeManager.VerifyCode(user.Email, model.Code);

                if (isValid)
                    return Json(new { success = true, message = "Code verified successfully" });
                else
                    return Json(new { success = false, message = "Invalid or expired code" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error verifying code: {ex.Message}");
                return Json(new { success = false, message = "Verification failed" });
            }
        }

        [HttpPost]
        [Authorize]
        public IActionResult ChangePassword([FromBody] ChangePasswordViewModel model)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                var user = AccountManager.FindById(userId);

                if (user == null)
                    return Json(new { success = false, message = "User not found" });

                if (!Helpers.Helpers.verifyPassword(model.CurrentPassword, user.PasswordHash))
                    return Json(new { success = false, message = "Current password is incorrect" });

                if (model.NewPassword != model.ConfirmPassword)
                    return Json(new { success = false, message = "New passwords do not match" });

                if (model.NewPassword.Length < 6)
                    return Json(new { success = false, message = "Password must be at least 6 characters" });

                user.PasswordHash = Helpers.Helpers.HashingPassword(model.NewPassword);
                AccountManager.UpdateAccount(user);

                return Json(new { success = true, message = "Password changed successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error changing password: {ex.Message}");
                return Json(new { success = false, message = "Failed to change password" });
            }
        }

        [HttpPost]
        [Authorize]
        public IActionResult ResetPasswordWithCode([FromBody] ResetPasswordViewModel model)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                var user = AccountManager.FindById(userId);

                if (user == null)
                    return Json(new { success = false, message = "User not found" });

                bool isValid = VerificationCodeManager.VerifyCode(user.Email, model.Code);
                if (!isValid)
                    return Json(new { success = false, message = "Invalid or expired code" });

                if (model.NewPassword != model.ConfirmPassword)
                    return Json(new { success = false, message = "Passwords do not match" });

                if (model.NewPassword.Length < 6)
                    return Json(new { success = false, message = "Password must be at least 6 characters" });

                user.PasswordHash = Helpers.Helpers.HashingPassword(model.NewPassword);
                AccountManager.UpdateAccount(user);

                VerificationCodeManager.MarkCodeAsUsed(user.Email, model.Code);

                return Json(new { success = true, message = "Password reset successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error resetting password: {ex.Message}");
                return Json(new { success = false, message = "Failed to reset password" });
            }
        }

        [HttpPost]
        [Authorize]
        public IActionResult UpdateProfile([FromForm] Account model)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var account = AccountManager.FindById(userId);

                if (account == null)
                    return RedirectToAction("Login");

                account.FirstName = model.FirstName;
                account.LastName = model.LastName;
                account.BirthDate = model.BirthDate;
                account.Age = Helpers.Helpers.CalculateAge(model.BirthDate);
                account.Gender = model.Gender;
                account.ContactNumber = model.ContactNumber;
                account.Email = model.Email;
                account.EmergencyName = model.EmergencyName;
                account.EmergencyContact = model.EmergencyContact;

                AccountManager.UpdateAccount(account);

                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction("UserProfileUser");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating profile: {ex.Message}");
                TempData["ErrorMessage"] = "Failed to update profile.";
                return RedirectToAction("UserProfileUser");
            }
        }
        #endregion


        // ==========================
        // ===== HELPERS & CLASSES ==
        // ==========================
        #region Helper Classes
        public class JoinClassRequest
        {
            public int ClassId { get; set; }
        }

        public class VerificationContext
        {
            public string Type { get; set; } = "password";
        }

        private string FormatClassType(ClassType type)
        {
            return System.Text.RegularExpressions.Regex.Replace(type.ToString(), "([a-z])([A-Z])", "$1 $2");
        }
        #endregion
    }
}
