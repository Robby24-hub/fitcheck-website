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
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }


    }
}
