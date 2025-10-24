using FitCheckWebApp.Models;
using MySql.Data.MySqlClient;

namespace FitCheckWebApp.DataAccess
{
    public class AccountManager
    {
        private static readonly string connectionString = "server=localhost;user id=root;password=;database=fitcheckdb;";

    // -------------------------
    // CREATE / REGISTER ACCOUNT
    // -------------------------
    internal static void PostAccount(Account account)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
                INSERT INTO account 
                (MemberID, Username, PasswordHash, Email, Role, DateCreated, IsActive, MembershipPlan,
                 FirstName, LastName, BirthDate, Gender, ContactNumber, EmergencyName, EmergencyContact)
                VALUES
                (@memberId, @username, @passwordhash, @email, @role, @datecreated, @isactive, @membershipplan,
                 @firstName, @lastName, @birthday, @gender, @contactNumber, @emergencyName, @emergencyContact)";

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
                    cmd.Parameters.AddWithValue("@birthday", account.BirthDate);
                    cmd.Parameters.AddWithValue("@gender", account.Gender);
                    cmd.Parameters.AddWithValue("@contactNumber", account.ContactNumber);
                    cmd.Parameters.AddWithValue("@emergencyName", account.EmergencyName);
                    cmd.Parameters.AddWithValue("@emergencyContact", account.EmergencyContact);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        // -------------------------
        // UPDATE EXISTING ACCOUNT
        // -------------------------
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
                    LastName = @lastName,
                    BirthDate = @birthday,
                    Gender = @gender,
                    ContactNumber = @contactNumber,
                    EmergencyName = @emergencyName,
                    EmergencyContact = @emergencyContact
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
                    cmd.Parameters.AddWithValue("@birthday", account.BirthDate);
                    cmd.Parameters.AddWithValue("@gender", account.Gender);
                    cmd.Parameters.AddWithValue("@contactNumber", account.ContactNumber);
                    cmd.Parameters.AddWithValue("@emergencyName", account.EmergencyName);
                    cmd.Parameters.AddWithValue("@emergencyContact", account.EmergencyContact);
                    cmd.Parameters.AddWithValue("@id", account.Id);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        // -------------------------
        // FIND BY EMAIL
        // -------------------------
        public static Account? FindByEmail(string email)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
                SELECT Id, MemberID, Username, PasswordHash, Email, Role, DateCreated, IsActive, MembershipPlan,
                       FirstName, LastName, BirthDate, Gender, ContactNumber, EmergencyName, EmergencyContact
                FROM account
                WHERE Email = @email";

                    cmd.Parameters.AddWithValue("@email", email);
                    using var reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        string? membershipValue = reader["MembershipPlan"]?.ToString();
                        Enum.TryParse(membershipValue, out MembershipPlan membershipPlan);

                        return new Account
                        {
                            Id = reader.GetInt32("Id"),
                            MemberID = reader["MemberID"]?.ToString(),
                            Username = reader["Username"].ToString(),
                            PasswordHash = reader["PasswordHash"].ToString(),
                            Email = reader["Email"].ToString(),
                            Role = reader["Role"].ToString(),
                            DateCreated = reader.GetDateTime("DateCreated"),
                            IsActive = reader.GetBoolean("IsActive"),
                            MembershipPlan = membershipPlan,
                            FirstName = reader["FirstName"]?.ToString(),
                            LastName = reader["LastName"]?.ToString(),
                            BirthDate = reader["BirthDate"] as DateTime?,
                            Gender = reader["Gender"]?.ToString(),
                            ContactNumber = reader["ContactNumber"]?.ToString(),
                            EmergencyName = reader["EmergencyName"]?.ToString(),
                            EmergencyContact = reader["EmergencyContact"]?.ToString()
                        };
                    }
                    return null;
                }
            }
        }

        // -------------------------
        // FIND BY ID
        // -------------------------
        public static Account? FindById(int id)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
                SELECT Id, MemberID, Username, PasswordHash, Email, Role, DateCreated, IsActive, MembershipPlan,
                       FirstName, LastName, BirthDate, Gender, ContactNumber, EmergencyName, EmergencyContact
                FROM account
                WHERE Id = @id";

                    cmd.Parameters.AddWithValue("@id", id);
                    using var reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        string? membershipValue = reader["MembershipPlan"]?.ToString();
                        Enum.TryParse(membershipValue, out MembershipPlan membershipPlan);

                        return new Account
                        {
                            Id = reader.GetInt32("Id"),
                            MemberID = reader["MemberID"]?.ToString(),
                            Username = reader["Username"].ToString(),
                            PasswordHash = reader["PasswordHash"].ToString(),
                            Email = reader["Email"].ToString(),
                            Role = reader["Role"].ToString(),
                            DateCreated = reader.GetDateTime("DateCreated"),
                            IsActive = reader.GetBoolean("IsActive"),
                            MembershipPlan = membershipPlan,
                            FirstName = reader["FirstName"]?.ToString(),
                            LastName = reader["LastName"]?.ToString(),
                            BirthDate = reader["BirthDate"] as DateTime?,
                            Gender = reader["Gender"]?.ToString(),
                            ContactNumber = reader["ContactNumber"]?.ToString(),
                            EmergencyName = reader["EmergencyName"]?.ToString(),
                            EmergencyContact = reader["EmergencyContact"]?.ToString()
                        };
                    }
                    return null;
                }
            }
        }
    }

}
