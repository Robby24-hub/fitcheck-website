using FitCheckWebApp.Models;
using MySql.Data.MySqlClient;

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
                    cmd.Parameters.AddWithValue("@status", transaction.Status);

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
                            WHERE AccountID = @accountId";

                    cmd.Parameters.AddWithValue("@accountId", id);

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
                        {
                            membershipPlan = MembershipPlan.None;
                        }

                        if (!Enum.TryParse(paymentValue, out paymentMethod))
                        {
                            paymentMethod = PaymentMethod.None;
                        }

                        if(!Enum.TryParse(statusValue, out status))
                        {
                            status = TransactionStatus.Active;
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
    }
}
