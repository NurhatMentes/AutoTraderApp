using AutoTraderApp.Domain.ExternalModels.Telegram;
using AutoTraderApp.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AutoTraderApp.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TelegramBotController : ControllerBase
    {
        private readonly ITelegramBotService _telegramBotService;

        public TelegramBotController(ITelegramBotService telegramBotService)
        {
            _telegramBotService = telegramBotService;
        }

        [HttpPost("set-webhook")]
        public async Task<IActionResult> SetWebhook([FromQuery] string botToken, [FromQuery] string webhookUrl)
        {
            if (string.IsNullOrWhiteSpace(botToken) || string.IsNullOrWhiteSpace(webhookUrl))
                return BadRequest("Bot token ve webhook URL'si gerekli.");

            await _telegramBotService.SetWebhookAsync(botToken, webhookUrl);
            return Ok("Webhook başarıyla ayarlandı.");
        }

        [HttpGet("get-webhook-info")]
        public async Task<IActionResult> GetWebhookInfo()
        {
            var result = await _telegramBotService.GetWebhookInfoAsync();
            return Ok(result);
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterTelegramUser([FromQuery] string chatId, [FromQuery] string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(chatId) || string.IsNullOrWhiteSpace(phoneNumber))
                return BadRequest("Chat ID ve telefon numarası gerekli.");

            var success = await _telegramBotService.RegisterUserAsync(chatId, phoneNumber);
            if (success)
                return Ok("Telegram kullanıcı kaydı başarılı.");
            return StatusCode(500, "Kayıt başarısız oldu.");
        }

        [HttpPost("send-message")]
        public async Task<IActionResult> SendMessage([FromQuery] string phoneNumber, [FromBody] string message)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(message))
                return BadRequest("Telefon numarası ve mesaj gerekli.");

            var result = await _telegramBotService.SendMessageAsync(phoneNumber, message);
            if (result)
                return Ok("Mesaj başarıyla gönderildi.");
            return BadRequest("Mesaj gönderilemedi.");
        }
    }
}
