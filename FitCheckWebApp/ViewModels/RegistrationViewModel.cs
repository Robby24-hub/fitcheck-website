using System.ComponentModel.DataAnnotations;

namespace FitCheckWebApp.ViewModels
{
    public class RegistrationViewModel
    {
        [Required]
        public string? FullName {  get; set; }

        [Required]
        public string? Gender { get; set; }

        [Required]
        public DateTime Birthday { get; set; }

        [Range(18, 100)]
        public int Age { get; set; }

        [Required, Phone]
        public string? ContactNumber { get; set; }

        [Required, EmailAddress]
        public string? Email { get; set; }

        [Required]  
        public string? Username { get; set; }

        [Required, MinLength(6)]
        public string? Password { get; set; }

        [Required, Compare("Password")]
        public string? ConfirmPassword { get; set; }

        [Required]
        public bool AgreeTerms { get; set; }

        [Required]
        public bool AgreePrivacy { get; set; }

    }


}
