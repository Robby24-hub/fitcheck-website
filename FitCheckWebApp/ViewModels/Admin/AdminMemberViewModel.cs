namespace FitCheckWebApp.ViewModels.Admin
{
    public class AdminMemberViewModel
    {
        public List<MemberViewModel> Members { get; set; } = new List<MemberViewModel>();
    }

    public class MemberViewModel
    {
        public int AccountID { get; set; }
        public string MemberID { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string MembershipPlan { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? PaymentDue { get; set; }
    }
}
