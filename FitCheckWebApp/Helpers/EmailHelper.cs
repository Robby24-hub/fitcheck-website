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

            try
            {
                EmailHelper.SendEmail(toEmail, subject, body);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to send email to {toEmail}: {ex.Message}");
            }
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

            try
            {
                EmailHelper.SendEmail(toEmail, subject, body);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to send email to {toEmail}: {ex.Message}");
            }
        }

        public static void SendTransactionReceipt(string toEmail, string userName, string membershipPlan,
        decimal amount, DateTime transactionDate, DateTime endDate, string transactionId, string referenceNumber)
        {
            string subject = "Payment Confirmation - FitCheck Gym Membership";
            string body = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                </head>
                <body style='margin: 0; padding: 0; background-color: #f4f4f4; font-family: Arial, sans-serif;'>
                    <table role='presentation' style='width: 100%; border-collapse: collapse;'>
                        <tr>
                            <td align='center' style='padding: 40px 0;'>
                                <table role='presentation' style='width: 600px; border-collapse: collapse; background-color: #ffffff; box-shadow: 0 4px 6px rgba(0,0,0,0.1);'>
                            
                                    <!-- Header -->
                                    <tr>
                                        <td style='background: linear-gradient(135deg, #1a1a1a 0%, #2d2d2d 100%); padding: 30px; text-align: center;'>
                                            <h1 style='color: #d2f801; margin: 0; font-size: 32px; font-weight: bold; letter-spacing: 2px;'>FITCHECK</h1>
                                            <p style='color: #ffffff; margin: 5px 0 0 0; font-size: 14px;'>Gym & Fitness Center</p>
                                        </td>
                                    </tr>
                            
                                    <!-- Success Badge -->
                                    <tr>
                                        <td style='padding: 30px 30px 0 30px; text-align: center;'>
                                            <div style='display: inline-block; background-color: #d4edda; border: 2px solid #28a745; border-radius: 50px; padding: 10px 25px;'>
                                                <span style='color: #155724; font-size: 16px; font-weight: bold;'>✓ PAYMENT SUCCESSFUL</span>
                                            </div>
                                        </td>
                                    </tr>
                            
                                    <!-- Main Content -->
                                    <tr>
                                        <td style='padding: 30px;'>
                                            <h2 style='color: #333333; margin: 0 0 10px 0; font-size: 24px;'>Payment Receipt</h2>
                                            <p style='color: #666666; margin: 0 0 30px 0; font-size: 14px;'>Transaction Date: {transactionDate:MMMM dd, yyyy}</p>
                                    
                                            <p style='color: #555555; line-height: 1.6; margin: 0 0 20px 0; font-size: 16px;'>
                                                Dear {userName},
                                            </p>
                                    
                                            <p style='color: #555555; line-height: 1.6; margin: 0 0 30px 0; font-size: 16px;'>
                                                Thank you for your payment! Your membership has been successfully activated. Below are the details of your transaction:
                                            </p>
                                    
                                            <!-- Transaction Details Box -->
                                            <table role='presentation' style='width: 100%; border-collapse: collapse; border: 2px solid #e0e0e0; border-radius: 8px; margin: 20px 0;'>
                                                <tr>
                                                    <td colspan='2' style='background-color: #f8f8f8; padding: 15px; border-bottom: 2px solid #e0e0e0;'>
                                                        <h3 style='margin: 0; color: #333333; font-size: 18px;'>Transaction Details</h3>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td style='padding: 15px; border-bottom: 1px solid #e0e0e0; color: #666666; font-size: 14px; width: 40%;'>
                                                        <strong>Reference Number:</strong>
                                                    </td>
                                                    <td style='padding: 15px; border-bottom: 1px solid #e0e0e0; color: #333333; font-size: 14px;'>
                                                        {referenceNumber}
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td style='padding: 15px; border-bottom: 1px solid #e0e0e0; color: #666666; font-size: 14px;'>
                                                        <strong>Transaction ID:</strong>
                                                    </td>
                                                    <td style='padding: 15px; border-bottom: 1px solid #e0e0e0; color: #333333; font-size: 14px;'>
                                                        #{transactionId}
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td style='padding: 15px; border-bottom: 1px solid #e0e0e0; color: #666666; font-size: 14px;'>
                                                        <strong>Membership Plan:</strong>
                                                    </td>
                                                    <td style='padding: 15px; border-bottom: 1px solid #e0e0e0; color: #333333; font-size: 14px;'>
                                                        <span style='background-color: #d2f801; color: #1a1a1a; padding: 5px 15px; border-radius: 20px; font-weight: bold;'>{membershipPlan}</span>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td style='padding: 15px; border-bottom: 1px solid #e0e0e0; color: #666666; font-size: 14px;'>
                                                        <strong>Start Date:</strong>
                                                    </td>
                                                    <td style='padding: 15px; border-bottom: 1px solid #e0e0e0; color: #333333; font-size: 14px;'>
                                                        {transactionDate:MMMM dd, yyyy}
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td style='padding: 15px; border-bottom: 1px solid #e0e0e0; color: #666666; font-size: 14px;'>
                                                        <strong>Expiry Date:</strong>
                                                    </td>
                                                    <td style='padding: 15px; border-bottom: 1px solid #e0e0e0; color: #333333; font-size: 14px;'>
                                                        {endDate:MMMM dd, yyyy}
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td style='padding: 20px; background-color: #f8f8f8; color: #666666; font-size: 16px; font-weight: bold;'>
                                                        <strong>Total Amount Paid:</strong>
                                                    </td>
                                                    <td style='padding: 20px; background-color: #f8f8f8; color: #28a745; font-size: 24px; font-weight: bold;'>
                                                        ₱{amount:N2}
                                                    </td>
                                                </tr>
                                            </table>
                                    
                                            <!-- What's Next Section -->
                                            <table role='presentation' style='width: 100%; border-collapse: collapse; margin: 30px 0;'>
                                                <tr>
                                                    <td style='background-color: #e7f3ff; border-left: 4px solid #0066cc; padding: 20px; border-radius: 4px;'>
                                                        <h3 style='color: #0066cc; margin: 0 0 10px 0; font-size: 16px;'>📋 What's Next?</h3>
                                                        <ul style='color: #555555; line-height: 1.8; margin: 0; padding-left: 20px; font-size: 14px;'>
                                                            <li>Your membership is now active and ready to use</li>
                                                            <li>Visit our gym with your Member ID for access</li>
                                                            <li>Download your digital membership card from your account</li>
                                                            <li>Check out our class schedules and book your first session</li>
                                                        </ul>
                                                    </td>
                                                </tr>
                                            </table>
                                    
                                            <p style='color: #555555; line-height: 1.6; margin: 30px 0 0 0; font-size: 16px;'>
                                                We're excited to have you as part of the FitCheck community. Let's achieve your fitness goals together!
                                            </p>
                                        </td>
                                    </tr>
                            
                                    <!-- Footer -->
                                    <tr>
                                        <td style='background-color: #f8f8f8; padding: 30px; border-top: 1px solid #e0e0e0;'>
                                            <p style='color: #555555; margin: 0 0 15px 0; font-size: 14px; font-weight: bold;'>
                                                Need Help?
                                            </p>
                                            <p style='color: #555555; margin: 0 0 5px 0; font-size: 14px; line-height: 1.6;'>
                                                📧 Email: support@fitcheckgym.com<br>
                                                📞 Phone: (123) 456-7890<br>
                                                🏢 Address: 123 Fitness Street, Quezon City, Metro Manila<br>
                                                🌐 Website: www.fitcheckgym.com
                                            </p>
                                    
                                            <hr style='border: none; border-top: 1px solid #e0e0e0; margin: 20px 0;'>
                                    
                                            <p style='color: #888888; margin: 0; font-size: 12px; line-height: 1.5;'>
                                                This is an automated receipt. Please keep this email for your records.<br>
                                                If you have any questions about this transaction, please contact our support team.<br><br>
                                                © 2025 FitCheck Gym. All rights reserved.
                                            </p>
                                        </td>
                                    </tr>
                            
                                </table>
                            </td>
                        </tr>
                    </table>
                </body>
                </html>
            ";

            try
            {
                EmailHelper.SendEmail(toEmail, subject, body);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to send receipt email to {toEmail}: {ex.Message}");
            }
        }

    }
}