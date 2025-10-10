using System.ComponentModel.DataAnnotations;

namespace FitCheckWebApp.ViewModels
{
    public class TransactionViewModel
    {

        [Required]
        public MembershipPlan MembershipPlan { get; set; }

        [Required]
        public PaymentMethod PaymentMethod { get; set; }


    }

    public enum MembershipPlan
    {
        FitStart,
        FitElite,
        FitPro
    }

    public enum PaymentMethod
    {
        credit,
        debit,
        cash
    }
}

