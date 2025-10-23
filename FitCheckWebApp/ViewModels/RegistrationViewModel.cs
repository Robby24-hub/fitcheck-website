using System.ComponentModel.DataAnnotations;

namespace FitCheckWebApp.ViewModels
{
    public class RegistrationViewModel
    {
        [Required(ErrorMessage = "First name is required.")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Birthday is required.")]
        [DataType(DataType.Date)]
        public DateTime? Birthday { get; set; }

        [Range(1, 120, ErrorMessage = "Age must be between 1 and 120.")]
        public int Age { get; set; }

        [Required(ErrorMessage = "Gender is required.")]
        public string? Gender { get; set; }

        [Required(ErrorMessage = "Contact number is required.")]
        [RegularExpression(@"^(\+63|0)9\d{9}$", ErrorMessage = "Invalid Philippine phone number.")]
        public string? ContactNumber { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        public string? Password { get; set; }

        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string? ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Emergency contact name is required.")]
        public string? FullName { get; set; }

        [Required(ErrorMessage = "Emergency contact number is required.")]
        [RegularExpression(@"^(\+63|0)9\d{9}$", ErrorMessage = "Invalid Philippine phone number.")]
        public string? EmergencyContactNumber { get; set; }


        [Required(ErrorMessage = "You must agree to the Terms and Conditions.")]
        [Display(Name = "Agree to Terms")]
        public bool AgreeTerms { get; set; }

        [Required(ErrorMessage = "You must agree to the Privacy Policy.")]
        [Display(Name = "Agree to Privacy Policy")]
        public bool AgreePrivacy { get; set; }
    }
}
