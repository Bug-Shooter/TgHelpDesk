using Microsoft.Extensions.Options;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
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

        public async Task<Message> SendMessageAsync(string text, long ChatId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _botClient.SendTextMessageAsync(
                chatId: ChatId,
                text: text,
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
        }

        public async Task<Message> SendReplyMsg(string text, long ChatId, int ReplyMsgId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _botClient.SendTextMessageAsync(
                chatId: ChatId,
                text: text,
                replyToMessageId: ReplyMsgId,
                cancellationToken: cancellationToken);
        }
         
        public async Task SendLogMsg(string text)
        {
            await _botClient.SendTextMessageAsync(
                chatId: _botConfiguration.LoggerChatId,
                text: text,
                messageThreadId: _botConfiguration.LoggerReplyMsg,
                parseMode: ParseMode.Html);
        }
        public async Task SetMenuButton(long chatId, CancellationToken cancellationToken = default(CancellationToken))
        {
            await _botClient.SetChatMenuButtonAsync
                 (chatId: chatId,
                   menuButton: new MenuButtonWebApp
                   {
                       Text = "Новый Запрос",
                       WebApp = new WebAppInfo()
                       {
                           Url = _botConfiguration.HostAddress,
                       }
                   },
                   cancellationToken: cancellationToken);
        }
    }
}
