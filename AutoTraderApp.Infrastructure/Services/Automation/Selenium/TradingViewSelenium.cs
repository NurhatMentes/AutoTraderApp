using AutoTraderApp.Core.Security.Hashing;
using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Infrastructure.Interfaces;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Globalization;

public class TradingViewSelenium : ITradingViewSeleniumService
{
    private static IWebDriver _driver; 
    private readonly IBaseRepository<UserTradingAccount> _tradingAccountRepository;

    public TradingViewSelenium(IBaseRepository<UserTradingAccount> tradingAccountRepository)
    {
        if (_driver == null) 
        {
            var options = new ChromeOptions();
            options.AddArgument("--start-maximized");
            _driver = new ChromeDriver(options);
        }

        _tradingAccountRepository = tradingAccountRepository;
    }

    public bool Login(Guid userId, string password)
    {
        try
        {
            _driver.Navigate().GoToUrl("https://www.tradingview.com");

            var tradingAccount = _tradingAccountRepository.GetAsync(x => x.UserId == userId);
            if (tradingAccount == null)
            {
                Console.WriteLine("No TradingView account found for the user.");
                return false;
            }

            var passwordHash = Convert.FromBase64String(tradingAccount.Result.EncryptedPassword);
            var passwordSalt = Convert.FromBase64String(tradingAccount.Result.PasswordSalt);

            if (!HashingHelper.VerifyPasswordHash(password, passwordHash, passwordSalt))
            {
                Console.WriteLine("Invalid TradingView password.");
                return false;
            }

            // Kullanıcı giriş butonuna tıkla
            var loginButton = _driver.FindElement(By.CssSelector("button.tv-header__user-menu-button--anonymous"));
            loginButton.Click();
            Thread.Sleep(1000);

            // Giriş işlemini başlat
            var emailButton = _driver.FindElement(By.CssSelector("button[data-name='header-user-menu-sign-in']"));
            emailButton.Click();
            Thread.Sleep(2000);

            var emailLoginButton = _driver.FindElement(By.CssSelector("button.emailButton-nKAw8Hvt"));
            emailLoginButton.Click();
            Thread.Sleep(1000);

            // Kullanıcı adı ve şifreyi doldur
            var emailInput = _driver.FindElement(By.CssSelector("input[name='id_username']"));
            emailInput.SendKeys(tradingAccount.Result.Email);
            Thread.Sleep(1000);
            var passwordInput = _driver.FindElement(By.CssSelector("input[name='id_password']"));
            passwordInput.SendKeys(password);
            Thread.Sleep(1000);

            // Giriş butonuna tıkla
            var submitButton = _driver.FindElement(By.CssSelector("button.submitButton-LQwxK8Bm"));
            submitButton.Click();

            Thread.Sleep(5000); // Giriş işleminin tamamlanmasını bekle.
            if (_driver.FindElements(By.CssSelector("button.tv-header__user-menu-button--logged")).Count > 0)
            {
                Console.WriteLine("Login successful.");
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

    public bool CreateAlertSync(string strategyName, string webhookUrl, string action, string symbol, int quantity, Guid brokerAccountId, Guid userId)
    {
        const int maxRetries = 2;
        int retryCount = 0;

        while (retryCount < maxRetries)
        {
            try { 


                Thread.Sleep(4000);
                 Console.WriteLine($"Navigating to chart page for symbol: {symbol}");
                _driver.Navigate().GoToUrl($"https://www.tradingview.com/chart/?symbol=NASDAQ%3A{symbol}");
                Thread.Sleep(6000);

                // Add Alert Button
                RetryAction(() =>
                {
                    var addAlertButton = _driver.FindElement(By.XPath("//*[@id='bottom-area']/div[4]/div/div[1]/div[1]/div[1]/div[2]/button[2]"));
                    addAlertButton.Click();
                    Console.WriteLine("Add Alert button clicked.");
                });

                // Alert Name
                //RetryAction(() =>
                //{
                //    var alertNameInput = _driver.FindElement(By.Id("alert-name"));
                //    alertNameInput.Clear();
                //    alertNameInput.SendKeys(strategyName);
                //    Console.WriteLine("Alert name set.");
                //    Thread.Sleep(4000);
                //});

                // Expiration
                RetryAction(() =>
                {
                    var expirationButton = _driver.FindElement(By.XPath("//button[@aria-controls='alert-editor-expiration-popup']"));
                    expirationButton.Click();
                    var expirationSet = _driver.FindElement(By.XPath("/html/body/div[5]/div[2]/div/div[2]/div/span/div[1]/div/div/div/button"));
                    expirationSet.Click();
                    Console.WriteLine("Expiration date set.");
                    Thread.Sleep(4000);
                });

                // Alert Message
                RetryAction(() =>
                {
                    var messageBox = _driver.FindElement(By.Id("alert-message"));
                    messageBox.Clear();
                    messageBox.SendKeys(Keys.Control + "a");
                    messageBox.SendKeys(Keys.Backspace);
                    messageBox.SendKeys($@"
{{
  ""action"": ""{action}"",
  ""symbol"": ""{symbol}"",
  ""quantity"": {quantity},
  ""brokerAccountId"": ""{brokerAccountId}"",
  ""userId"": ""{userId}""
}}");
                    Console.WriteLine("Alert message set.");
                    Thread.Sleep(4000);
                });

                // Notifications Tab
                RetryAction(() =>
                {
                    var notificationsTab = _driver.FindElement(By.XPath("/html/body/div[5]/div[2]/div/div[1]/div[2]/div/div/button[2]"));
                    try
                    {
                        notificationsTab.Click();
                        Console.WriteLine("Notifications tab selected.");
                    }
                    catch (ElementClickInterceptedException ex)
                    {
                        Console.WriteLine($"Element blocked by another element: {ex.Message}");
                        CloseOverlayIfExists();
                    }
                });


                // Webhook URL
                RetryAction(() =>
                {
                    var webhookBox = _driver.FindElement(By.XPath("/html/body/div[5]/div[2]/div/div[1]/form/div[1]/div/div[12]/span/span[1]/input"));
                    webhookBox.Clear();
                    webhookBox.SendKeys(Keys.Control + "a");
                    webhookBox.SendKeys(Keys.Backspace);
                    webhookBox.SendKeys(webhookUrl);
                    Console.WriteLine("Webhook URL set.");
                });

                // Create Button
                RetryAction(() =>
                {
                    Thread.Sleep(1000);
                    var createButton = _driver.FindElement(By.XPath("/html/body/div[5]/div[2]/div/div[1]/form/div[2]/div/div/button[2]"));
                    createButton.Click();
                    Console.WriteLine("Alert created successfully.");
                });

                return true;
            }
            catch (Exception ex)
            {
                retryCount++;
                Console.WriteLine($"Error during alert creation. Retry attempt {retryCount}/{maxRetries}. Error: {ex.Message}");
                if (retryCount == maxRetries)
                {
                    Console.WriteLine("Max retries reached. Alert creation failed.");
                    return false;
                }
            }
        }

        return false;
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

    private void CloseOverlayIfExists()
    {
        var overlays = _driver.FindElements(By.ClassName("wrap-fpDXgGC1")); 
        if (overlays.Any())
        {
            Console.WriteLine("Closing overlay...");
            overlays.First().Click(); 
            Thread.Sleep(1000); 
        }
    }


}
