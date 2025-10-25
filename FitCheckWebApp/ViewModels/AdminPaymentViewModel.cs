namespace FitCheckWebApp.ViewModels
{
    public class AdminPaymentViewModel
    {
        public List<PendingMembershipViewModel> PendingMemberships { get; set; } = new List<PendingMembershipViewModel>();
    }

    public class PendingMembershipViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Plan { get; set; } = string.Empty;
        public decimal Payment { get; set; }
    }
}
