using System.Collections.Generic;
using System.Transactions;
using FitCheckWebApp.Models;
using FitCheckWebApp.ViewModels;
using MySql.Data.MySqlClient;
using Mysqlx.Crud;
using Transaction = FitCheckWebApp.Models.Transaction;
using TransactionStatus = FitCheckWebApp.Models.TransactionStatus;

namespace FitCheckWebApp.DataAccess
{
    public class TransactionManager
    {
        private static readonly string connectionString = "server=localhost;user id=root;password=;database=fitcheckdb;";

        internal static void PostTransaction(Transaction transaction)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = @"
                        INSERT INTO `transaction` 
                        (AccountID, MembershipPlan, PaymentMethod, TransactionDate, StartDate, EndDate, Amount, Status)
                        VALUES 
                        (@accountId, @membershipPlan, @paymentMethod, @transactionDate, @startDate, @endDate, @amount, @status)
                    ";

                        cmd.Parameters.AddWithValue("@accountId", transaction.AccountID);
                        cmd.Parameters.AddWithValue("@membershipPlan", transaction.MembershipPlan.ToString());
                        cmd.Parameters.AddWithValue("@paymentMethod", transaction.PaymentMethod.ToString());
                        cmd.Parameters.AddWithValue("@transactionDate", transaction.TransactionDate);
                        cmd.Parameters.AddWithValue("@startDate", transaction.StartDate);
                        cmd.Parameters.AddWithValue("@endDate", transaction.EndDate);
                        cmd.Parameters.AddWithValue("@amount", transaction.Amount);
                        cmd.Parameters.AddWithValue("@status", transaction.Status.ToString());

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception($"Database error while creating transaction: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating transaction: {ex.Message}", ex);
            }
        }

        public static Transaction? FindById(int id)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = @"
                        SELECT TransactionID, AccountID, MembershipPlan, PaymentMethod, TransactionDate, StartDate, EndDate, Amount, Status
                        FROM Transaction
                        WHERE TransactionID = @transactionId
                    ";

                        cmd.Parameters.AddWithValue("@transactionId", id);

                        using var reader = cmd.ExecuteReader();

                        if (reader.Read())
                        {
                            string? membershipValue = reader["MembershipPlan"]?.ToString();
                            string? paymentValue = reader["PaymentMethod"]?.ToString();
                            string? statusValue = reader["Status"]?.ToString();

                            MembershipPlan membershipPlan;
                            PaymentMethod paymentMethod;
                            Models.TransactionStatus status;

                            if (!Enum.TryParse(membershipValue, out membershipPlan))
                            {
                                membershipPlan = MembershipPlan.None;
                            }

                            if (!Enum.TryParse(paymentValue, out paymentMethod))
                            {
                                paymentMethod = PaymentMethod.None;
                            }

                            if (!Enum.TryParse(statusValue, out status))
                            {
                                status = Models.TransactionStatus.Active;
                            }

                            return new Transaction
                            {
                                TransactionID = reader.GetInt32("TransactionID"),
                                AccountID = reader.GetInt32("AccountID"),
                                MembershipPlan = membershipPlan,
                                PaymentMethod = paymentMethod,
                                TransactionDate = reader.GetDateTime("TransactionDate"),
                                StartDate = reader.GetDateTime("StartDate"),
                                EndDate = reader.GetDateTime("EndDate"),
                                Amount = reader.GetDecimal("Amount"),
                                Status = status
                            };
                        }

                        return null;
                    }
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception($"Database error while finding transaction by ID: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error finding transaction by ID: {ex.Message}", ex);
            }
        }

        public static Transaction? FindLatestByAccount(int accountId)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"
                    SELECT * 
                    FROM transaction
                    WHERE AccountID = @AccountID
                    ORDER BY EndDate DESC
                    LIMIT 1;
                ";

                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@AccountID", accountId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new Transaction
                                {
                                    TransactionID = reader.GetInt32("TransactionID"),
                                    AccountID = reader.GetInt32("AccountID"),
                                    MembershipPlan = Enum.Parse<FitCheckWebApp.Models.MembershipPlan>(reader.GetString("MembershipPlan")),
                                    PaymentMethod = Enum.Parse<FitCheckWebApp.Models.PaymentMethod>(reader.GetString("PaymentMethod")),
                                    StartDate = reader.GetDateTime("StartDate"),
                                    EndDate = reader.GetDateTime("EndDate"),
                                    TransactionDate = reader.GetDateTime("TransactionDate"),
                                    Status = Enum.Parse<FitCheckWebApp.Models.TransactionStatus>(reader.GetString("Status"), true)
                                };
                            }
                        }
                    }
                }

                return null;
            }
            catch (MySqlException ex)
            {
                throw new Exception($"Database error while finding latest transaction by account: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error finding latest transaction by account: {ex.Message}", ex);
            }
        }

        public static void ExpireOldMemberships()
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
                    UPDATE transaction
                    SET Status = 'Expired'
                    WHERE EndDate < NOW() AND Status != 'Expired';
                ";

                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception($"Database error while expiring old memberships: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error expiring old memberships: {ex.Message}", ex);
            }
        }

        public static Transaction? FindLatestActiveByAccount(int accountId)
        {
            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"
                SELECT TransactionID, AccountID, MembershipPlan, PaymentMethod, TransactionDate, StartDate, EndDate, Amount, Status
                FROM transaction
                WHERE AccountID = @accountId AND Status = 'Active'
                ORDER BY EndDate DESC
                LIMIT 1;
            ";

                cmd.Parameters.AddWithValue("@accountId", accountId);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    string? membershipValue = reader["MembershipPlan"]?.ToString();
                    string? paymentValue = reader["PaymentMethod"]?.ToString();
                    string? statusValue = reader["Status"]?.ToString();

                    Models.MembershipPlan membershipPlan;
                    Models.PaymentMethod paymentMethod;
                    Models.TransactionStatus status;

                    if (!Enum.TryParse(membershipValue, out membershipPlan))
                        membershipPlan = MembershipPlan.None;

                    if (!Enum.TryParse(paymentValue, out paymentMethod))
                        paymentMethod = PaymentMethod.None;

                    if (!Enum.TryParse(statusValue, out status))
                        status = Models.TransactionStatus.Active;

                    var transaction = new Transaction
                    {
                        TransactionID = reader.GetInt32("TransactionID"),
                        AccountID = reader.GetInt32("AccountID"),
                        MembershipPlan = membershipPlan,
                        PaymentMethod = paymentMethod,
                        TransactionDate = reader.GetDateTime("TransactionDate"),
                        StartDate = reader.GetDateTime("StartDate"),
                        EndDate = reader.GetDateTime("EndDate"),
                        Amount = reader.GetDecimal("Amount"),
                        Status = status
                    };

                    return transaction;
                }

                return null;
            }
            catch (MySqlException ex)
            {
                throw new Exception($"Database error while finding latest active transaction: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error finding latest active transaction: {ex.Message}", ex);
            }
        }

        internal static void UpdateTransaction(Transaction transaction)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = @"
                UPDATE `transaction`
                SET
                    MembershipPlan = @membershipPlan,
                    PaymentMethod = @paymentMethod,
                    TransactionDate = @transactionDate,
                    StartDate = @startDate,
                    EndDate = @endDate,
                    Amount = @amount,          
                    Status = @status
                WHERE TransactionID = @transactionId
            ";

                        cmd.Parameters.AddWithValue("@membershipPlan", transaction.MembershipPlan.ToString());
                        cmd.Parameters.AddWithValue("@paymentMethod", transaction.PaymentMethod.ToString());
                        cmd.Parameters.AddWithValue("@transactionDate", transaction.TransactionDate);
                        cmd.Parameters.AddWithValue("@startDate", transaction.StartDate);
                        cmd.Parameters.AddWithValue("@endDate", transaction.EndDate);
                        cmd.Parameters.AddWithValue("@amount", transaction.Amount);
                        cmd.Parameters.AddWithValue("@status", transaction.Status.ToString());
                        cmd.Parameters.AddWithValue("@transactionId", transaction.TransactionID);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception($"Database error while updating transaction: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating transaction: {ex.Message}", ex);
            }
        }

        // -------------------------
        // COUNT ACTIVE MEMBERS BLEHH
        // -------------------------
        public static int CountActiveMembers()
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = @"
                        SELECT COUNT(*) FROM transaction WHERE Status = @status";

                        cmd.Parameters.AddWithValue("@status", "Active");

                        var result = cmd.ExecuteScalar();

                        return Convert.ToInt32(result);
                    }
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception($"Database error while counting active members: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error counting active members: {ex.Message}", ex);
            }
        }

        // -------------------------
        // COUNT PENDING PAYMENTS RAAAA
        // -------------------------
        public static int CountPendingPayment()
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = @"
                        SELECT COUNT(*) FROM transaction WHERE Status = @status";

                        cmd.Parameters.AddWithValue("@status", "Pending");

                        var result = cmd.ExecuteScalar();

                        return Convert.ToInt32(result);
                    }
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception($"Database error while counting pending payments: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error counting pending payments: {ex.Message}", ex);
            }
        }

        // -------------------------
        // GET PENDING TRANSACTIONS
        // -------------------------
        public static List<Transaction> GetPendingTransactions()
        {
            try
            {
                var pending = new List<Transaction>();

                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                using var cmd = connection.CreateCommand();
                cmd.CommandText = @"
                SELECT t.TransactionID, t.AccountID, a.FirstName, a.LastName, 
                       t.MembershipPlan, t.PaymentMethod, t.Amount, t.Status, t.TransactionDate, t.StartDate, t.EndDate
                FROM transaction t
                INNER JOIN account a ON t.AccountID = a.Id
                WHERE t.Status = 'Pending'
                ORDER BY t.TransactionDate DESC;
            ";

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string? membershipValue = reader["MembershipPlan"]?.ToString();
                    string? paymentValue = reader["PaymentMethod"]?.ToString();
                    string? statusValue = reader["Status"]?.ToString();

                    MembershipPlan membershipPlan;
                    PaymentMethod paymentMethod;
                    TransactionStatus status;

                    Enum.TryParse(membershipValue, out membershipPlan);
                    Enum.TryParse(paymentValue, out paymentMethod);
                    Enum.TryParse(statusValue, out status);

                    pending.Add(new Transaction
                    {
                        TransactionID = reader.GetInt32("TransactionID"),
                        AccountID = reader.GetInt32("AccountID"),
                        AccountName = reader["FirstName"] + " " + reader["LastName"],
                        MembershipPlan = membershipPlan,
                        PaymentMethod = paymentMethod,
                        Amount = reader.GetDecimal("Amount"),
                        TransactionDate = reader.GetDateTime("TransactionDate"),
                        StartDate = reader.GetDateTime("StartDate"),
                        EndDate = reader.GetDateTime("EndDate"),
                        Status = status
                    });
                }

                return pending;
            }
            catch (MySqlException ex)
            {
                throw new Exception($"Database error while retrieving pending transactions: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving pending transactions: {ex.Message}", ex);
            }
        }

        public static List<Transaction> GetTransactionsByUser(int accountId)
        {
            try
            {
                var transactions = new List<Transaction>();

                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                using var cmd = connection.CreateCommand();
                cmd.CommandText = @"
                SELECT t.TransactionID, t.AccountID, t.MembershipPlan, t.PaymentMethod, 
                       t.Amount, t.Status, t.TransactionDate, t.StartDate, t.EndDate
                FROM transaction t
                WHERE t.AccountID = @accountId
                ORDER BY t.TransactionDate DESC;
            ";

                cmd.Parameters.AddWithValue("@accountId", accountId);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string? membershipValue = reader["MembershipPlan"]?.ToString();
                    string? paymentValue = reader["PaymentMethod"]?.ToString();
                    string? statusValue = reader["Status"]?.ToString();

                    MembershipPlan membershipPlan;
                    PaymentMethod paymentMethod;
                    TransactionStatus status;

                    Enum.TryParse(membershipValue, out membershipPlan);
                    Enum.TryParse(paymentValue, out paymentMethod);
                    Enum.TryParse(statusValue, out status);

                    transactions.Add(new Transaction
                    {
                        TransactionID = reader.GetInt32("TransactionID"),
                        AccountID = reader.GetInt32("AccountID"),
                        MembershipPlan = membershipPlan,
                        PaymentMethod = paymentMethod,
                        Amount = reader.GetDecimal("Amount"),
                        TransactionDate = reader.GetDateTime("TransactionDate"),
                        StartDate = reader.GetDateTime("StartDate"),
                        EndDate = reader.GetDateTime("EndDate"),
                        Status = status
                    });
                }

                return transactions;
            }
            catch (MySqlException ex)
            {
                throw new Exception($"Database error while retrieving user transactions: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving user transactions: {ex.Message}", ex);
            }
        }

        public static Transaction? GetActiveTransactionByAccountId(int accountId)
        {
            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"
                SELECT TransactionID, AccountID, MembershipPlan, PaymentMethod, TransactionDate, StartDate, EndDate, Amount, Status
                FROM transaction 
                WHERE AccountID = @AccountID 
                AND Status = 'Active' 
                AND EndDate >= NOW()
                ORDER BY EndDate DESC 
                LIMIT 1
            ";
                cmd.Parameters.AddWithValue("@AccountID", accountId);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    string? membershipValue = reader["MembershipPlan"]?.ToString();
                    string? paymentValue = reader["PaymentMethod"]?.ToString();
                    string? statusValue = reader["Status"]?.ToString();

                    MembershipPlan membershipPlan;
                    PaymentMethod paymentMethod;
                    TransactionStatus status;

                    if (!Enum.TryParse(membershipValue, out membershipPlan))
                        membershipPlan = MembershipPlan.None;

                    if (!Enum.TryParse(paymentValue, out paymentMethod))
                        paymentMethod = PaymentMethod.None;

                    if (!Enum.TryParse(statusValue, out status))
                        status = TransactionStatus.Active;

                    return new Transaction
                    {
                        TransactionID = reader.GetInt32("TransactionID"),
                        AccountID = reader.GetInt32("AccountID"),
                        MembershipPlan = membershipPlan,
                        PaymentMethod = paymentMethod,
                        TransactionDate = reader.GetDateTime("TransactionDate"),
                        StartDate = reader.GetDateTime("StartDate"),
                        EndDate = reader.GetDateTime("EndDate"),
                        Amount = reader.GetDecimal("Amount"),
                        Status = status
                    };
                }
                return null;
            }
            catch (MySqlException ex)
            {
                throw new Exception($"Database error while finding active transaction by account: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error finding active transaction by account: {ex.Message}", ex);
            }
        }
    }
}