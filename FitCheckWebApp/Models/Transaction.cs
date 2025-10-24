namespace FitCheckWebApp.Models
{
        public class Transaction
        {
            public int TransactionID { get; set; }
            public int AccountID { get; set; }

            public string AccountName { get; set; } = string.Empty;

            public MembershipPlan MembershipPlan { get; set; }
            public PaymentMethod PaymentMethod { get; set; }

            public DateTime TransactionDate { get; set; } = DateTime.Now;
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }

            public Decimal Amount { get; set; }

            public TransactionStatus Status { get; set; } = TransactionStatus.Active;
        }

        public enum MembershipPlan
        {
            None,
            FitStart,
            FitElite,
            FitPro
        }

        public enum PaymentMethod
        {
            Credit,
            Debit,
            Cash,
            None
        }

        public enum TransactionStatus
        {
            Active,
            Expired,
            Cancelled,
            Pending
        }

}
