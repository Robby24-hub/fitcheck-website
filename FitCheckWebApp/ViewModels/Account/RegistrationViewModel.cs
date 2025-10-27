using System.ComponentModel.DataAnnotations;

namespace FitCheckWebApp.ViewModels.Account
{
    public class RegistrationViewModel
    {
        [Required(ErrorMessage = "First name is required.")]
        [RegularExpression(@"^[a-zA-Z][a-zA-Z\s'-]{1,}$", ErrorMessage = "Last name must be at least 2 letters and can only contain letters, spaces, hyphens, or apostrophes.")]
        public string? FirstName { get; set; }


        [Required(ErrorMessage = "Last name is required.")]
        [RegularExpression(@"^[a-zA-Z][a-zA-Z\s'-]{1,}$", ErrorMessage = "Last name must be at least 2 letters and can only contain letters, spaces, hyphens, or apostrophes.")]
        public string? LastName { get; set; }


        [Required(ErrorMessage = "Birth date is required.")]
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }



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
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&_]).+$", ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character.")]
        public string? Password { get; set; }


        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string? ConfirmPassword { get; set; }


        [Required(ErrorMessage = "Emergency contact name is required.")]
        public string? FullName { get; set; }


        [Required(ErrorMessage = "Emergency contact number is required.")]
        [RegularExpression(@"^(\+63|0)9\d{9}$", ErrorMessage = "Invalid Philippine phone number.")]
        public string? EmergencyContactNumber { get; set; }


        [Display(Name = "Agree to Terms")]
        public bool AgreeTerms { get; set; }

        [Display(Name = "Agree to Privacy Policy")]
        public bool AgreePrivacy { get; set; }


    }
}
