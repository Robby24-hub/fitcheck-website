using System.ComponentModel.DataAnnotations;
using FitCheckWebApp.Models;

namespace FitCheckWebApp.ViewModels
{
    public class TransactionViewModel
    {

        [Required]
        public MembershipPlan MembershipPlan { get; set; }

        [Required]
        public PaymentMethod PaymentMethod { get; set; }


    }

    
}

