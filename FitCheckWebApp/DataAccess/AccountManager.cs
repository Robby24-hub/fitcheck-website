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
                        @"INSERT INTO account 
                            (MemberID, Username, PasswordHash, Email, Role, DateCreated, IsActive, MembershipPlan, FirstName, LastName)
                        VALUES
                            (@memberId, @username, @passwordhash, @email, @role, @datecreated, @isactive, @membershipplan, @firstName, @lastName)";

                    cmd.Parameters.AddWithValue("@memberId", account.MemberID); 
                    cmd.Parameters.AddWithValue("@username", account.Username);
                    cmd.Parameters.AddWithValue("@passwordhash", account.PasswordHash);
                    cmd.Parameters.AddWithValue("@email", account.Email);
                    cmd.Parameters.AddWithValue("@role", account.Role);
                    cmd.Parameters.AddWithValue("@datecreated", account.DateCreated);
                    cmd.Parameters.AddWithValue("@isactive", account.IsActive);
                    cmd.Parameters.AddWithValue("@membershipplan", account.MembershipPlan.ToString());
                    cmd.Parameters.AddWithValue("@firstName", account.FirstName);
                    cmd.Parameters.AddWithValue("@lastName", account.LastName);

                    cmd.ExecuteNonQuery();
                }
            }


        }

        internal static void UpdateAccount(Account account)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
                UPDATE account
                SET MemberID = @memberId,
                    Username = @username,
                    PasswordHash = @passwordHash,
                    Email = @email,
                    Role = @role,
                    DateCreated = @dateCreated,
                    IsActive = @isActive,
                    MembershipPlan = @membershipPlan,
                    FirstName = @firstName,
                    LastName = @lastName
                WHERE Id = @id";

                    cmd.Parameters.AddWithValue("@memberId", account.MemberID);
                    cmd.Parameters.AddWithValue("@username", account.Username);
                    cmd.Parameters.AddWithValue("@passwordHash", account.PasswordHash);
                    cmd.Parameters.AddWithValue("@email", account.Email);
                    cmd.Parameters.AddWithValue("@role", account.Role);
                    cmd.Parameters.AddWithValue("@dateCreated", account.DateCreated);
                    cmd.Parameters.AddWithValue("@isActive", account.IsActive);
                    cmd.Parameters.AddWithValue("@membershipPlan", account.MembershipPlan.ToString());
                    cmd.Parameters.AddWithValue("@firstName", account.FirstName);
                    cmd.Parameters.AddWithValue("@lastName", account.LastName);
                    cmd.Parameters.AddWithValue("@id", account.Id);

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
                        @"SELECT Id, MemberID, Username, PasswordHash, Email, Role, DateCreated, IsActive, MembershipPlan, FirstName, LastName
                        FROM account
                        WHERE Email = @email";

                    cmd.Parameters.AddWithValue("@email", email);

                    using var reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        string? membershipValue = reader["MembershipPlan"]?.ToString();

                        MembershipPlan membershipPlan;
                        if (!Enum.TryParse(membershipValue, out membershipPlan))
                        {
                            membershipPlan = MembershipPlan.None;
                        }

                        return new Account
                        {
                            Id = reader.GetInt32("Id"),
                            MemberID = reader["MemberID"]?.ToString(),
                            Username = reader.GetString("Username"),
                            PasswordHash = reader.GetString("PasswordHash"),
                            Email = reader.GetString("Email"),
                            Role = reader.GetString("Role"),
                            DateCreated = reader.GetDateTime("DateCreated"),
                            IsActive = reader.GetBoolean("IsActive"),
                            MembershipPlan = membershipPlan,
                            FirstName = reader["FirstName"]?.ToString(),
                            LastName = reader["LastName"]?.ToString()
                        };
                    }

                    return null;
                }
            }


        }

        public static Account? FindById(int id)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText =
                        @"SELECT Id, MemberID, Username, PasswordHash, Email, Role, DateCreated, IsActive, MembershipPlan, FirstName, LastName
                        FROM account
                        WHERE Id = @id";

                    cmd.Parameters.AddWithValue("@id", id);

                    using var reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        string? membershipValue = reader["MembershipPlan"]?.ToString();

                        MembershipPlan membershipPlan;
                        if (!Enum.TryParse(membershipValue, out membershipPlan))
                        {
                            membershipPlan = MembershipPlan.None;
                        }

                        return new Account
                        {
                            Id = reader.GetInt32("Id"),
                            MemberID = reader["MemberID"]?.ToString(),
                            Username = reader.GetString("Username"),
                            PasswordHash = reader.GetString("PasswordHash"),
                            Email = reader.GetString("Email"),
                            Role = reader.GetString("Role"),
                            DateCreated = reader.GetDateTime("DateCreated"),
                            IsActive = reader.GetBoolean("IsActive"),
                            MembershipPlan = membershipPlan,
                            FirstName = reader["FirstName"]?.ToString(),
                            LastName = reader["LastName"]?.ToString()
                        };
                    }

                    return null;
                }
            }
        }


    }
}
    