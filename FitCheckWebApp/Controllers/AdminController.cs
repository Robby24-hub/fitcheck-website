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
        public IActionResult AdminPayment() => View();



        [Authorize(Roles = "admin")]
        public IActionResult AdminClass() => View();


        [Authorize(Roles = "admin")]
        public IActionResult AdminMember() => View(); 

    }
}
