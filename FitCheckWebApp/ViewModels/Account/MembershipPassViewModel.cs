namespace FitCheckWebApp.ViewModels.Account
{
    public class MembershipPassViewModel
    {

        public string? FullName { get; set; }
        public string? MemberID { get; set; }
        public string? MembershipPlan { get; set; }

        public string? EmergencyName { get; set; }

        public string? EmergencyContact { get; set; }

        public DateTime? TransactionDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Status { get; set; }
        public string? PaymentStatus { get; set; }
        public bool CanRenew { get; set; }
        
        public bool canUpgrade { get; set; }

        public string CurrentPlanLabel { get; set; } = string.Empty;
        public string NextPlanLabel { get; set; } = string.Empty;

        public bool HasActiveMembership { get; set; }


        public string? WarningMessage { get; set; }


        public int TransactionId { get; set; }


    }

}
