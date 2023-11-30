using Microsoft.Extensions.Options;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using TgHelpDesk.Models.Service;
using TgHelpDesk.Services.TgUsers;
using Telegram.Bot.Types.Enums;

namespace TgHelpDesk.Services.Bot
{
    public class BotMethods
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<BotMethods> _logger;
        private readonly BotConfiguration _botConfiguration;

        public BotMethods(ITelegramBotClient botClient, IOptions<BotConfiguration> botOptions, ILogger<BotMethods> logger)
        {
            _botClient = botClient;
            _logger = logger;
            _botConfiguration = botOptions.Value;
        }

        public async Task SendMessage(string text, long ChatId)
        {
            await _botClient.SendTextMessageAsync(
                chatId: ChatId,
                text: text,
                parseMode: ParseMode.Html);
        }
    }
}
