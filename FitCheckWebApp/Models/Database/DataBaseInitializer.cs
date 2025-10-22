using MySql.Data.MySqlClient;

namespace FitCheckWebApp.Models.Database
{
    public class DataBaseInitializer
    {

        private static readonly string connectionString = "server=localhost;user id=root;password=;";
        private static readonly string connectionString_Table = "server=localhost;user id=root;password=;database=fitcheckdb;";


        public static void InitializeDB()
        {

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText =
                        @"CREATE DATABASE IF NOT EXISTS fitcheckdb;";
                    cmd.ExecuteNonQuery();
                }

                connection.Close();
            }


            using (var connection = new MySqlConnection(connectionString_Table))
            {
                connection.Open();

                using (var tableCmd = connection.CreateCommand())
                {

                    tableCmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Account (
                            Id INT(11) AUTO_INCREMENT PRIMARY KEY,
                            MemberID VARCHAR(20) UNIQUE,
                            Username VARCHAR(100) NOT NULL UNIQUE,
                            PasswordHash TEXT NOT NULL,
                            Email VARCHAR(150) NOT NULL UNIQUE,
                            Role ENUM('admin','trainer','user') DEFAULT 'user',
                            DateCreated DATETIME DEFAULT CURRENT_TIMESTAMP,
                            IsActive TINYINT(1) DEFAULT 1,
                            MembershipPlan ENUM('FitStart','FitElite','FitPro') DEFAULT NULL,
                            FirstName VARCHAR(50),
                            LastName VARCHAR(50)
                        );";
                    tableCmd.ExecuteNonQuery();


                    tableCmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS `Transaction` (
                            TransactionID INT AUTO_INCREMENT PRIMARY KEY,
                            AccountID INT NOT NULL,
                            MembershipPlan ENUM('FitStart','FitElite','FitPro') NOT NULL,
                            PaymentMethod ENUM('credit','debit','cash') NOT NULL,
                            TransactionDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                            StartDate DATETIME NOT NULL,
                            EndDate DATETIME NOT NULL,
                            Status ENUM('Active','Expired','Cancelled') DEFAULT 'Active',
                            FOREIGN KEY (AccountID) REFERENCES Account(Id)
                        );";
                    tableCmd.ExecuteNonQuery();


                    tableCmd.CommandText = "SELECT COUNT(*) FROM Account WHERE Username = 'admin'";
                    long count = (long)tableCmd.ExecuteScalar();

                    if (count == 0)
                    {
                        tableCmd.CommandText = @"
                            INSERT INTO Account (MemberID, Username, PasswordHash, Email, Role, FirstName, LastName)
                            VALUES ('ADM001', 'admin', @PasswordHash, 'admin@fitcheck.com', 'admin', 'System', 'Admin');
                        ";
                        tableCmd.Parameters.AddWithValue("@PasswordHash", Helpers.Helpers.HashingPassword("admin123"));

                        tableCmd.ExecuteNonQuery();
                    }


                }

                connection.Close();

            }
        }
    


    }
}
