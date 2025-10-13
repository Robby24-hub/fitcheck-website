using FitCheckWebApp.DataAccess;
using FitCheckWebApp.Models;
using FitCheckWebApp.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FitCheckWebApp.Controllers
{
    public class TransactionController : Controller
    {
        // ==== MemberShipChoice

        [HttpGet]
        public IActionResult PaymentMethod() => View();

        [HttpPost]
        public IActionResult PaymentMethod(TransactionViewModel newtransaction)
        {

            if (!ModelState.IsValid)
                return View(newtransaction);

            var transaction = new Transaction
            {
                AccountID = 1,
                MembershipPlan = newtransaction.MembershipPlan,
                PaymentMethod = newtransaction.PaymentMethod,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(1)

            };



           return RedirectToAction("Membership");
        }

        // ===== PAGES =====
        public IActionResult Membership() => View();

        

    }
}
