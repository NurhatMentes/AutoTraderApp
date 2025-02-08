using AutoTraderApp.Infrastructure.Interfaces;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AutoTraderApp.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OkxTrController : BaseController
    {
        private readonly IOkxService _okxService;

        public OkxTrController(IOkxService okxService)
        {
            _okxService = okxService;
        }

        /// <summary>
        /// Kullanıcının OKX (TR) Hesap Bakiyesini Getirir
        /// </summary>
        [HttpGet("balance/{brokerAccountId}")]
        public async Task<IActionResult> GetAccountBalance(Guid brokerAccountId, string currency="TRY")
        {
            try
            {
                decimal balance = await _okxService.GetAccountBalanceAsync(brokerAccountId, currency);
                return Ok(new SuccessDataResult<decimal>(balance, "OKX bakiyesi getirildi."));
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorResult($"Hata: {ex.Message}"));
            }
        }

        /// <summary>
        /// Kullanıcının OKX (TR) Açık Emirlerini Getirir
        /// </summary>
        [HttpGet("orders/{brokerAccountId}/{symbol}")]
        public async Task<IActionResult> GetActiveOrders(Guid brokerAccountId, string symbol)
        {
            try
            {
                var orders = await _okxService.GetOpenOrdersAsync(brokerAccountId, symbol);
                return Ok(new SuccessDataResult<object>(orders, "OKX açık emirleri getirildi."));
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorResult($"Hata: {ex.Message}"));
            }
        }

        /// <summary>
        /// Kullanıcının OKX (TR) Piyasa Fiyatını Getirir
        /// </summary>
        [HttpGet("market-price/{brokerAccountId}/{symbol}")]
        public async Task<IActionResult> GetMarketPrice(Guid brokerAccountId, string symbol)
        {
            try
            {
                decimal price = await _okxService.GetMarketPriceAsync(symbol, brokerAccountId);
                return Ok(new SuccessDataResult<decimal>(price, "OKX piyasa fiyatı getirildi."));
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorResult($"Hata: {ex.Message}"));
            }
        }

        /// <summary>
        /// Kullanıcının OKX (TR) Hesap Bilgilerini Getirir
        /// </summary>
        [HttpGet("account-info/{brokerAccountId}")]
        public async Task<IActionResult> GetAccountInfo(Guid brokerAccountId)
        {
            try
            {
                var balance = await _okxService.GetAccountInfoAsync(brokerAccountId);
                var openOrders = await _okxService.GetActiveOrdersAsync(brokerAccountId);

                var accountInfo = new
                {
                    Balance = balance,
                    OpenOrders = openOrders
                };

                return Ok(new SuccessDataResult<object>(accountInfo, "OKX hesap bilgileri getirildi."));
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorResult($"Hata: {ex.Message}"));
            }
        }
    }
}
