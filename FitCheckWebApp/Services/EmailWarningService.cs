using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using FitCheckWebApp.DataAccess;
using FitCheckWebApp.Helpers;

namespace FitCheckWebApp.Services
{
    public class EmailWarningService : BackgroundService
    {
        private readonly ILogger<EmailWarningService> _logger; 
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24); 

        public EmailWarningService(ILogger<EmailWarningService> logger) 
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("📬 EmailWarningService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    
                    TransactionManager.ExpireOldMemberships();

                    
                    EmailHelper.SendMembershipExpiryWarnings();

                    _logger.LogInformation("✅ Membership expiry check & emails done at {time}", DateTime.Now);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error during EmailWarningService execution.");
                }

                
                await Task.Delay(_checkInterval, stoppingToken);
            }
        }
    }
}
