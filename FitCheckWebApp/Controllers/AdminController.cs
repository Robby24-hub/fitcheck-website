using FitCheckWebApp.DataAccess;
using FitCheckWebApp.Models;
using FitCheckWebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;


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
                UpcomingClasses = 3,
                AdminName = User.Identity?.Name ?? "Admin"
            };

            return View(model);
        }


        [Authorize(Roles = "admin")]
        public IActionResult AdminPayment()
        {
            var pendingTransactions = TransactionManager.GetPendingTransactions();

            var pendingViewModels = pendingTransactions.Select(t => new PendingTransactionViewModel
            {
                TransactionId = t.TransactionID,
                AccountName = t.AccountName,
                MembershipPlan = t.MembershipPlan.ToString(),
                PaymentMethod = t.PaymentMethod.ToString(),
                Amount = t.Amount,
                TransactionDate = t.TransactionDate
            }).ToList();

            var model = new AdminPaymentViewModel
            {
                PendingTransactions = pendingViewModels
            };

            return View(model);

        }



        [Authorize(Roles = "admin")]
        public IActionResult AdminClass() => View();


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
        public IActionResult ApproveMembership(int transactionId)
        {
            
            var transaction = TransactionManager.FindById(transactionId);
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
        public IActionResult DeclineMembership(int transactionId)
        {
            var transaction = TransactionManager.FindById(transactionId);
            if (transaction == null)
                return NotFound();

            
            transaction.Status = TransactionStatus.Declined;
            TransactionManager.UpdateTransaction(transaction);

            return RedirectToAction("AdminPayment");
        }

    }
}
