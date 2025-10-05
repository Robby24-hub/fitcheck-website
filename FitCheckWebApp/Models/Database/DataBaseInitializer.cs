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

                    tableCmd.CommandText =
                        @"
                        CREATE TABLE IF NOT EXISTS Membership (
                            MembershipID INT AUTO_INCREMENT PRIMARY KEY,
                            MembershipName ENUM('FitStart', 'FitElite', 'FitPro') NOT NULL,
                            Price DECIMAL(10,2) DEFAULT 0.00,
                            DurationMonths INT DEFAULT 1,
                            Benefits TEXT
                        );";
                    tableCmd.ExecuteNonQuery();


                    tableCmd.CommandText =
                        @"
                        CREATE TABLE IF NOT EXISTS Account (
                            Id INT AUTO_INCREMENT PRIMARY KEY,
                            Username VARCHAR(100) NOT NULL UNIQUE,
                            PasswordHash TEXT NOT NULL,
                            Email VARCHAR(150),
                            Role ENUM('admin', 'trainer', 'user') DEFAULT 'user',
                            MembershipID INT NULL,
                            DateCreated DATETIME DEFAULT CURRENT_TIMESTAMP,
                            IsActive BOOLEAN DEFAULT TRUE,
                            FOREIGN KEY (MembershipID) REFERENCES Membership(MembershipID)
                        );";
                    tableCmd.ExecuteNonQuery();

                    tableCmd.CommandText =
                        @"
                        INSERT IGNORE INTO Membership (MembershipName, Price, DurationMonths, Benefits)
                        VALUES
                        ('FitStart', 0.00, 1, 'Basic plan'),
                        ('FitElite', 0.00, 3, 'Elite plan'),
                        ('FitPro', 0.00, 6, 'Pro plan');
                        ";
                    tableCmd.ExecuteNonQuery();

                }

                connection.Close();

            }
        }
    


    }
}
