using FitCheckWebApp.Models;
using FitCheckWebApp.ViewModels;

namespace FitCheckWebApp.Helpers
{
    public static class Helpers
    {

        public static int MapMemberShipToID (MembershipPlan plan)
        {

            switch(plan)
            {
                case MembershipPlan.FitStart:
                    return 1;
                case MembershipPlan.FitElite:
                    return 2;
                case MembershipPlan.FitPro: 
                    return 3;
                default: 
                    return 1;
            }

        }

        public static string? HashingPassword(string password)
        {
            
            string hash = BCrypt.Net.BCrypt.EnhancedHashPassword(password, 13);

            return hash;

        }

        public static Boolean verifyPassword(string passwordInput, string storedHash)
        {

            return BCrypt.Net.BCrypt.EnhancedVerify(passwordInput, storedHash);

        }

        public static int CalculateAge(RegistrationViewModel model)
        {
            
            if (!model.Birthday.HasValue)
                return 0; 

            var birthDate = model.Birthday.Value.Date;
            var today = DateTime.Today;

            int age = today.Year - birthDate.Year;


            if (birthDate > today.AddYears(-age))
                age--;

            return age < 0 ? 0 : age; 
        }


        public static bool IsBirthdayValid(DateTime? birthday)
        {
            if (!birthday.HasValue)
                return false; 


            return birthday.Value.Date <= DateTime.Today;
        }



        public static string? MemberIdGenerator()
        {
            string todaysDate = DateTime.Now.ToString("yyyyMMdd");

            int dailyCount = 1;

            string memberID = $"{todaysDate}-{dailyCount:D3}";

            return memberID;

        }

    }
}
