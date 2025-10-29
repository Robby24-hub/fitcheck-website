using System.ComponentModel.DataAnnotations;
using FitCheckWebApp.Models;

namespace FitCheckWebApp.ViewModels.Transaction
{
    public class TransactionViewModel
    {

        [Required]
        public MembershipPlan MembershipPlan { get; set; }

        [Required]
        public PaymentMethod PaymentMethod { get; set; }

        public decimal Amount { get; set; }
        public bool IsUpgrade { get; set; }
        public string? CurrentPlan { get; set; }


    }

    
}

