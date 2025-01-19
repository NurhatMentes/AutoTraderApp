using AutoTraderApp.Core.CrossCuttingConcerns.Caching;
using AutoTraderApp.Core.Security.Hashing;
using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Infrastructure.Interfaces;
using Microsoft.Playwright;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace AutoTraderApp.Infrastructure.Services.Automation.Playwrights
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
                await Task.Delay(TimeSpan.FromSeconds(1));
                await _page.FillAsync("input[name='id_password']", password);
                await Task.Delay(TimeSpan.FromSeconds(2));
                await _page.ClickAsync("button.submitButton-LQwxK8Bm");
                await Task.Delay(TimeSpan.FromMinutes(3));

                if (await _page.IsVisibleAsync("div#rc-anchor-container"))
                {
                    Console.WriteLine("Captcha detected. Please solve it manually.");
                    await Task.Delay(TimeSpan.FromMinutes(2));
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
                        _cacheManager.Add($"TradingViewSession_{userId}", true, 60);
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
            int randomTime = new Random().Next(4, 10);
            try
            {
                if (!_cacheManager.Get<bool>($"TradingViewSession_{userId}"))
                {
                    Console.WriteLine("Kullanıcı oturumu açık değil. Strateji oluşturma iptal ediliyor.");
                    return false;
                }

                Console.WriteLine($"Sembol için grafik sayfasına yönlendirme: {symbol}");
                await _page.GotoAsync($"https://www.tradingview.com/chart/?symbol=NASDAQ%3A{symbol}");

                await Task.Delay(TimeSpan.FromSeconds(randomTime));

                // Pine Editor kontrolü ve açma
                var pineEditorCheck = await _page.QuerySelectorAsync("button[aria-label='Close Pine Editor']");
                if (pineEditorCheck == null)
                {
                    // Pine Editor açma
                    var pineEditorButton = await _page.QuerySelectorAsync("button[aria-label='Open Pine Editor'], button[data-name='scripteditor'], button[data-tooltip='Open Pine Editor']");

                    if (pineEditorButton != null)
                    {
                        Console.WriteLine("Pine Editor butonu bulundu, tıklanıyor...");
                        await pineEditorButton.ClickAsync();
                        await Task.Delay(TimeSpan.FromSeconds(randomTime));
                    }
                    else
                    {
                        Console.WriteLine("Pine Editor bulunamadı.");
                        return false;
                    }
                }

                // Script temizleme ve yazma işlemi
                await _page.EvaluateAsync(@"() => {
            const textArea = document.querySelector('textarea.inputarea');
            if (textArea) {
                // Textarea'nın tüm içeriğini silmek için 
                textArea.focus();
                
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

                // Virgülleri yalnızca sayısal değerlerde noktaya çevir
                script = ConvertCommasInNumericValues(script);

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

                    // Save Script Save butonuna tıklama
                    var saveScriptButton = await _page.QuerySelectorAsync("button[name='save']");
                    if (saveScriptButton != null)
                    {
                        Console.WriteLine("Save Script butonu bulundu, tıklanıyor...");
                        await saveScriptButton.ClickAsync();
                        await Task.Delay(TimeSpan.FromSeconds(3));

                        // "Add to Chart" düğmesine tıklama
                        Console.WriteLine("Strateji charts'a ekleniyor...");
                        var addToChartButton = await _page.QuerySelectorAsync("div[data-name='add-script-to-chart']");
                        if (addToChartButton != null)
                        {
                            Console.WriteLine("Add to Chart butonu bulundu, tıklanıyor...");

                            // "Confirmation" uyarısı varsa onayla
                            var confirmationButton = await _page.QuerySelectorAsync("button[name='yes']");
                            if (confirmationButton != null)
                            {
                                await confirmationButton.ClickAsync();
                                await Task.Delay(TimeSpan.FromSeconds(2));
                            }

                            await addToChartButton.ClickAsync();
                            await Task.Delay(TimeSpan.FromSeconds(3));

                            Console.WriteLine("Strateji başarıyla oluşturuldu.");
                            return true;
                        }
                        else
                        {
                            Console.WriteLine("Add to Chart butonu bulunamadı. Başka Sembole geçiliyor");
                            return false;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Save Script butonu bulunamadı. İşlem 'Add to Chart' ile devam edecek.");

                        // "Add to Chart" düğmesine tıklama
                        Console.WriteLine("Strateji charts'a ekleniyor...");
                        var addToChartButton = await _page.QuerySelectorAsync("div[data-name='add-script-to-chart']");
                        if (addToChartButton != null)
                        {
                            Console.WriteLine("Add to Chart butonu bulundu, tıklanıyor...");
                            await addToChartButton.ClickAsync();
                            await Task.Delay(TimeSpan.FromSeconds(randomTime));
                        }
                        else
                        {
                            Console.WriteLine("Add to Chart butonu bulunamadı.");
                            return false;
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("----------------------------------------");
                Console.WriteLine("----------------------------------------");
                Console.WriteLine($"Strateji oluşturma sırasında hata: {ex.Message}");
                Console.WriteLine("----------------------------------------");
                Console.WriteLine("----------------------------------------");
                return false;
            }
        }


        public async Task<bool> CreateAlertAsync(string strategyName, string webhookUrl, string action, string symbol, int quantity, Guid brokerAccountId, Guid userId)
        {
            Console.WriteLine($"Sembol için grafik sayfasına yönlendirme: {symbol}");
            await _page.GotoAsync($"https://www.tradingview.com/chart/?symbol=NASDAQ%3A{symbol}");
            const int maxRetries = 2;
            int retryCount = 0;

            try
            {
                int randomTime = new Random().Next(2, 6);
                await Task.Delay(TimeSpan.FromSeconds(randomTime));
                Console.WriteLine($"{symbol} sembolü için alert butonu aranıyor...");

                // Add Alert butonuna tıkla
                var allButtons = await _page.QuerySelectorAllAsync("button");
                foreach (var button in allButtons)
                {
                    var pathD = await button.EvaluateAsync<string>("button => button.querySelector('svg path')?.getAttribute('d')");
                    if (pathD == "M4.12 0 .5 3.72l.72.7L4.84.7 4.12 0ZM13.88 0l3.62 3.72-.72.7L13.16.7l.72-.7ZM13.99 5.67A6 6 0 0 1 15 9v.4h1V9a7 7 0 1 0-7 7h.4v-1H9a6 6 0 1 1 4.99-9.33Z")
                    {
                        await button.ClickAsync();
                        Console.WriteLine("Add Alert butonuna başarıyla tıklandı.");
                        break;
                    }
                }

                    await Task.Delay(TimeSpan.FromSeconds(4));

                    //// **Condition Alanını Güncelleme**
                    //Console.WriteLine("Condition alanı ayarlanıyor...");
                    //var conditionDropdown = await _page.QuerySelectorAsync("span[data-name='main-series-select']");

                    //if (conditionDropdown == null)
                    //    throw new Exception("Condition listbox bulunamadı.");
                    //await conditionDropdown.ClickAsync();
                    //await Task.Delay(TimeSpan.FromSeconds(1));

                    //// **Strateji adıyla eşleşme sağlama**
                    //string sanitizedStrategyName = strategyName.Split('/').Last(); // Sadece `{strategy.StrategyName}` kısmını al
                    //sanitizedStrategyName = sanitizedStrategyName.Replace("(", @"\(").Replace(")", @"\)"); // Özel karakterleri escape et
                    //Console.WriteLine($"Eşleşme için aranan strateji adı: {sanitizedStrategyName}");

                    //// Tüm seçenekleri listele ve eşleşmeyi kontrol et
                    //var strategyOptions = await _page.QuerySelectorAllAsync("div[data-name]");
                    //foreach (var option in strategyOptions)
                    //{
                    //    var optionName = await option.EvaluateAsync<string>("node => node.getAttribute('data-name')");
                    //    if (optionName != null && optionName.Contains(sanitizedStrategyName))
                    //    {
                    //        Console.WriteLine($"Strateji bulundu: {optionName}, seçim yapılıyor...");
                    //        await option.ClickAsync();
                    //        await Task.Delay(TimeSpan.FromSeconds(1));
                    //        break;
                    //    }
                    //}

                    //// Eğer hiçbir seçenek bulunamadıysa hata döndür
                    //if (strategyOptions == null || strategyOptions.Count == 0)
                    //    throw new Exception($"Strateji '{sanitizedStrategyName}' listede bulunamadı.");


                    //// **Trigger Alanını Güncelleme**
                    //Console.WriteLine("Trigger alanı ayarlanıyor...");
                    //var everyTimeButton = await _page.QuerySelectorAsync("button[data-name='every-time']");
                    //if (everyTimeButton == null)
                    //    throw new Exception("Every Time düğmesi bulunamadı.");
                    //await everyTimeButton.ClickAsync();

                // Alert adı ve diğer alanları doldurma

                var alertNameInput = await _page.QuerySelectorAsync("#alert-name");
                if (alertNameInput == null)
                    throw new Exception("Alert adı input'u bulunamadı.");
                await alertNameInput.FillAsync(strategyName);

                await Task.Delay(TimeSpan.FromSeconds(2));

                // Expiration Ayarla (8 Saat Sonraya)
                Console.WriteLine("Expiration tarihi ayarlanıyor...");
                var expirationButton = await _page.QuerySelectorAsync("button[aria-controls='alert-editor-expiration-popup']");
                if (expirationButton != null)
                {
                    await expirationButton.ClickAsync();
                    var expirationDate = DateTime.UtcNow.AddHours(8).ToString("MMMM dd, yyyy 'at' HH:mm", CultureInfo.InvariantCulture);
                    await _page.EvaluateAsync($"() => document.querySelector('button[aria-controls=\"alert-editor-expiration-popup\"] .content-H6_2ZGVv').innerText = '{expirationDate}'");
                }

                await Task.Delay(TimeSpan.FromSeconds(4));

                // Mesaj alanını doldur
                var messageBox = await _page.QuerySelectorAsync("#alert-message");
                if (messageBox == null)
                    throw new Exception("Message kutusu bulunamadı.");
                string message = $@"
{{
  ""action"": ""{action}"",
  ""symbol"": ""{symbol}"",
  ""quantity"": {quantity},
  ""brokerAccountId"": ""{brokerAccountId}"",
  ""userId"": ""{userId}""
}}";
                await messageBox.FillAsync(string.Empty);
                await messageBox.FillAsync(message);

                // Notifications Tab
                await Task.Delay(TimeSpan.FromSeconds(1));
                await _page.ClickAsync("#alert-dialog-tabs__notifications");

                // Webhook URL
                await Task.Delay(TimeSpan.FromSeconds(2));
                var webhookBox = await _page.QuerySelectorAsync("#webhook-url");
                if (webhookBox == null)
                    throw new Exception("Webhook URL alanı bulunamadı.");
                await webhookBox.FillAsync(webhookUrl);

                // Create
                await Task.Delay(TimeSpan.FromSeconds(2));
                var createButton = await _page.QuerySelectorAsync("button[data-name='submit']");
                if (createButton == null)
                    throw new Exception("Create butonu bulunamadı.");
                await createButton.ClickAsync();

                Console.WriteLine($"Alert başarıyla oluşturuldu: {symbol} - {action}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Alert oluşturma sırasında hata: {ex.Message}");
                return false;
            }
        }

        public static string ConvertCommasInNumericValues(string script)
        {
            // Regex pattern to match floating point numbers with commas
            var pattern = @"(\d+,\d+)";

            return Regex.Replace(script, pattern, match =>
            {
                // Replace comma with period only in numeric matches
                return match.Value.Replace(",", ".");
            });
        }

        public async Task<bool> DeleteAllAlertsAsync()
        {
            try
            {
                if (_page == null)
                    throw new Exception("TradingView sayfası yüklenemedi.");

                Console.WriteLine("TradingView'deki tüm alarmlar siliniyor...");

                // Alerts settings butonuna tıkla
                var alertsButton = await _page.QuerySelectorAsync("div[data-name='alerts-settings-button']");
                if (alertsButton == null)
                    throw new Exception("Alerts settings butonu bulunamadı.");
                await alertsButton.ClickAsync();
                await Task.Delay(TimeSpan.FromSeconds(3));

                // Delete all inactive alarmları bul ve tıkla
                var deleteInactiveButton = await _page.QuerySelectorAsync("span.label-xZRtm41u:has-text('Delete all inactive')");
                if (deleteInactiveButton == null)
                    throw new Exception("Delete all inactive butonu bulunamadı.");
                await deleteInactiveButton.ClickAsync();
                await Task.Delay(TimeSpan.FromSeconds(2));

                // Açılan onay kutusunda "Delete" butonuna tıkla
                var confirmDeleteButton = await _page.QuerySelectorAsync("span.content-D4RPB3ZC:has-text('Delete')");
                if (confirmDeleteButton == null)
                    throw new Exception("Delete onay butonu bulunamadı.");
                await confirmDeleteButton.ClickAsync();
                await Task.Delay(TimeSpan.FromSeconds(3));

                Console.WriteLine("Tüm TradingView alarmları başarıyla silindi.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Alarmlar silinirken hata oluştu: {ex.Message}");
                return true;
            }
        }


        private void RetryAction(Action action)
        {
            const int maxRetries = 2;
            int retryCount = 0;

            while (retryCount < maxRetries)
            {
                try
                {
                    action();
                    return;
                }
                catch (Exception ex)
                {
                    retryCount++;
                    Console.WriteLine($"Retrying action. Attempt {retryCount}/{maxRetries}. Error: {ex.Message}");
                    Thread.Sleep(1000);
                    if (retryCount == maxRetries)
                        throw;
                }
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
