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

        [HttpPost]
        public IActionResult PaymentMethod(TransactionViewModel newtransaction)
        {

            int accountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            if (!ModelState.IsValid)
                return View(newtransaction);


            if (!User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var transaction = new Transaction
            {
                AccountID = accountId,
                MembershipPlan = newtransaction.MembershipPlan,
                PaymentMethod = newtransaction.PaymentMethod,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(1)
            };

            TransactionManager.PostTransaction(transaction);

            var account = AccountManager.FindById(accountId);

            if (account != null)
            {
                account.MemberID = Helpers.Helpers.MemberIdGenerator();
                account.MembershipPlan = newtransaction.MembershipPlan;

                AccountManager.UpdateAccount(account);
            }

            return RedirectToAction("Membership");
        }



        // ===== PAGES =====
        public IActionResult Membership() => View();

        public IActionResult UserMembership() => View();


    }
}
