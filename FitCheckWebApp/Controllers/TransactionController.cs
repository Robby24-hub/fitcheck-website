using System.Security.Claims;
using FitCheckWebApp.DataAccess;
using FitCheckWebApp.Helpers;
using FitCheckWebApp.Models;
using FitCheckWebApp.ViewModels;
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
            bool isRenewal = lastTransaction != null && lastTransaction.Status == TransactionStatus.Expired;
            bool isExtension = lastTransaction != null && lastTransaction.Status == TransactionStatus.Active;

            DateTime startDate;
            if (isExtension)
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


            if (transaction.Status == TransactionStatus.Active)
            {
                var account = AccountManager.FindById(accountId);
                if (account != null)
                {
                    if (string.IsNullOrEmpty(account.MemberID))
                    {
                        account.MemberID = Helpers.Helpers.MemberIdGenerator();
                    }

                    account.MembershipPlan = newtransaction.MembershipPlan;
                    AccountManager.UpdateAccount(account);
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
        public IActionResult Membership() => View();

        public IActionResult UserMembership() => View();



        [Authorize(Roles = "user")]
        public IActionResult TransactionHistoryUser()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            int userId = int.Parse(userIdClaim);

            var transactions = TransactionManager.GetTransactionsByUser(userId);

            // Map from Transaction -> UserTransactionViewModel
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





    }
}
