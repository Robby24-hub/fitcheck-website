namespace FitCheckWebApp.Models
{
    public class Account
    {

        public int Id { get; set; }

        public string? MemberID { get; set; }

        public string? Username { get; set; }

        public string? PasswordHash { get; set; }

        public string? Email { get; set; }

        public string Role { get; set; } = "user";

        public DateTime DateCreated { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        public MembershipPlan MembershipPlan { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public DateTime? BirthDate { get; set; }
        public int? Age { get; set; }
        public string? Gender { get; set; }
        public string? ContactNumber { get; set; }
        public string? EmergencyName { get; set; }
        public string? EmergencyContact { get; set; }

    }
}
