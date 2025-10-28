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
                            LastName VARCHAR(50),
                            BirthDate DATE DEFAULT NULL,
                            Age INT DEFAULT NULL,
                            Gender ENUM('Male','Female','Other') DEFAULT NULL,
                            ContactNumber VARCHAR(20) DEFAULT NULL,
                            EmergencyName VARCHAR(100) DEFAULT NULL,
                            EmergencyContact VARCHAR(20) DEFAULT NULL
                        );";
                    tableCmd.ExecuteNonQuery();


                    tableCmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS `Transaction` (
                            TransactionID INT AUTO_INCREMENT PRIMARY KEY,
                            AccountID INT NOT NULL,
                            MembershipPlan ENUM('FitStart','FitElite','FitPro') NOT NULL,
                            PaymentMethod ENUM('Credit','Debit','Cash','None') NOT NULL,
                            TransactionDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                            StartDate DATETIME NOT NULL,
                            EndDate DATETIME NOT NULL,
                            Amount DECIMAL(10,2) NOT NULL,
                            Status ENUM('Active','Expired','Cancelled', 'Pending', 'Declined') DEFAULT 'Active',
                            FOREIGN KEY (AccountID) REFERENCES Account(Id)
                        );";
                    tableCmd.ExecuteNonQuery();


                    tableCmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Class (
                        Id INT AUTO_INCREMENT PRIMARY KEY,
                        AccountID INT NOT NULL,                       
                        Day ENUM('Monday','Tuesday','Wednesday','Thursday','Friday','Saturday','Sunday') NOT NULL,
                        Type ENUM('Yoga','Zumba','Pilates','CrossFit','HIIT','StrengthTraining','Cardio','DanceFitness') NOT NULL,
                        Time TIME NOT NULL,
                        DurationMinutes INT NOT NULL,
                        ParticipantLimit INT NOT NULL,
                        ParticipantsCount INT DEFAULT 0,
                        FOREIGN KEY (AccountID) REFERENCES Account(Id)
                    );";
                    tableCmd.ExecuteNonQuery();

                    tableCmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS VerificationCode (
                        Id INT AUTO_INCREMENT PRIMARY KEY,
                        Email VARCHAR(150) NOT NULL,
                        Code VARCHAR(4) NOT NULL,
                        CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                        ExpiresAt DATETIME NOT NULL,
                        IsUsed TINYINT(1) DEFAULT 0,
                        INDEX idx_email_code (Email, Code)
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

                    

                    tableCmd.CommandText = "SELECT COUNT(*) FROM Account WHERE Username = 'magnamalo2006'";
                    long trainer1Count = (long)tableCmd.ExecuteScalar();
                    if (trainer1Count == 0)
                    {
                        tableCmd.CommandText = @"
                            INSERT INTO Account (Username, PasswordHash, Email, Role, FirstName, LastName, IsActive, DateCreated)
                            VALUES ('magnamalo2006', @PasswordHash1, 'rex.taeza@fitcheck.com', 'trainer', 'Rex', 'Taeza', 1, NOW());
                        ";
                        tableCmd.Parameters.Clear();
                        tableCmd.Parameters.AddWithValue("@PasswordHash1", Helpers.Helpers.HashingPassword("trainer123"));
                        tableCmd.ExecuteNonQuery();
                    }


                    tableCmd.CommandText = "SELECT COUNT(*) FROM Account WHERE Username = 'rcandano'";
                    long trainer2Count = (long)tableCmd.ExecuteScalar();
                    if (trainer2Count == 0)
                    {
                        tableCmd.CommandText = @"
                            INSERT INTO Account (Username, PasswordHash, Email, Role, FirstName, LastName, IsActive, DateCreated)
                            VALUES ('rcandano', @PasswordHash2, 'robby.candano@fitcheck.com', 'trainer', 'Robby', 'Candano', 1, NOW());
                        ";
                        tableCmd.Parameters.Clear();
                        tableCmd.Parameters.AddWithValue("@PasswordHash2", Helpers.Helpers.HashingPassword("trainer123"));
                        tableCmd.ExecuteNonQuery();
                    }

  
                    tableCmd.CommandText = "SELECT COUNT(*) FROM Account WHERE Username = 'mlitang'";
                    long trainer3Count = (long)tableCmd.ExecuteScalar();
                    if (trainer3Count == 0)
                    {
                        tableCmd.CommandText = @"
                            INSERT INTO Account (Username, PasswordHash, Email, Role, FirstName, LastName, IsActive, DateCreated)
                            VALUES ('mlitang', @PasswordHash3, 'michael.litang@fitcheck.com', 'trainer', 'Michael', 'Litang', 1, NOW());
                        ";
                        tableCmd.Parameters.Clear();
                        tableCmd.Parameters.AddWithValue("@PasswordHash3", Helpers.Helpers.HashingPassword("trainer123"));
                        tableCmd.ExecuteNonQuery();
                    }


                    tableCmd.CommandText = "SELECT COUNT(*) FROM Account WHERE Username = 'ssollano'";
                    long trainer4Count = (long)tableCmd.ExecuteScalar();
                    if (trainer4Count == 0)
                    {
                        tableCmd.CommandText = @"
                            INSERT INTO Account (Username, PasswordHash, Email, Role, FirstName, LastName, IsActive, DateCreated)
                            VALUES ('ssollano', @PasswordHash4, 'shaira.sollano@fitcheck.com', 'trainer', 'Shaira', 'Sollano', 1, NOW());
                        ";
                        tableCmd.Parameters.Clear();
                        tableCmd.Parameters.AddWithValue("@PasswordHash4", Helpers.Helpers.HashingPassword("trainer123"));
                        tableCmd.ExecuteNonQuery();
                    }

                }

                connection.Close();

            }
        }
    


    }
}
