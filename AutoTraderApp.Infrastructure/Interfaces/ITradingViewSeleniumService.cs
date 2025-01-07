using Microsoft.Playwright;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTraderApp.Infrastructure.Interfaces
{
    public interface ITradingViewSeleniumService
    {
        public bool CreateAlertSync(string strategyName, string webhookUrl, string action, string symbol, int quantity, Guid brokerAccountId, Guid userId);
        public bool Login(Guid userId, string password);
    }
}
