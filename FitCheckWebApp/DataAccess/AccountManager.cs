using FitCheckWebApp.Models;
using FitCheckWebApp.ViewModels.Admin;
using MySql.Data.MySqlClient;
using Mysqlx.Crud;

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
            try
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
            catch (MySqlException ex)
            {
                throw new Exception($"Database error while creating account: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating account: {ex.Message}", ex);
            }
        }

        // -------------------------
        // UPDATE EXISTING ACCOUNT
        // -------------------------
        internal static void UpdateAccount(Account account)
        {
            try
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
            catch (MySqlException ex)
            {
                throw new Exception($"Database error while updating account: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating account: {ex.Message}", ex);
            }
        }

        // -------------------------
        // FIND BY EMAIL
        // -------------------------
        public static Account? FindByEmail(string email)
        {
            try
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
            catch (MySqlException ex)
            {
                throw new Exception($"Database error while finding account by email: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error finding account by email: {ex.Message}", ex);
            }
        }

        // -------------------------
        // FIND BY ID
        // -------------------------
        public static Account? FindById(int id)
        {
            try
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
                        WHERE Id = @id
                    ";

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
            catch (MySqlException ex)
            {
                throw new Exception($"Database error while finding account by ID: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error finding account by ID: {ex.Message}", ex);
            }
        }

        public static List<MemberViewModel> GetAllMembers()
        {
            try
            {
                var members = new List<MemberViewModel>();

                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                using var cmd = connection.CreateCommand();
                cmd.CommandText = @"
                SELECT a.Id, a.MemberID, a.FirstName, a.LastName, a.Email, a.MembershipPlan,
                       t.EndDate AS PaymentDue, a.IsActive
                FROM account a
                LEFT JOIN transaction t ON t.AccountID = a.Id
                WHERE a.IsActive = 1 AND Role != 'admin'
                ORDER BY a.FirstName;
            ";

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    members.Add(new MemberViewModel
                    {
                        AccountID = reader.GetInt32("Id"),
                        MemberID = reader["MemberID"]?.ToString() ?? "",
                        Name = reader["FirstName"] + " " + reader["LastName"],
                        Email = reader["Email"]?.ToString() ?? "",
                        MembershipPlan = reader["MembershipPlan"]?.ToString() ?? "",
                        Status = (reader["IsActive"] != DBNull.Value && Convert.ToBoolean(reader["IsActive"]))
                            ? "Active"
                            : "Inactive",
                        PaymentDue = reader["PaymentDue"] != DBNull.Value
                            ? (DateTime?)reader.GetDateTime("PaymentDue")
                            : null
                    });
                }

                return members;
            }
            catch (MySqlException ex)
            {
                throw new Exception($"Database error while retrieving all members: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving all members: {ex.Message}", ex);
            }
        }

        public static void DeleteAccount(int accountId)
        {
            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                using var cmd = connection.CreateCommand();
                cmd.CommandText = "DELETE FROM account WHERE Id = @accountId";
                cmd.Parameters.AddWithValue("@accountId", accountId);

                cmd.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            {
                throw new Exception($"Database error while deleting account: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting account: {ex.Message}", ex);
            }
        }

        public static void SoftDeleteMember(int accountId)
        {
            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                using var transaction = connection.BeginTransaction();

                using var cmd = connection.CreateCommand();
                cmd.Transaction = transaction;

                try
                {
                    cmd.CommandText = @"
                    UPDATE account 
                    SET IsActive = 0
                    WHERE Id = @id AND Role != 'admin';
                ";
                    cmd.Parameters.AddWithValue("@id", accountId);
                    cmd.ExecuteNonQuery();

                    cmd.Parameters.Clear();
                    cmd.CommandText = @"
                    UPDATE transaction
                    SET Status = 'Cancelled'
                    WHERE AccountID = @id;
                ";
                    cmd.Parameters.AddWithValue("@id", accountId);
                    cmd.ExecuteNonQuery();

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception($"Database error while soft-deleting member: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error soft-deleting member: {ex.Message}", ex);
            }
        }

        public static List<Account> GetAllAccounts()
        {
            try
            {
                var accounts = new List<Account>();

                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "SELECT Id, FirstName, LastName, Role FROM Account WHERE IsActive = 1";
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var acc = new Account
                                {
                                    Id = reader.GetInt32("Id"),
                                    FirstName = reader["FirstName"]?.ToString() ?? "",
                                    LastName = reader["LastName"]?.ToString() ?? "",
                                    Role = reader["Role"]?.ToString() ?? ""
                                };
                                accounts.Add(acc);
                            }
                        }
                    }
                }

                return accounts;
            }
            catch (MySqlException ex)
            {
                throw new Exception($"Database error while retrieving all accounts: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving all accounts: {ex.Message}", ex);
            }
        }
    }
}