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
    }
}
