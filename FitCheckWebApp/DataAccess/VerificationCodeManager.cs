using FitCheckWebApp.Models;
using MySql.Data.MySqlClient;

namespace FitCheckWebApp.DataAccess
{
    public class VerificationCodeManager
    {
        private static readonly string connectionString = "server=localhost;user id=root;password=;database=fitcheckdb;";

        public static string GenerateCode()
        {
            try
            {
                Random random = new Random();
                return random.Next(1000, 9999).ToString();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating verification code: {ex.Message}", ex);
            }
        }

        public static void SaveCode(string email, string code)
        {
            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();
                using var cmd = connection.CreateCommand();

                // Delete old codes for this email
                cmd.CommandText = "DELETE FROM VerificationCode WHERE Email = @Email";
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.ExecuteNonQuery();

                // Insert new code
                cmd.Parameters.Clear();
                cmd.CommandText = @"
                INSERT INTO VerificationCode (Email, Code, CreatedAt, ExpiresAt, IsUsed)
                VALUES (@Email, @Code, @CreatedAt, @ExpiresAt, 0)
            ";
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Code", code);
                cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                cmd.Parameters.AddWithValue("@ExpiresAt", DateTime.Now.AddMinutes(10)); // 10 minutes expiry
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            {
                throw new Exception($"Database error while saving verification code: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error saving verification code: {ex.Message}", ex);
            }
        }

        public static bool VerifyCode(string email, string code)
        {
            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"
                SELECT Id, ExpiresAt, IsUsed 
                FROM VerificationCode 
                WHERE Email = @Email AND Code = @Code
                ORDER BY CreatedAt DESC
                LIMIT 1
            ";
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Code", code);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    DateTime expiresAt = reader.GetDateTime("ExpiresAt");
                    bool isUsed = reader.GetBoolean("IsUsed");

                    if (isUsed)
                        return false;

                    if (DateTime.Now > expiresAt)
                        return false;

                    return true;
                }

                return false;
            }
            catch (MySqlException ex)
            {
                throw new Exception($"Database error while verifying code: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error verifying code: {ex.Message}", ex);
            }
        }

        public static void MarkCodeAsUsed(string email, string code)
        {
            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"
                UPDATE VerificationCode 
                SET IsUsed = 1 
                WHERE Email = @Email AND Code = @Code
            ";
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Code", code);
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            {
                throw new Exception($"Database error while marking code as used: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error marking code as used: {ex.Message}", ex);
            }
        }
    }
}