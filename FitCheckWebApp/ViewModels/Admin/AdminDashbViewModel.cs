namespace FitCheckWebApp.ViewModels.Admin
{
    public class AdminDashbViewModel
    {
        public int ActiveMembers { get; set; }
        public int PendingPayments { get; set; }
        public int UpcomingClasses { get; set; }

        public string AdminName { get; set; } = string.Empty;
    }
}
