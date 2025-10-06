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


    }
}
