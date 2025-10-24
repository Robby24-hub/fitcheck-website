using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FitCheckWebApp.Models;
using FitCheckWebApp.ViewModels;
using FitCheckWebApp.DataAccess;


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
        public IActionResult AdminMember() => View(); 

    }
}
