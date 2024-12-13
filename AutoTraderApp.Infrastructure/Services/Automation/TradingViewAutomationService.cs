using AutoTraderApp.Core.CrossCuttingConcerns.Caching;
using AutoTraderApp.Core.Security.Hashing;
using AutoTraderApp.Core.Utilities.Repositories;
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

        private static IBrowserContext _context;
        private static IPage _page;

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

                _context ??= await _browser.NewContextAsync();
                _page ??= await _context.NewPageAsync();

                await _page.GotoAsync("https://www.tradingview.com");
                await _page.ClickAsync("button.tv-header__user-menu-button--anonymous");
                await _page.ClickAsync("button[data-name='header-user-menu-sign-in']");
                await _page.ClickAsync("button.emailButton-nKAw8Hvt");
                await _page.FillAsync("input[name='id_username']", tradingAccount.Email);
                await _page.FillAsync("input[name='id_password']", password);
                await _page.ClickAsync("button.submitButton-LQwxK8Bm");
                await Task.Delay(TimeSpan.FromSeconds(30));

                if (await _page.IsVisibleAsync("div#rc-anchor-container"))
                {
                    Console.WriteLine("Captcha detected. Please solve it manually.");
                    await Task.Delay(TimeSpan.FromMinutes(3));
                    if (await _page.IsVisibleAsync("div#rc-anchor-container"))
                    {
                        Console.WriteLine("Captcha not solved within the time limit.");
                        return false;
                    }
                }

                if (await _page.IsVisibleAsync("input[name='id_code']"))
                {
                    Console.WriteLine("2FA required. Waiting for user to complete it.");
                    await Task.Delay(TimeSpan.FromMinutes(2));

                    if (await _page.IsVisibleAsync("button.tv-header__user-menu-button--logged"))
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

                if (await _page.IsVisibleAsync("button.tv-header__user-menu-button--logged"))
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
                if (!_cacheManager.Get<bool>($"TradingViewSession_{userId}"))
                {
                    Console.WriteLine("Kullanıcı oturumu açık değil. Strateji oluşturma iptal ediliyor.");
                    return false;
                }

                Console.WriteLine($"Sembol için grafik sayfasına yönlendirme: {symbol}");
                await _page.GotoAsync($"https://www.tradingview.com/chart/?symbol=NASDAQ%3A{symbol}");

                await Task.Delay(TimeSpan.FromSeconds(15));

               
                // Gelişmiş script temizleme ve yazma yaklaşımı
                await _page.EvaluateAsync(@"() => {
            const textArea = document.querySelector('textarea.inputarea');
            if (textArea) {
                // Textarea'nın tüm içeriğini silmek için 
                textArea.focus();
                
                // Ctrl+A ile tüm içeriği seçme
                const selectAllEvent = new KeyboardEvent('keydown', {
                    bubbles: true,
                    cancelable: true,
                    keyCode: 65,
                    which: 65,
                    key: 'a',
                    code: 'KeyA',
                    ctrlKey: true
                });
                textArea.dispatchEvent(selectAllEvent);

                // Delete tuşu ile içeriği silme
                const deleteEvent = new KeyboardEvent('keydown', {
                    bubbles: true,
                    cancelable: true,
                    keyCode: 46,
                    which: 46,
                    key: 'Delete',
                    code: 'Delete'
                });
                textArea.dispatchEvent(deleteEvent);
            }
        }");

                await Task.Delay(TimeSpan.FromSeconds(2));

                // Yeni scripti yazma
                await _page.EvaluateAsync(@"(script) => {
            const textArea = document.querySelector('textarea.inputarea');
            if (textArea) {
                // Native input value setter kullanarak içeriği yazma
                const nativeInputValueSetter = Object.getOwnPropertyDescriptor(window.HTMLTextAreaElement.prototype, 'value').set;
                nativeInputValueSetter.call(textArea, script);
                
                // Input eventi tetikleme
                const event = new Event('input', { bubbles: true });
                textArea.dispatchEvent(event);
            }
        }", script);

                await Task.Delay(TimeSpan.FromSeconds(3));

                // Save Script butonuna tıklama
                Console.WriteLine("Script kaydediliyor...");
                var saveButton = await _page.QuerySelectorAsync("div[data-tooltip='Save script']");
                if (saveButton != null)
                {
                    await saveButton.ClickAsync();
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
                else
                {
                    Console.WriteLine("Kaydetme butonu bulunamadı.");
                    return false;
                }

                // "Add to Chart" düğmesine tıklama
                Console.WriteLine("Strateji charts'a ekleniyor...");
                var addToChartButton = await _page.QuerySelectorAsync("button:has-text('Add to Chart')");
                if (addToChartButton != null)
                {
                    await addToChartButton.ClickAsync();
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
                else
                {
                    Console.WriteLine("Charts'a Ekle butonu bulunamadı.");
                    return false;
                }

                Console.WriteLine("Strateji başarıyla oluşturuldu.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Strateji oluşturma sırasında hata: {ex.Message}");
                Console.WriteLine($"Detaylı Hata İzi: {ex.StackTrace}");
                return false;
            }
        }

        public void Dispose()
        {
            _page?.CloseAsync();
            _context?.CloseAsync();
            _browser?.CloseAsync();
        }
    }
}
