using AutoTraderApp.Infrastructure.Interfaces;
using Microsoft.Playwright;

namespace AutoTraderApp.Infrastructure.Services.Automation
{
    public class TradingViewAutomationService : ITradingViewAutomationService
    {
        private readonly IBrowser _browser;

        public TradingViewAutomationService()
        {
            var playwright = Playwright.CreateAsync().Result;
            _browser = playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false
            }).Result;
        }

        public async Task<bool> CreateStrategyAsync(string strategyName, string symbol, string script, string webhookUrl)
        {
            try
            {
                var context = await _browser.NewContextAsync();
                var page = await context.NewPageAsync();

                await page.GotoAsync($"https://www.tradingview.com/chart/?symbol=NASDAQ%3A{symbol}");

                await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                await page.WaitForSelectorAsync("button[aria-label='Open Pine Editor']", new PageWaitForSelectorOptions
                {
                    Timeout = 10000
                });
                await page.ClickAsync("button[aria-label='Open Pine Editor']");

                var editor = await page.WaitForSelectorAsync("textarea.inputarea[data-mprt='7']", new PageWaitForSelectorOptions
                {
                    Timeout = 15000 
                });

                if (editor == null)
                {
                    Console.WriteLine("Editor textarea not found.");
                    return false;
                }

                await editor.FillAsync(string.Empty);

                await editor.FillAsync(script);

                await page.WaitForSelectorAsync("div[data-tooltip='Save script']", new PageWaitForSelectorOptions
                {
                    Timeout = 10000
                });
                await page.ClickAsync("div[data-tooltip='Save script']");

                await page.WaitForSelectorAsync("button:has-text('Add to Chart')", new PageWaitForSelectorOptions
                {
                    Timeout = 10000
                });
                await page.ClickAsync("button:has-text('Add to Chart')");

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

