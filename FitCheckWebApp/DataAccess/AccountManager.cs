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
                        @"INSERT INTO account (Username, PasswordHash, Email, Role, DateCreated, IsActive, MembershipPlan)
                            VALUES(@username, @passwordhash, @email, @role, @datecreated, @isactive, @membershipplan)";

                    cmd.Parameters.AddWithValue("@username", account.Username);
                    cmd.Parameters.AddWithValue("@passwordhash", account.PasswordHash);
                    cmd.Parameters.AddWithValue("@email", account.Email);
                    cmd.Parameters.AddWithValue("@role", account.Role);
                    cmd.Parameters.AddWithValue("@datecreated", account.DateCreated);
                    cmd.Parameters.AddWithValue("@isactive", account.IsActive);
                    cmd.Parameters.AddWithValue("@membershipplan", account.MembershipPlan);

                    cmd.ExecuteNonQuery();
                }
            }

        }

        public static Account? FindByEmail(string email)
        {

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                using (var cmd = connection.CreateCommand())
                {

                    cmd.CommandText =
                        @"SELECT Id, Username, PasswordHash, Email, Role, DateCreated, IsActive, MembershipPlan
                            FROM account
                            WHERE Email = @email";

                    cmd.Parameters.AddWithValue("@email", email);

                    using var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        string parsedMembershipPlan = reader.GetString("MembershipPlan");


                        if (Enum.TryParse(parsedMembershipPlan, out MembershipPlan membershipPlan))
                        {

                            return new Account
                            {
                                Id = reader.GetInt32("Id"),
                                Username = reader.GetString("Username"),
                                PasswordHash = reader.GetString("PasswordHash"),
                                Email = reader.GetString("Email"),
                                Role = reader.GetString("Role"),
                                DateCreated = reader.GetDateTime("DateCreated"),
                                IsActive = reader.GetBoolean("IsActive"),
                                MembershipPlan = membershipPlan
                            };
                        } else
                        {
                            throw new InvalidOperationException($"Invalid MembershipPlan value: {parsedMembershipPlan}");
                        }

                       
                    }

                    return null;


                }
            }

        }

    }
}
    