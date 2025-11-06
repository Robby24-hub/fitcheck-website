using System.Diagnostics;
using System.Security.Claims;
using FitCheckWebApp.DataAccess;
using FitCheckWebApp.Helpers;
using FitCheckWebApp.Models;
using FitCheckWebApp.ViewModels.Account;
using FitCheckWebApp.ViewModels.Transaction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitCheckWebApp.Controllers
{
    public class TransactionController : Controller
    {
        // ==== MemberShipChoice

        [Authorize(Roles = "admin")]
        public IActionResult TestWarningEmails()
        {
            TransactionManager.ExpireOldMemberships();
            EmailHelper.SendMembershipExpiryWarnings();
            return Content("Warning emails triggered manually.");
        }



        [HttpGet, Authorize]
        public IActionResult PaymentMethod(string plan)
        {
            var model = new TransactionViewModel();

            if (!string.IsNullOrEmpty(plan) && Enum.TryParse(plan, true, out MembershipPlan selectedPlan))
            {
                model.MembershipPlan = selectedPlan;
            }
            else
            {
                model.MembershipPlan = MembershipPlan.None;
            }


            bool isUpgrade = TempData["IsUpgrade"] as bool? ?? false;

            Console.WriteLine($"=== PAYMENT METHOD DEBUG ===");
            Console.WriteLine($"IsUpgrade from TempData: {isUpgrade}");
            Console.WriteLine($"TempData['UpgradeAmount']: {TempData["UpgradeAmount"]}");

            if (isUpgrade)
            {
               
                string upgradeAmountStr = TempData["UpgradeAmount"] as string ?? "0";
                decimal.TryParse(upgradeAmountStr, out decimal upgradeCost);

                model.Amount = upgradeCost;
                model.IsUpgrade = true;
                model.CurrentPlan = TempData["CurrentPlan"] as string;

                Console.WriteLine($"Model.Amount set to: {model.Amount}");
                Console.WriteLine($"Model.IsUpgrade: {model.IsUpgrade}");
                Console.WriteLine($"Model.CurrentPlan: {model.CurrentPlan}");

                TempData.Keep("IsUpgrade");
                TempData.Keep("UpgradeAmount");
                TempData.Keep("UpgradePlan");
                TempData.Keep("CurrentPlan");
            }
            else
            {
       
                model.Amount = model.MembershipPlan switch
                {
                    MembershipPlan.FitStart => 999m,
                    MembershipPlan.FitPro => 1499m, 
                    MembershipPlan.FitElite => 2499m,
                    _ => 0m
                };
                model.IsUpgrade = false;
                Console.WriteLine($"Regular subscription - Amount: {model.Amount}");
            }

            return View(model);
        }


        [HttpPost, Authorize]
        public IActionResult PaymentMethod(TransactionViewModel newtransaction)
        {
            int accountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (!ModelState.IsValid)
            {
                // Preserve the necessary data when returning the view
                newtransaction.IsUpgrade = TempData["IsUpgrade"] as bool? ?? false;
                newtransaction.CurrentPlan = TempData["CurrentPlan"] as string;

                // If it's an upgrade, preserve the amount
                if (newtransaction.IsUpgrade)
                {
                    string upgradeAmountStr = TempData["UpgradeAmount"] as string ?? "0";
                    decimal.TryParse(upgradeAmountStr, out decimal upgradeCost);
                    newtransaction.Amount = upgradeCost;
                }

                return View(newtransaction);
            }
            if (!User.Identity!.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            var lastTransaction = TransactionManager.FindLatestActiveByAccount(accountId);

            bool isRenewal = TempData["IsRenewal"] != null && (bool)TempData["IsRenewal"];
            bool isExtension = lastTransaction != null && lastTransaction.Status == TransactionStatus.Active;
            bool isUpgrade = TempData["IsUpgrade"] != null && (bool)TempData["IsUpgrade"];

            decimal upgradeAmount = 0m;
            if (TempData["UpgradeAmount"] != null)
            {
                upgradeAmount = decimal.Parse((string)TempData["UpgradeAmount"]);
            }

            if (isRenewal && lastTransaction != null)
            {
                newtransaction.MembershipPlan = lastTransaction.MembershipPlan;
            }

            var account = AccountManager.FindById(accountId);


            if (isUpgrade && lastTransaction != null)
            {
                Console.WriteLine($">>> UPGRADE: Canceling old transaction {lastTransaction.TransactionID} and creating new one");


                lastTransaction.Status = TransactionStatus.Cancelled;
                TransactionManager.UpdateTransaction(lastTransaction);


                DateTime startDate = DateTime.Now; 
                DateTime endDate = lastTransaction.EndDate; 

                decimal fullNewAmount = newtransaction.MembershipPlan switch
                {
                    MembershipPlan.FitStart => 999m,
                    MembershipPlan.FitPro => 1499m,
                    MembershipPlan.FitElite => 2499m,
                    _ => 0m
                };

                var status = newtransaction.PaymentMethod.ToString() == "Cash"
                    ? TransactionStatus.Pending
                    : TransactionStatus.Active;

                var upgradeTransaction = new Transaction
                {
                    AccountID = accountId,
                    MembershipPlan = newtransaction.MembershipPlan,
                    PaymentMethod = newtransaction.PaymentMethod,
                    StartDate = startDate,
                    EndDate = endDate,
                    TransactionDate = DateTime.Now,
                    Status = status,
                    Amount = upgradeAmount 
                };

                TransactionManager.PostTransaction(upgradeTransaction);


                if (account != null && status == TransactionStatus.Active)
                {
                    account.MembershipPlan = newtransaction.MembershipPlan;
                    AccountManager.UpdateAccount(account);


                    try
                    {
                        var savedTransaction = TransactionManager.FindLatestActiveByAccount(accountId);
                        string fullName = $"{account.FirstName} {account.LastName}";
                        string referenceNumber = $"REF-UPG-{DateTime.Now:yyyyMMdd}-{savedTransaction?.TransactionID ?? 0:D6}";

                        EmailHelper.SendTransactionReceipt(
                            toEmail: account.Email!,
                            userName: fullName,
                            membershipPlan: newtransaction.MembershipPlan.ToString(),
                            amount: upgradeAmount,
                            transactionDate: upgradeTransaction.TransactionDate,
                            endDate: endDate,
                            transactionId: (savedTransaction?.TransactionID ?? 0).ToString(),
                            referenceNumber: referenceNumber
                        );

                        Console.WriteLine($"✅ Upgrade receipt sent to {account.Email}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Email failed: {ex.Message}");
                    }
                }

                return RedirectToAction("UserMembership");
            }

            DateTime regularStartDate;
            if (isExtension && lastTransaction != null)
            {
                regularStartDate = lastTransaction.EndDate.AddDays(1);
            }
            else
            {
                regularStartDate = DateTime.Now;
            }

            DateTime regularEndDate = regularStartDate.AddMonths(1);

            decimal amount = newtransaction.MembershipPlan switch
            {
                MembershipPlan.FitStart => 999m,
                MembershipPlan.FitPro => 1499m,
                MembershipPlan.FitElite => 2499m,
                _ => 0m
            };

            var transactionStatus = newtransaction.PaymentMethod.ToString() == "Cash"
                ? TransactionStatus.Pending
                : TransactionStatus.Active;

            var transaction = new Transaction
            {
                AccountID = accountId,
                MembershipPlan = newtransaction.MembershipPlan,
                PaymentMethod = newtransaction.PaymentMethod,
                StartDate = regularStartDate,
                EndDate = regularEndDate,
                TransactionDate = DateTime.Now,
                Status = transactionStatus,
                Amount = amount
            };

            TransactionManager.PostTransaction(transaction);

            if (transaction.Status == TransactionStatus.Active)
            {
                if (account != null)
                {
                    if (string.IsNullOrEmpty(account.MemberID))
                    {
                        account.MemberID = Helpers.Helpers.MemberIdGenerator();
                    }
                    account.MembershipPlan = newtransaction.MembershipPlan;
                    AccountManager.UpdateAccount(account);


                    try
                    {
                        var savedTransaction = TransactionManager.FindLatestActiveByAccount(accountId);
                        string fullName = $"{account.FirstName} {account.LastName}";
                        string referenceNumber = $"REF-{DateTime.Now:yyyyMMdd}-{savedTransaction?.TransactionID ?? 0:D6}";

                        EmailHelper.SendTransactionReceipt(
                            toEmail: account.Email!,
                            userName: fullName,
                            membershipPlan: newtransaction.MembershipPlan.ToString(),
                            amount: amount,
                            transactionDate: transaction.TransactionDate,
                            endDate: regularEndDate,
                            transactionId: (savedTransaction?.TransactionID ?? 0).ToString(),
                            referenceNumber: referenceNumber
                        );

                        Console.WriteLine($"✅ Receipt sent to {account.Email}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Email failed: {ex.Message}");
                    }
                }
            }

            return RedirectToAction("UserMembership");
        }



        [Authorize]
        [HttpPost]
        public IActionResult CancelMembership(int transactionId)
        {
            int accountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var transaction = TransactionManager.FindById(transactionId);

            if (transaction == null || transaction.AccountID != accountId || transaction.Status != TransactionStatus.Active)
            {
                return BadRequest("Cannot cancel this transaction.");
            }

            transaction.Status = TransactionStatus.Cancelled;
            TransactionManager.UpdateTransaction(transaction);



            var account = AccountManager.FindById(accountId);
            if (account != null)
            {
                account.MembershipPlan = MembershipPlan.None;
                AccountManager.UpdateAccount(account);
            }

            return RedirectToAction("UserMembership");
        }


        // ===== PAGES =====

        [Authorize]
        public IActionResult UserMembership()
        {
            if (!User.Identity!.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            int accountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var account = AccountManager.FindById(accountId);

            if (account == null)
                return RedirectToAction("Login", "Account");

            var transaction = TransactionManager.FindLatestActiveByAccount(accountId);

            var model = new MembershipPassViewModel
            {
                FullName = $"{account.FirstName} {account.LastName}",
                MemberID = account.MemberID,
            };


            if (transaction != null)
            {

                bool isActive = transaction.Status == TransactionStatus.Active && transaction.EndDate > DateTime.Now;

                model.HasActiveMembership = transaction.Status == TransactionStatus.Active && transaction.EndDate > DateTime.Now;
                model.MembershipPlan = transaction.MembershipPlan.ToString();

                model.canUpgrade = transaction.MembershipPlan < MembershipPlan.FitElite && transaction.MembershipPlan > MembershipPlan.None;

                model.CurrentPlanLabel = isActive ? "CURRENT PLAN" : "EXPIRED PLAN";

                model.NextPlanLabel = model.canUpgrade ? "UPGRADE NOW" : "SUBSCRIBE NOW";


            }
            else
            {
                model.MembershipPlan = "None";
                model.CurrentPlanLabel = "NO ACTIVE PLAN";
                model.NextPlanLabel = "SUBSCRIBE NOW";
            }

            return View(model);
        }



        [Authorize(Roles = "user")]
        public IActionResult TransactionHistoryUser()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            int userId = int.Parse(userIdClaim);

            var transactions = TransactionManager.GetTransactionsByUser(userId);


            var model = new TransactionHistoryViewModel
            {
                Transactions = transactions.Select(t => new UserTransactionViewModel
                {
                    OrderNumber = $"#ORD-{t.TransactionID:D3}",
                    TransactionDate = t.TransactionDate,
                    Plan = t.MembershipPlan.ToString(),
                    Amount = t.Amount
                }).ToList()
            };

            return View(model);
        }


        [HttpPost]
        [Authorize]
        public IActionResult RenewMembership()
        {
            int accountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var account = AccountManager.FindById(accountId);


            if (account == null)
                return RedirectToAction("Login", "Account");

            var lastTransaction = TransactionManager.FindLatestByAccount(accountId);

            if (lastTransaction == null)
            {
                TempData["Error"] = "No previous membership found. Please choose a plan.";
                return RedirectToAction("UserMembership", "Transaction");
            }

            if (lastTransaction.Status == TransactionStatus.Active)
            {
                TempData["Notice"] = "Your membership is still active. You can extend it instead.";
                return RedirectToAction("UserMembership", "Transaction");
            }

            if (lastTransaction.Status == TransactionStatus.Expired)
            {

                TempData["IsRenewal"] = true;
                TempData["RenewPlan"] = lastTransaction.MembershipPlan;

                return RedirectToAction("PaymentMethod", "Transaction");
            }

            TempData["Error"] = "Unable to process renewal. Please try again later.";
            return RedirectToAction("UserMembership", "Transaction");

        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult UpgradeMembership(string plan)
        {
            Console.WriteLine($">>>>>>> UpgradeMembership called with plan: {plan}");

            if (!System.Enum.TryParse<MembershipPlan>(plan, out var newPlan))
            {
                TempData["Error"] = "Invalid membership plan selected.";
                return RedirectToAction("UserMembership");
            }

            int accountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var account = AccountManager.FindById(accountId);
            if (account == null)
                return RedirectToAction("Login", "Account");

            var lastTransaction = TransactionManager.FindLatestActiveByAccount(accountId);

            if (lastTransaction != null)
            {
                
            }

            if (lastTransaction == null || lastTransaction.Status != TransactionStatus.Active)
            {
                TempData["Error"] = "No active membership found. Please subscribe to a new plan.";
                return RedirectToAction("PaymentMethod", new { plan = plan });
            }

            if (newPlan <= lastTransaction.MembershipPlan)
            {
                TempData["Error"] = "You can only upgrade to a higher plan.";
                return RedirectToAction("UserMembership");
            }

            decimal currentAmount = lastTransaction.Amount;
            decimal newAmount = newPlan switch
            {
                MembershipPlan.FitStart => 999m,
                MembershipPlan.FitPro => 1499m,
                MembershipPlan.FitElite => 2499m,
                _ => 0m
            };

            

            decimal upgradeCost = Helpers.Helpers.UnusedMembershipCalculator(
                currentAmount: currentAmount,
                newAmount: newAmount,
                startDate: lastTransaction.StartDate
            );

           

            TempData["IsUpgrade"] = true;
            TempData["UpgradeAmount"] = upgradeCost.ToString("F2");
            TempData["UpgradePlan"] = plan;
            TempData["CurrentPlan"] = lastTransaction.MembershipPlan.ToString();

            Console.WriteLine($">>>>>>> TempData set - UpgradeAmount: {TempData["UpgradeAmount"]}");

            return RedirectToAction("PaymentMethod", new { plan = plan });
        }



    }
}
