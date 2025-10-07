namespace FitCheckWebApp.Models
{
    public class Account
    {

        public int Id { get; set; }

        public string? Username { get; set; }

        public string? PasswordHash { get; set; }

        public string? Email { get; set; }

        public string Role { get; set; } = "user";

        public int? MembershipID { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

    }
}
