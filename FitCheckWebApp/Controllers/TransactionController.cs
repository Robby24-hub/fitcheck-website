using System.Security.Claims;
using FitCheckWebApp.DataAccess;
using FitCheckWebApp.Models;
using FitCheckWebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitCheckWebApp.Controllers
{
    public class TransactionController : Controller
    {
        // ==== MemberShipChoice

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
            bool isRenewal = lastTransaction != null && lastTransaction.EndDate > DateTime.Now;

            DateTime startDate = isRenewal ? lastTransaction!.EndDate.AddDays(1) : DateTime.Now;
            DateTime endDate = startDate.AddMonths(1);


            var transaction = new Transaction
            {
                AccountID = accountId,
                MembershipPlan = newtransaction.MembershipPlan,
                PaymentMethod = newtransaction.PaymentMethod,
                StartDate = startDate,
                EndDate = endDate,
                TransactionDate = DateTime.Now,
                Status = TransactionStatus.Active 
            };


            TransactionManager.PostTransaction(transaction);

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

        public IActionResult TransactionHistoryUser() => View();

    }
}
