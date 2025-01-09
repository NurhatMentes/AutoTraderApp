using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.ExternalModels.Telegram;
using AutoTraderApp.Infrastructure.Interfaces;
using System.Net.Http.Json;

namespace AutoTraderApp.Infrastructure.Services.Telegram
{
    public class TelegramBotService : ITelegramBotService
    {
        private readonly IBaseRepository<TelegramUser> _telegramUserRepository;
        private readonly IBaseRepository<TelegramBotConfig> _telegramBotConfigRepository;
        private readonly HttpClient _httpClient;

        public TelegramBotService(
            IBaseRepository<TelegramUser> telegramUserRepository,
            IBaseRepository<TelegramBotConfig> telegramBotConfigRepository)
        {
            _telegramUserRepository = telegramUserRepository;
            _telegramBotConfigRepository = telegramBotConfigRepository;
            _httpClient = new HttpClient();
        }

        // Webhook ayarı yapma
        public async Task SetWebhookAsync(string botToken, string webhookUrl)
        {
            var requestUrl = $"https://api.telegram.org/bot{botToken}/setWebhook";
            var content = new MultipartFormDataContent
            {
                { new StringContent(webhookUrl), "url" }
            };

            var response = await _httpClient.PostAsync(requestUrl, content);
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Webhook ayarlanamadı: {response.ReasonPhrase}");
        }

        // Webhook bilgilerini alma
        public async Task<string> GetWebhookInfoAsync()
        {
            var botConfig = await _telegramBotConfigRepository.GetSingleAsync(c => true);
            if (botConfig == null || string.IsNullOrEmpty(botConfig.BotToken))
                throw new Exception("Telegram bot token yapılandırılmadı.");

            var url = $"https://api.telegram.org/bot{botConfig.BotToken}/getWebhookInfo";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        // Bot token güncelleme
        public async Task<bool> UpdateBotTokenAsync(string botToken)
        {
            var config = await _telegramBotConfigRepository.GetSingleAsync(c => true);
            if (config != null)
            {
                config.BotToken = botToken;
                await _telegramBotConfigRepository.UpdateAsync(config);
                await _telegramBotConfigRepository.SaveChangesAsync();
                return true;
            }
            return false;
        }

        // Kullanıcıya mesaj gönderme
        public async Task<bool> SendMessageAsync(string phoneNumber, string message)
        {
            var user = await _telegramUserRepository.GetSingleAsync(u => u.PhoneNumber == phoneNumber);
            if (user == null || string.IsNullOrEmpty(user.ChatId))
                return false;

            var botConfig = await _telegramBotConfigRepository.GetSingleAsync(c => true);
            if (botConfig == null)
                throw new Exception("Telegram bot token yapılandırılmadı.");

            var url = $"https://api.telegram.org/bot{botConfig.BotToken}/sendMessage";
            var payload = new { chat_id = user.ChatId, text = message };
            var response = await _httpClient.PostAsJsonAsync(url, payload);
            return response.IsSuccessStatusCode;
        }

        // Telegram kullanıcı kaydı
        public async Task<bool> RegisterUserAsync(string chatId, string phoneNumber)
        {
            var existingUser = await _telegramUserRepository.GetSingleAsync(u => u.PhoneNumber == phoneNumber);
            if (existingUser != null)
            {
                existingUser.ChatId = chatId;
                await _telegramUserRepository.UpdateAsync(existingUser);
            }
            else
            {
                var newUser = new TelegramUser
                {
                    ChatId = chatId,
                    PhoneNumber = phoneNumber,
                    CreatedAt = DateTime.UtcNow
                };
                await _telegramUserRepository.AddAsync(newUser);
            }

            await _telegramUserRepository.SaveChangesAsync();
            return true;
        }

        public async Task<TelegramUser> GetUserByIdOrPhoneNumberAsync(Guid? userId, string? phoneNumber)
        {
            if (userId == null && string.IsNullOrEmpty(phoneNumber))
                throw new Exception("Kullanıcı kimlği veya kullanıcı telefonu girilmelidir.");
            return await _telegramUserRepository.GetSingleAsync(u => u.UserId == userId);
        }
    }
}
