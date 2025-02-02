using AutoTraderApp.Infrastructure.Interfaces;
using AutoTraderApp.WebAPI.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace AutoTraderApp.API.Controllers
{
    [Route("api/binance")]
    [ApiController]
    public class BinanceController : BaseController
    {
        private readonly IBinanceService _binanceService;

        public BinanceController(IBinanceService binanceService)
        {
            _binanceService = binanceService;
        }

        [HttpGet("account/{brokerAccountId}")]
        public async Task<IActionResult> GetAccountInfo(Guid brokerAccountId)
        {
            try
            {
                var accountInfo = await _binanceService.GetAccountInfoAsync(brokerAccountId);
                return Ok(new { success = true, data = accountInfo });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMarketPriceAsync(Guid brokerAccountId, string Symbol)
        {
            try
            {
                var accountInfo = await _binanceService.GetMarketPriceAsync(Symbol,brokerAccountId);
                return Ok(new { success = true, data = accountInfo });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("account/total/balance/{brokerAccountId}")]
        public async Task<IActionResult> GetTotalBalance(Guid brokerAccountId)
        {
            try
            {
                var accountInfo = await _binanceService.GetTotalBalanceAsync(brokerAccountId);
                return Ok(new { success = true, data = accountInfo });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("account/binance/user/{brokerAccountId}")]
        public async Task<IActionResult> GetBinanceUID(Guid brokerAccountId)
        {
            try
            {
                var accountInfo = await _binanceService.GetBinanceUIDAsync(brokerAccountId);
                return Ok(new { success = true, data = accountInfo });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("account/spot/balance/{brokerAccountId}")]
        public async Task<IActionResult> GetSpotBalanceAsync(Guid brokerAccountId)
        {
            try
            {
                var accountInfo = await _binanceService.GetSpotBalanceAsync(brokerAccountId);
                return Ok(new { success = true, data = accountInfo });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("account/total/portfolio/{brokerAccountId}")]
        public async Task<IActionResult> GetTotalPortfolioValue(Guid brokerAccountId)
        {
            try
            {
                var accountInfo = await _binanceService.GetTotalPortfolioValueAsync(brokerAccountId);
                return Ok(new { success = true, data = accountInfo });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("orders/symbol/{brokerAccountId}")]
        public async Task<IActionResult> GetAllOrdersForAllSymbols(Guid brokerAccountId, string symbol)
        {
            try
            {
                var orders = await _binanceService.GetAllOrdersBySymbolAsync(brokerAccountId, symbol);
                return Ok(new { success = true, data = orders });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }


        [HttpGet("balance/{brokerAccountId}")]
        public async Task<IActionResult> GetBalance(Guid brokerAccountId)
        {
            var balance = await _binanceService.GetAccountBalanceAsync(brokerAccountId);
            return Ok(new { Balance = balance });
        }

        [HttpPost("place-order")]
        public async Task<IActionResult> PlaceOrder(Guid brokerAccountId, string symbol, decimal quantity, string orderType)
        {
            var result = await _binanceService.PlaceOrderAsync(brokerAccountId, symbol, quantity, orderType);
            return result ? Ok("Order placed successfully!") : BadRequest("Order failed!");
        }
    }
}
