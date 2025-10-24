namespace FitCheckWebApp.ViewModels
{

    public class AdminPaymentViewModel
    {
        public List<PendingTransactionViewModel> PendingTransactions { get; set; } = new List<PendingTransactionViewModel>();
    }

    public class PendingTransactionViewModel
    {
        public int TransactionId { get; set; }          
        public string AccountName { get; set; } = string.Empty;
        public string MembershipPlan { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }


    }

    
}
