using MailKit.Net.Smtp;
using MimeKit;
using MySql.Data.MySqlClient;

namespace FitCheckWebApp.Helpers
{
    public class EmailHelper
    {

        private static readonly string connectionString = "server=localhost;user id=root;password=;database=fitcheckdb;";


        public static void SendEmail(string toEmail, string subject, string body)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("FitCheck Gym", "fitcheck.noreply@gmail.com"));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            email.Body = new TextPart("html")
            {
                Text = body
            };

            using (var smtp = new SmtpClient())
            {
                smtp.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                smtp.Authenticate("fitcheckgymservice@gmail.com", "uwqp gjxn psvv asbi");
                smtp.Send(email);
                smtp.Disconnect(true);
            }
        }

        public static void SendMembershipExpiryWarnings()
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
                    SELECT t.TransactionID, t.AccountID, t.EndDate, a.Email, a.FirstName
                    FROM transaction t
                    JOIN account a ON t.AccountID = a.Id
                    WHERE t.Status = 'Active'
                    AND DATEDIFF(t.EndDate, NOW()) <= 3
                    AND DATEDIFF(t.EndDate, NOW()) >= 0;
                ";

                using (var cmd = new MySqlCommand(query, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string email = reader["Email"].ToString()!;
                        string name = reader["FirstName"].ToString()!;
                        DateTime endDate = Convert.ToDateTime(reader["EndDate"]);

                        string subject = "⏰💪 FitCheck Membership Expiring Soon!";
                        string body = $@"
                            <h2>Hi {name},</h2>
                            <p>Your <b>FitCheck Gym</b> membership will expire on <b>{endDate:MMMM dd, yyyy}</b>.</p>
                            <p>Please renew before it expires to avoid losing access to gym privileges.</p>
                            <br>
                            <p style='text-align:center;'>Click below to renew now:</p>
                            <p style='text-align:center;'>
                                <a href='https://your-fitcheck-domain.com/Transaction/UserMembership' 
                                   style='display:inline-block; background:#d2f801; color:#ffffff; 
                                          padding:12px 24px; border-radius:6px; text-decoration:none; 
                                          font-weight:bold; font-size:16px;'>
                                   Renew Membership
                                </a>
                            </p>
                            <br>
                            <p>Stay fit,</p>
                            <p><b>The FitCheck Team</b></p>
                        ";


                        try
                        {
                            EmailHelper.SendEmail(email, subject, body);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"❌ Failed to send email to {email}: {ex.Message}");
                        }
                    }
                }
            }
        }


        public static void SendVerificationCode(string toEmail, string code, string userName)
        {
            string subject = "🔐 Your FitCheck Password Reset Code";
            string body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #333;'>Hi {userName},</h2>
                    <p>You requested to change your password. Use the verification code below:</p>
            
                    <div style='background: #f5f5f5; padding: 20px; text-align: center; margin: 20px 0; border-radius: 8px;'>
                        <h1 style='color: #d2f801; font-size: 48px; letter-spacing: 10px; margin: 0;'>{code}</h1>
                    </div>
            
                    <p>This code will expire in <b>10 minutes</b>.</p>
                    <p>If you didn't request this, please ignore this email.</p>
            
                    <br>
                    <p>Stay secure,</p>
                    <p><b>The FitCheck Team</b></p>
                </div>
            ";

            SendEmail(toEmail, subject, body);
        }


        public static void SendProfileUpdateVerificationCode(string toEmail, string code, string userName)
        {
            string subject = "✏️ Verify Your FitCheck Profile Update";
            string body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #333;'>Hi {userName},</h2>
                    <p>You're about to update your profile information. To confirm this change, please use the verification code below:</p>
            
                    <div style='background: #f5f5f5; padding: 20px; text-align: center; margin: 20px 0; border-radius: 8px;'>
                        <h1 style='color: #d2f801; font-size: 48px; letter-spacing: 10px; margin: 0;'>{code}</h1>
                    </div>
            
                    <p>This code will expire in <b>10 minutes</b>.</p>
                    <p>If you didn't request this profile update, please contact us immediately.</p>
            
                    <br>
                    <p>Stay fit,</p>
                    <p><b>The FitCheck Team</b></p>
                </div>
            ";

            SendEmail(toEmail, subject, body);
        }

    }
}