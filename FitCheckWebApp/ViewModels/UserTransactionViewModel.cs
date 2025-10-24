namespace FitCheckWebApp.ViewModels
{
    public class UserTransactionViewModel
    {
        public string OrderNumber { get; set; } = string.Empty; // e.g., "#ORD-2024-001"
        public DateTime TransactionDate { get; set; }
        public string Plan { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public class TransactionHistoryViewModel
    {
        public List<UserTransactionViewModel> Transactions { get; set; } = new List<UserTransactionViewModel>();
    }
}
