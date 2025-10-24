using System.Collections.Generic;
using System.Transactions;
using FitCheckWebApp.Models;
using MySql.Data.MySqlClient;
using Mysqlx.Crud;
using Transaction = FitCheckWebApp.Models.Transaction;

namespace FitCheckWebApp.DataAccess
{
    
    public class TransactionManager
    {

        private static readonly string connectionString = "server=localhost;user id=root;password=;database=fitcheckdb;";

        internal static void PostTransaction(Transaction transaction)
        {
            using (var connection = new MySqlConnection(connectionString))
            {

                connection.Open();

                using(var cmd = connection.CreateCommand())
                {
                    cmd.CommandText =
                        @"INSERT INTO `transaction` (AccountID, MembershipPlan, PaymentMethod, TransactionDate, StartDate, EndDate, Status)
                            VALUES (@accountId, @membershipPlan, @paymentMethod, @transactionDate, @startDate, @endDate, @status)";

                    cmd.Parameters.AddWithValue("@accountId", transaction.AccountID);
                    cmd.Parameters.AddWithValue("@membershipPlan", transaction.MembershipPlan.ToString());
                    cmd.Parameters.AddWithValue("@paymentMethod", transaction.PaymentMethod.ToString());
                    cmd.Parameters.AddWithValue("@transactionDate", transaction.TransactionDate);
                    cmd.Parameters.AddWithValue("@startDate", transaction.StartDate);
                    cmd.Parameters.AddWithValue("@endDate", transaction.EndDate);
                    cmd.Parameters.AddWithValue("@status", transaction.Status.ToString());


                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static Transaction? FindById(int id)
        {

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                using( var cmd = connection.CreateCommand())
                {
                    cmd.CommandText =
                    @"SELECT TransactionID, AccountID, MembershipPlan, PaymentMethod, TransactionDate, StartDate, EndDate, Status
                      FROM Transaction
                      WHERE TransactionID = @transactionId";

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

                        if(!Enum.TryParse(statusValue, out status))
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
                            Status = status
                        };

                    }

                    return null;

                }
            }

        }

        public static Transaction? FindLatestByAccount(int accountId)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
            SELECT * 
            FROM transaction
            WHERE AccountID = @AccountID
            ORDER BY EndDate DESC
            LIMIT 1;";

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
                                Status = Enum.Parse<FitCheckWebApp.Models.TransactionStatus>(reader.GetString("Status"))
                            };
                        }
                    }
                }
            }

            return null;
        }



        public static void ExpireOldMemberships()
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

        public static Transaction? FindLatestActiveByAccount(int accountId)
        {
            using var connection = new MySqlConnection(connectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();

            cmd.CommandText = @"
                SELECT TransactionID, AccountID, MembershipPlan, PaymentMethod, TransactionDate, StartDate, EndDate, Status
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
                    Status = status
                };

                return transaction;
            }

            return null;
        }

         internal static void UpdateTransaction(Transaction transaction)
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
                        Status = @status
                    WHERE TransactionID = @transactionId
                    ";


                    cmd.Parameters.AddWithValue("@membershipPlan", transaction.MembershipPlan.ToString());
                    cmd.Parameters.AddWithValue("@paymentMethod", transaction.PaymentMethod.ToString());
                    cmd.Parameters.AddWithValue("@transactionDate", transaction.TransactionDate);
                    cmd.Parameters.AddWithValue("@startDate", transaction.StartDate);
                    cmd.Parameters.AddWithValue("@endDate", transaction.EndDate);
                    cmd.Parameters.AddWithValue("@status", transaction.Status.ToString());
                    cmd.Parameters.AddWithValue("@transactionId", transaction.TransactionID);

                    cmd.ExecuteNonQuery();
                }
            }
        }



    }
}
