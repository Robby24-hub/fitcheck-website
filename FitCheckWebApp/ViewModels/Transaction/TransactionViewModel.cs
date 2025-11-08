using System.ComponentModel.DataAnnotations;
using FitCheckWebApp.Models;

namespace FitCheckWebApp.ViewModels.Transaction
{
    public class TransactionViewModel
    {
        [Required(ErrorMessage = "Membership plan is required")]
        public MembershipPlan MembershipPlan { get; set; }

        [Required(ErrorMessage = "Payment method is required")]
        public PaymentMethod PaymentMethod { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        public bool IsUpgrade { get; set; }
        public string? CurrentPlan { get; set; }


        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "First name can only contain letters and spaces")]
        public string? FirstName { get; set; }

        [StringLength(50, ErrorMessage = "Surname cannot exceed 50 characters")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Surname can only contain letters and spaces")]
        public string? Surname { get; set; }

        [Required(ErrorMessage = "Card number is required")]
        [RegularExpression(@"^\d{13,19}$", ErrorMessage = "Card number must be 13-19 digits")]
        [StringLength(19, ErrorMessage = "Card number cannot exceed 19 characters")]
        public string? CardNumber { get; set; }

        [StringLength(100, ErrorMessage = "Cardholder name cannot exceed 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Cardholder name can only contain letters and spaces")]
        public string? CardHolderName { get; set; }

        [StringLength(3, MinimumLength = 3, ErrorMessage = "CVV must be exactly 3 digits")]
        [RegularExpression(@"^[0-9]{3}$", ErrorMessage = "CVV must be 3 digits")]
        public string? CVV { get; set; }

        [RegularExpression(@"^(0[1-9]|1[0-2])\/([0-9]{2})$", ErrorMessage = "Expiry date must be in MM/YY format")]
        public string? Validity { get; set; }

        [StringLength(11, MinimumLength = 11, ErrorMessage = "GCash number must be 11 digits")]
        [RegularExpression(@"^(09|\+639)\d{9}$", ErrorMessage = "Invalid GCash number. Must be 09XXXXXXXXX or +639XXXXXXXXX format")]
        public string? AccountNumber { get; set; }

        [StringLength(11, ErrorMessage = "Cellphone number must be 10-11 digits")]
        [RegularExpression(@"^(09|\+639|9)\d{9}$", ErrorMessage = "Invalid Philippine mobile number. Use 09XXXXXXXXX, +639XXXXXXXXX, or 9XXXXXXXXX format")]
        public string? CellphoneNumber { get; set; }
    }
}