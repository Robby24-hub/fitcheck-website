using FitCheckWebApp.DataAccess;
using FitCheckWebApp.Models;
using FitCheckWebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitCheckWebApp.Controllers
{
    public class AdminController : Controller
    {
        [Authorize(Roles = "admin")]
        public IActionResult AdminHome()
        {
            var model = new AdminDashbViewModel
            {
                ActiveMembers = TransactionManager.CountActiveMembers(),
                PendingPayments = TransactionManager.CountPendingPayment(),
                UpcomingClasses = ClassManager.CountUpcomingClassesToday(),
                AdminName = User.Identity?.Name ?? "Admin"
            };

            return View(model);
        }   

        [Authorize(Roles = "admin")]
        public IActionResult AdminPayment()
        {
            var pendingTransactions = TransactionManager.GetPendingTransactions();

            var pendingViewModels = pendingTransactions.Select(t => new PendingMembershipViewModel
            {
                Id = t.TransactionID,
                Name = t.AccountName,
                Plan = t.MembershipPlan.ToString(),
                Payment = t.Amount
            }).ToList();

            var model = new AdminPaymentViewModel
            {
                PendingMemberships = pendingViewModels
            };

            return View(model);
        }

        [Authorize(Roles = "admin")]
        public IActionResult AdminClass()
        {
            try
            {
                // Get all classes
                var classes = ClassManager.GetAllClasses();

                // Get trainers
                var trainers = AccountManager.GetAllAccounts()
                                .Where(a => !string.IsNullOrEmpty(a.Role) && a.Role.ToLower() == "trainer")
                                .ToList();

                // Debug output to console
                Console.WriteLine($"=== AdminClass Debug ===");
                Console.WriteLine($"Total trainers found: {trainers.Count}");
                foreach (var t in trainers)
                {
                    Console.WriteLine($"Trainer: ID={t.Id}, Name={t.FirstName} {t.LastName}, Role={t.Role}");
                }

                // Pass to view
                ViewBag.Trainers = trainers;

                return View(classes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in AdminClass: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        [Authorize(Roles = "admin")]
        public IActionResult AdminMember()
        {
            var model = new AdminMemberViewModel
            {
                Members = AccountManager.GetAllMembers()
            };

            return View(model);
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public IActionResult DeleteMember(int id)
        {
            AccountManager.SoftDeleteMember(id);
            return RedirectToAction("AdminMember");
        }

        [HttpPost, Authorize(Roles = "admin")]
        public IActionResult ApproveMembership(int id)
        {
            var transaction = TransactionManager.FindById(id);
            if (transaction == null)
                return NotFound();

            transaction.Status = TransactionStatus.Active;
            transaction.StartDate = DateTime.Now;
            transaction.EndDate = transaction.StartDate.AddMonths(1);
            transaction.TransactionDate = DateTime.Now;
            TransactionManager.UpdateTransaction(transaction);

            var account = AccountManager.FindById(transaction.AccountID);
            if (account != null)
            {
                account.MembershipPlan = transaction.MembershipPlan;
                if (string.IsNullOrEmpty(account.MemberID))
                    account.MemberID = Helpers.Helpers.MemberIdGenerator();
                AccountManager.UpdateAccount(account);
            }

            return RedirectToAction("AdminPayment");
        }

        [HttpPost, Authorize(Roles = "admin")]
        public IActionResult DeclineMembership(int id)
        {
            var transaction = TransactionManager.FindById(id);
            if (transaction == null)
                return NotFound();

            transaction.Status = TransactionStatus.Declined;
            TransactionManager.UpdateTransaction(transaction);

            return RedirectToAction("AdminPayment");
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public IActionResult AddClass(Class cls)
        {
            try
            {
                ClassManager.AddClass(cls);
                TempData["SuccessMessage"] = "Class added successfully!";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while adding the class.";
                Console.WriteLine($"Error adding class: {ex.Message}");
            }

            return RedirectToAction("AdminClass");
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public IActionResult EditClass(Class cls)
        {
            try
            {
                ClassManager.UpdateClass(cls);
                TempData["SuccessMessage"] = "Class updated successfully!";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while updating the class.";
                Console.WriteLine($"Error updating class: {ex.Message}");
            }

            return RedirectToAction("AdminClass");
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public IActionResult DeleteClass(int id)
        {
            try
            {
                bool deleted = ClassManager.DeleteClass(id);

                if (deleted)
                {
                    TempData["SuccessMessage"] = "Class deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Class not found or could not be deleted.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the class.";
                Console.WriteLine($"Error deleting class: {ex.Message}");
            }

            return RedirectToAction("AdminClass");
        }


    }
}
