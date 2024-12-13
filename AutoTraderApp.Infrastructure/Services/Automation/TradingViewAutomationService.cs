using AutoTraderApp.Core.CrossCuttingConcerns.Caching;
using AutoTraderApp.Core.Security.Hashing;
using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Infrastructure.Interfaces;
using Microsoft.Playwright;

namespace AutoTraderApp.Infrastructure.Services.Automation
{
    public class TradingViewAutomationService : ITradingViewAutomationService
    {
        private readonly IBrowser _browser;
        private readonly IBaseRepository<UserTradingAccount> _tradingAccountRepository;
        private readonly ICacheManager _cacheManager;

        public TradingViewAutomationService(IBaseRepository<UserTradingAccount> tradingAccountRepository, ICacheManager cacheManager)
        {
            var playwright = Playwright.CreateAsync().Result;
            _browser = playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false
            }).Result;

            _tradingAccountRepository = tradingAccountRepository;
            _cacheManager = cacheManager;
        }

        public async Task<bool> LoginAsync(Guid userId, string password)
        {
            try
            {
                var tradingAccount = await _tradingAccountRepository.GetAsync(x => x.UserId == userId);
                if (tradingAccount == null)
                {
                    Console.WriteLine("No TradingView account found for the user.");
                    return false;
                }

                var passwordHash = Convert.FromBase64String(tradingAccount.EncryptedPassword);
                var passwordSalt = Convert.FromBase64String(tradingAccount.PasswordSalt);

                if (!HashingHelper.VerifyPasswordHash(password, passwordHash, passwordSalt))
                {
                    Console.WriteLine("Invalid TradingView password.");
                    return false;
                }

                var context = await _browser.NewContextAsync();
                var page = await context.NewPageAsync();

                await page.GotoAsync("https://www.tradingview.com");
                await page.ClickAsync("button.tv-header__user-menu-button--anonymous");
                await page.ClickAsync("button[data-name='header-user-menu-sign-in']");
                await page.ClickAsync("button.emailButton-nKAw8Hvt");
                await page.FillAsync("input[name='id_username']", tradingAccount.Email);
                await page.FillAsync("input[name='id_password']", password);
                await page.ClickAsync("button.submitButton-LQwxK8Bm");
                await Task.Delay(TimeSpan.FromSeconds(30));

                // Captcha kontrolü
                if (await page.IsVisibleAsync("div#rc-anchor-container"))
                {
                    Console.WriteLine("Captcha detected. Please solve it manually.");
                    await Task.Delay(TimeSpan.FromMinutes(3)); 

                    if (await page.IsVisibleAsync("div#rc-anchor-container"))
                    {
                        Console.WriteLine("Captcha not solved within the time limit.");
                        return false;
                    }
                }

                // 2FA kontrolü
                if (await page.IsVisibleAsync("input[name='id_code']"))
                {
                    Console.WriteLine("2FA required. Waiting for user to complete it.");
                    await Task.Delay(TimeSpan.FromMinutes(5));

                    if (await page.IsVisibleAsync("button.tv-header__user-menu-button--logged"))
                    {
                        Console.WriteLine("2FA completed successfully.");
                        _cacheManager.Add($"TradingViewSession_{userId}", true, 30);
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("2FA not completed within the time limit.");
                        return false;
                    }
                }

                // Giriş kontrolü
                if (await page.IsVisibleAsync("button.tv-header__user-menu-button--logged"))
                {
                    Console.WriteLine("Login successful.");
                    _cacheManager.Add($"TradingViewSession_{userId}", true, 30); 
                    return true;
                }

                Console.WriteLine("Login failed.");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during login: {ex.Message}");
                return false;
            }
        }






        public async Task<bool> CreateStrategyAsync(string strategyName, string symbol, string script, string webhookUrl, Guid userId)
        {
            try
            {
                var context = await _browser.NewContextAsync();
                var page = await context.NewPageAsync();

                var isLoggedIn = _cacheManager.Get<bool>($"TradingViewSession_{userId}");
                if (!isLoggedIn)
                {
                    new ErrorResult("Kullanıcı giriş yapmamış. Lütfen önce giriş yapın.");
                }


                await page.GotoAsync($"https://www.tradingview.com/chart/?symbol=NASDAQ%3A{symbol}");


                // Open the Pine Editor tab
                await page.WaitForSelectorAsync("button[aria-label='Open Pine Editor']", new PageWaitForSelectorOptions
                {
                    Timeout = 10000
                });
                await page.ClickAsync("button[aria-label='Open Pine Editor']");

                // Access textarea in Pine Editor
                var editor = await page.WaitForSelectorAsync("textarea.inputarea[data-mprt='7']", new PageWaitForSelectorOptions
                {
                    Timeout = 15000
                });

                if (editor == null)
                {
                    Console.WriteLine("Editor textarea not found.");
                    return false;
                }

                // Generate and paste the Pine Script code
                await editor.FillAsync(string.Empty); 
                await editor.FillAsync(script);

                // Find and click the Save button
                await page.WaitForSelectorAsync("div[data-tooltip='Save script']", new PageWaitForSelectorOptions
                {
                    Timeout = 10000
                });
                await page.ClickAsync("div[data-tooltip='Save script']");

                // Run strategy
                await page.WaitForSelectorAsync("button:has-text('Add to Chart')", new PageWaitForSelectorOptions
                {
                    Timeout = 10000
                });
                await page.ClickAsync("button:has-text('Add to Chart')");

                // Create alarm and add webhook URL
                await page.WaitForSelectorAsync("button[data-name='alerts']");
                await page.ClickAsync("button[data-name='alerts']");
                await page.ClickAsync("button:has-text('Create Alert')");
                await page.WaitForSelectorAsync("input[placeholder='Webhook URL']");
                await page.FillAsync("input[placeholder='Webhook URL']", webhookUrl);
                await page.ClickAsync("button:has-text('Create')");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating strategy: {ex.Message}");
                return false;
            }
        }

    }
}

