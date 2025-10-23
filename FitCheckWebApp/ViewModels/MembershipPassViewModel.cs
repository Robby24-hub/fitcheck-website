namespace FitCheckWebApp.ViewModels
{
    public class MembershipPassViewModel
    {

        public string? FullName { get; set; }
        public string? MemberID { get; set; }
        public string? MembershipPlan { get; set; }


        public DateTime? TransactionDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Status { get; set; }

        public string? WarningMessage { get; set; }

    }

}
