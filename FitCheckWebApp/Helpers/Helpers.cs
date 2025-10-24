using FitCheckWebApp.Models;
using FitCheckWebApp.ViewModels;
using MySql.Data.MySqlClient;

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

        public static int CalculateAge(DateTime? birthDate)
        {
            if (!birthDate.HasValue)
                return 0;

            var date = birthDate.Value.Date;
            var today = DateTime.Today;

            int age = today.Year - date.Year;
            if (date > today.AddYears(-age))
                age--;

            return age < 0 ? 0 : age;
        }




        public static bool IsBirthdayValid(DateTime? birthday)
        {
            if (!birthday.HasValue)
                return false; 


            return birthday.Value.Date <= DateTime.Today;
        }



        public static string MemberIdGenerator()
        {
            string todaysDate = DateTime.Now.ToString("yyyyMMdd");

            using (var connection = new MySqlConnection("server=localhost;user id=root;password=;database=fitcheckdb;"))
            {
                connection.Open();
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM account WHERE MemberID LIKE @datePrefix", connection))
                {
                    cmd.Parameters.AddWithValue("@datePrefix", $"{todaysDate}-%");
                    int dailyCount = Convert.ToInt32(cmd.ExecuteScalar()) + 1;
                    return $"{todaysDate}-{dailyCount:D3}";
                }
            }
        }




    }
}
