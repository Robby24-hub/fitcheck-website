using FitCheckWebApp.Models;
using MySql.Data.MySqlClient;

namespace FitCheckWebApp.DataAccess
{
    public class AccountManager
    {

        private static readonly string connectionString = "server=localhost;user id=root;password=;database=fitcheckdb;";

        internal static void PostAccount(Account account)
        {

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                using (var cmd = connection.CreateCommand())
                {

                    cmd.CommandText =
                        @"INSERT INTO account (Username, PasswordHash, Email, Role, MembershipID, DateCreated, IsActive)
                            VALUES(@username, @passwordhash, @email, @role, @membershipid, @datecreated, @isactive)";

                    cmd.Parameters.AddWithValue("@username", account.Username);
                    cmd.Parameters.AddWithValue("@passwordhash", account.PasswordHash);
                    cmd.Parameters.AddWithValue("@email", account.Email);
                    cmd.Parameters.AddWithValue("@role", account.Role);
                    cmd.Parameters.AddWithValue("@membershipid", account.MembershipID);
                    cmd.Parameters.AddWithValue("@datecreated", account.DateCreated);
                    cmd.Parameters.AddWithValue("@isactive", account.IsActive);

                    cmd.ExecuteNonQuery();
                }
            }

        }

    }
}
    