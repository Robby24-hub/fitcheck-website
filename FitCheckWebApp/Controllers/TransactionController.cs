using System.Security.Claims;
using FitCheckWebApp.DataAccess;
using FitCheckWebApp.Helpers;
using FitCheckWebApp.Models;
using FitCheckWebApp.ViewModels;
using FitCheckWebApp.ViewModels.Transaction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitCheckWebApp.Controllers
{
    public class TransactionController : Controller
    {
        // ==== MemberShipChoice

        [Authorize(Roles = "admin")]
        public IActionResult TestWarningEmails()
        {
            TransactionManager.ExpireOldMemberships();
            EmailHelper.SendMembershipExpiryWarnings();
            return Content("Warning emails triggered manually.");
        }



        [HttpGet, Authorize]
        public IActionResult PaymentMethod() => View();

        [HttpPost, Authorize]
        public IActionResult PaymentMethod(TransactionViewModel newtransaction)
        {
            int accountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (!ModelState.IsValid)
                return View(newtransaction);
            if (!User.Identity!.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            var lastTransaction = TransactionManager.FindLatestActiveByAccount(accountId);

            bool isRenewal = TempData["IsRenewal"] != null && (bool)TempData["IsRenewal"];
            bool isExtension = lastTransaction != null && lastTransaction.Status == TransactionStatus.Active;

            if (isRenewal && lastTransaction != null)
            {
                newtransaction.MembershipPlan = lastTransaction.MembershipPlan;
            }

            DateTime startDate;
            if (isExtension && lastTransaction != null)
            {
                startDate = lastTransaction.EndDate.AddDays(1);
            }
            else
            {
                startDate = DateTime.Now;
            }

            DateTime endDate = startDate.AddMonths(1);

            decimal amount = newtransaction.MembershipPlan switch
            {
                MembershipPlan.FitStart => 999m,
                MembershipPlan.FitPro => 1499m,
                MembershipPlan.FitElite => 2499m,
                _ => 0m
            };

            var status = newtransaction.PaymentMethod.ToString() == "Cash"
                ? TransactionStatus.Pending
                : TransactionStatus.Active;

            var transaction = new Transaction
            {
                AccountID = accountId,
                MembershipPlan = newtransaction.MembershipPlan,
                PaymentMethod = newtransaction.PaymentMethod,
                StartDate = startDate,
                EndDate = endDate,
                TransactionDate = DateTime.Now,
                Status = status,
                Amount = amount
            };

            TransactionManager.PostTransaction(transaction);

            var account = AccountManager.FindById(accountId);

            if (transaction.Status == TransactionStatus.Active)
            {
                if (account != null)
                {
                    if (string.IsNullOrEmpty(account.MemberID))
                    {
                        account.MemberID = Helpers.Helpers.MemberIdGenerator();
                    }
                    account.MembershipPlan = newtransaction.MembershipPlan;
                    AccountManager.UpdateAccount(account);

                    // ===== SEND EMAIL RECEIPT =====
                    try
                    {
                        var savedTransaction = TransactionManager.FindLatestActiveByAccount(accountId);
                        string fullName = $"{account.FirstName} {account.LastName}";
                        string referenceNumber = $"REF-{DateTime.Now:yyyyMMdd}-{savedTransaction?.TransactionID ?? 0:D6}";

                        EmailHelper.SendTransactionReceipt(
                            toEmail: account.Email!,
                            userName: fullName,
                            membershipPlan: newtransaction.MembershipPlan.ToString(),
                            amount: amount,
                            transactionDate: transaction.TransactionDate,
                            endDate: endDate,
                            transactionId: (savedTransaction?.TransactionID ?? 0).ToString(),
                            referenceNumber: referenceNumber
                        );

                        Console.WriteLine($"✅ Receipt sent to {account.Email}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Email failed: {ex.Message}");
                    }
                }
            }

            return RedirectToAction("UserMembership");
        }




        [Authorize]
        [HttpPost]
        public IActionResult CancelMembership(int transactionId)
        {
            int accountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var transaction = TransactionManager.FindById(transactionId);

            if (transaction == null || transaction.AccountID != accountId || transaction.Status != TransactionStatus.Active)
            {
                return BadRequest("Cannot cancel this transaction.");
            }

            transaction.Status = TransactionStatus.Cancelled;
            TransactionManager.UpdateTransaction(transaction);

            var latestActive = TransactionManager.FindLatestActiveByAccount(accountId);

            var account = AccountManager.FindById(accountId);
            if (account != null)
            {
                if (latestActive != null)
                {
                    account.MembershipPlan = latestActive.MembershipPlan;
                }
                else
                {
                    account.MembershipPlan = MembershipPlan.None;
                }

                AccountManager.UpdateAccount(account);
            }

            return RedirectToAction("UserMembership");
        }




        // ===== PAGES =====


        public IActionResult UserMembership() => View();



        [Authorize(Roles = "user")]
        public IActionResult TransactionHistoryUser()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            int userId = int.Parse(userIdClaim);

            var transactions = TransactionManager.GetTransactionsByUser(userId);


            var model = new TransactionHistoryViewModel
            {
                Transactions = transactions.Select(t => new UserTransactionViewModel
                {
                    OrderNumber = $"#ORD-{t.TransactionID:D3}",
                    TransactionDate = t.TransactionDate,
                    Plan = t.MembershipPlan.ToString(),
                    Amount = t.Amount
                }).ToList()
            };

            return View(model);
        }




        [HttpPost]
        [Authorize]
        public IActionResult RenewMembership()
        {
            int accountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var account = AccountManager.FindById(accountId);


            if (account == null)
                return RedirectToAction("Login", "Account");

            var lastTransaction = TransactionManager.FindLatestByAccount(accountId);

            if (lastTransaction == null)
            {
                TempData["Error"] = "No previous membership found. Please choose a plan.";
                return RedirectToAction("UserMembership", "Transaction");
            }

            if (lastTransaction.Status == TransactionStatus.Active)
            {
                TempData["Notice"] = "Your membership is still active. You can extend it instead.";
                return RedirectToAction("UserMembership", "Transaction");
            }

            if (lastTransaction.Status == TransactionStatus.Expired)
            {

                TempData["IsRenewal"] = true;
                TempData["RenewPlan"] = lastTransaction.MembershipPlan;

                return RedirectToAction("PaymentMethod", "Transaction");
            }

            TempData["Error"] = "Unable to process renewal. Please try again later.";
            return RedirectToAction("UserMembership", "Transaction");

        }

    }
}
