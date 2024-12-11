using AutoTraderApp.Infrastructure.Interfaces;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
using AutoTraderApp.Core.Utilities.Settings;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Microsoft.Extensions.Options;
using AutoTraderApp.Domain.ExternalModels.TradingView;

namespace AutoTraderApp.Infrastructure.Services.TradingView
{
    public class TradingViewService : ITradingViewService
    {
        private readonly HttpClient _httpClient;
        private readonly TradingViewSettings _settings;

        public TradingViewService(HttpClient httpClient, IOptions<TradingViewSettings> options)
        {
            _settings = options.Value ?? throw new ArgumentNullException(nameof(options));
            _httpClient = httpClient;
        }

        public async Task<bool> UploadStrategyAsync(string strategyName, string pineScriptCode)
        {
            var payload = new
            {
                name = strategyName,
                script = pineScriptCode
            };

            var response = await _httpClient.PostAsJsonAsync("tradingview/strategies", payload);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteStrategyAsync(string strategyName)
        {
            var response = await _httpClient.DeleteAsync($"tradingview/strategies/{strategyName}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> SendStrategyAsync(TradingViewStrategy strategy)
        {

            try
            {
                var requestBody = JsonSerializer.Serialize(strategy);
                var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("https://6567-85-103-104-123.ngrok-free.app/api/TradingViewWebhook/webhook", content);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TradingView gönderim hatası: {ex.Message}");
                return false;
            }

        }


        public void UploadStrategyToTradingView(string script, string username, string password)
        {
            var options = new ChromeOptions();
            options.AddArgument("--headless"); 

            using var driver = new ChromeDriver(options);
            driver.Navigate().GoToUrl("https://www.tradingview.com");

            driver.FindElement(By.Name("username")).SendKeys(username);
            driver.FindElement(By.Name("password")).SendKeys(password);
            driver.FindElement(By.Id("loginButton")).Click();

            driver.Navigate().GoToUrl("https://www.tradingview.com/pine-script-editor/");
            var editor = driver.FindElement(By.CssSelector(".pine-script-editor"));
            editor.Clear();
            editor.SendKeys(script);

            driver.FindElement(By.Id("saveButton")).Click();
        }
    }
}

