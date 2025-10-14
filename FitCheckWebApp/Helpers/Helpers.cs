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

            int birthYear = model.Birthday.Year;
            int currentYear = DateTime.Now.Year;
            int age = birthYear - currentYear;

            return birthYear;


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
