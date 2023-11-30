using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using Microsoft.Extensions.Options;
using TgHelpDesk.Models.Service;
using TgHelpDesk.Services.TgUsers;

namespace TgHelpDesk.Services.Bot
{
    public class UpdateHandlers
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<UpdateHandlers> _logger;
        private readonly BotConfiguration _botConfiguration;
        private readonly TgUsersService _tgUsersService;

        public UpdateHandlers(ITelegramBotClient botClient, ILogger<UpdateHandlers> logger, IOptions<BotConfiguration> botOptions, TgUsersService tgUsersService)
        {
            _botClient = botClient;
            _logger = logger;
            _botConfiguration = botOptions.Value;
            _tgUsersService = tgUsersService;
        }

        public Task HandleErrorAsync(Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            _logger.LogInformation("HandleError: {ErrorMessage}", ErrorMessage);
            return Task.CompletedTask;
        }

        public async Task HandleUpdateAsync(Update update, CancellationToken cancellationToken)
        {
            var handler = update switch
            {
                // UpdateType.Unknown:
                // UpdateType.ChannelPost:
                // UpdateType.EditedChannelPost:
                // UpdateType.ShippingQuery:
                // UpdateType.PreCheckoutQuery:
                // UpdateType.Poll:
                { Message: { } message } => BotOnMessageReceived(message, cancellationToken),
                //{ EditedMessage: { } message } => BotOnMessageReceived(message, cancellationToken),
                //{ CallbackQuery: { } callbackQuery } => BotOnCallbackQueryReceived(callbackQuery, cancellationToken),
                //{ InlineQuery: { } inlineQuery } => BotOnInlineQueryReceived(inlineQuery, cancellationToken),
                //{ ChosenInlineResult: { } chosenInlineResult } => BotOnChosenInlineResultReceived(chosenInlineResult, cancellationToken),
                _ => UnknownUpdateHandlerAsync(update, cancellationToken)
            };

            await handler;
        }

        private async Task BotOnMessageReceived(Message message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Receive message type: {MessageType}", message.Type);
            if (message.Text is not { } messageText)
                return;

            var action = messageText.Split(' ')[0] switch
            {
                "/start" => SendStartMessage(_botClient, _tgUsersService, message, _botConfiguration, cancellationToken),
                "/help" => Help(_botClient, message, cancellationToken),
                //"/status" => Status(_botClient, message, cancellationToken),
                //"/inline_keyboard" => SendInlineKeyboard(_botClient, message, cancellationToken),
                //"/keyboard" => SendReplyKeyboard(_botClient, message, cancellationToken),
                //"/remove" => RemoveKeyboard(_botClient, message, cancellationToken),
                //"/photo" => SendFile(_botClient, message, cancellationToken),
                //"/request" => RequestContactAndLocation(_botClient, message, cancellationToken),
                //"/inline_mode" => StartInlineQuery(_botClient, message, cancellationToken),
                _ => Unknown(_botClient, message, cancellationToken)
            };
            Message sentMessage = await action;
            _logger.LogInformation("The message was sent with id: {SentMessageId}", sentMessage.MessageId);

            static async Task<Message> SendStartMessage(ITelegramBotClient botClient, TgUsersService _tgUsersService, Message message, BotConfiguration botConfiguration, CancellationToken cancellationToken)
            {
                if(!await _tgUsersService.CheckTgUserExistence(message.From.Id))
                    await _tgUsersService.RegistereNewUser(message.From.Id, message.Chat.Id, message.From.Username ?? "Unknown");

                //TODO: Вариативность в зависимости от статуса авторизации

                string StartMessage = "Привет!\n" +
                                      "Я HelpDesk Бот\n" +
                                      "Воспользуйтесь кнопкой 'создать заявку' для отправки запроса\n" +
                                      "/help - для дополнительной информации";

                await SetMenuButton(botClient, message.Chat.Id, botConfiguration.HostAddress, cancellationToken);

                return await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: StartMessage,
                    replyMarkup: new ReplyKeyboardRemove(),
                    cancellationToken: cancellationToken);
            }

            static async Task SetMenuButton(ITelegramBotClient botClient, long chatId, string Url, CancellationToken cancellationToken)
            {
                await botClient.SetChatMenuButtonAsync
                    (chatId: chatId,
                      menuButton: new MenuButtonWebApp
                      {
                          Text = "Создать запрос",
                          WebApp = new WebAppInfo()
                          {
                              Url = Url,
                          }
                      },
                      cancellationToken: cancellationToken);
            }

            // Send inline keyboard
            // You can process responses in BotOnCallbackQueryReceived handler
            //static async Task<Message> SendInlineKeyboard(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
            //{
            //    await botClient.SendChatActionAsync(
            //        chatId: message.Chat.Id,
            //        chatAction: ChatAction.Typing,
            //        cancellationToken: cancellationToken);

            //    // Simulate longer running task
            //    await Task.Delay(500, cancellationToken);

            //    InlineKeyboardMarkup inlineKeyboard = new(
            //        new[]
            //        {
            //        // first row
            //        new []
            //        {
            //            InlineKeyboardButton.WithCallbackData("1.1", "11"),
            //            InlineKeyboardButton.WithCallbackData("1.2", "12"),
            //        },
            //        // second row
            //        new []
            //        {
            //            InlineKeyboardButton.WithCallbackData("2.1", "21"),
            //            InlineKeyboardButton.WithCallbackData("2.2", "22"),
            //        },
            //        });

            //    return await botClient.SendTextMessageAsync(
            //        chatId: message.Chat.Id,
            //        text: "Choose",
            //        replyMarkup: inlineKeyboard,
            //        cancellationToken: cancellationToken);
            //}

            //static async Task<Message> SendReplyKeyboard(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
            //{
            //    ReplyKeyboardMarkup replyKeyboardMarkup = new(
            //        new[]
            //        {
            //            new KeyboardButton[] { "1.1", "1.2" },
            //            new KeyboardButton[] { "2.1", "2.2" },
            //        })
            //    {
            //        ResizeKeyboard = true
            //    };

            //    return await botClient.SendTextMessageAsync(
            //        chatId: message.Chat.Id,
            //        text: "Choose",
            //        replyMarkup: replyKeyboardMarkup,
            //        cancellationToken: cancellationToken);
            //}

            //static async Task<Message> RemoveKeyboard(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
            //{
            //    return await botClient.SendTextMessageAsync(
            //        chatId: message.Chat.Id,
            //        text: "Removing keyboard",
            //        replyMarkup: new ReplyKeyboardRemove(),
            //        cancellationToken: cancellationToken);
            //}

            //static async Task<Message> SendFile(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
            //{
            //    await botClient.SendChatActionAsync(
            //        message.Chat.Id,
            //        ChatAction.UploadPhoto,
            //        cancellationToken: cancellationToken);

            //    const string filePath = "Files/tux.png";
            //    await using FileStream fileStream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            //    var fileName = filePath.Split(Path.DirectorySeparatorChar).Last();

            //    return await botClient.SendPhotoAsync(
            //        chatId: message.Chat.Id,
            //        photo: new InputFileStream(fileStream, fileName),
            //        caption: "Nice Picture",
            //        cancellationToken: cancellationToken);
            //}

            //static async Task<Message> RequestContactAndLocation(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
            //{
            //    ReplyKeyboardMarkup RequestReplyKeyboard = new(
            //        new[]
            //        {
            //        KeyboardButton.WithRequestLocation("Location"),
            //        KeyboardButton.WithRequestContact("Contact"),
            //        });

            //    return await botClient.SendTextMessageAsync(
            //        chatId: message.Chat.Id,
            //        text: "Who or Where are you?",
            //        replyMarkup: RequestReplyKeyboard,
            //        cancellationToken: cancellationToken);
            //}

            static async Task<Message> Help(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
            {
                string Message = "Я HelpDesk Бот\n" +
                                 "Используя кнопку ниже, вы можете отправить запрос на помощь от IT отдела.\n" +
                                 "Воспользуйтесь тегами, чтобы выбрать вашу проблему, и/или опишите ее самостоятельно. \n" +
                                 "/status - узнать о статусе моих заявок\n" +
                                 "/old - посмотреть мои предыдущие заявки";

                return await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: Message,
                    replyMarkup: new ReplyKeyboardRemove(),
                    cancellationToken: cancellationToken);
            }
            //static async Task<Message> Status(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
            //{
                
            //    string Message = "Заявка:  n" +
            //                     "Используя кнопку ниже, вы можете отправить запрос на помощь от IT отдела.\n" +
            //                     "Воспользуйтесь тегами, чтобы выбрать вашу проблему, и/или опишите ее самостоятельно. \n" +
            //                     "/status - узнать о статусе моих заявок\n" +
            //                     "/old - посмотреть мои предыдущие заявки";

            //    return await botClient.SendTextMessageAsync(
            //        chatId: message.Chat.Id,
            //        text: Message,
            //        replyMarkup: new ReplyKeyboardRemove(),
            //        cancellationToken: cancellationToken);
            //}

            static async Task<Message> Unknown(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
            {
                //const string usage = "Usage:\n" +
                //                     "/inline_keyboard - send inline keyboard\n" +
                //                     "/keyboard    - send custom keyboard\n" +
                //                     "/remove      - remove custom keyboard\n" +
                //                     "/photo       - send a photo\n" +
                //                     "/request     - request location or contact\n" +
                //                     "/inline_mode - send keyboard with Inline Query";

                return await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Неизвестная команда",
                    replyMarkup: new ReplyKeyboardRemove(),
                    cancellationToken: cancellationToken);
            }

            //static async Task<Message> StartInlineQuery(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
            //{
            //    InlineKeyboardMarkup inlineKeyboard = new(
            //        InlineKeyboardButton.WithSwitchInlineQueryCurrentChat("Inline Mode"));

            //    return await botClient.SendTextMessageAsync(
            //        chatId: message.Chat.Id,
            //        text: "Press the button to start Inline Query",
            //        replyMarkup: inlineKeyboard,
            //        cancellationToken: cancellationToken);
            //}
        }

        // Process Inline Keyboard callback data
        //private async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery, CancellationToken cancellationToken)
        //{
        //    _logger.LogInformation("Received inline keyboard callback from: {CallbackQueryId}", callbackQuery.Id);

        //    await _botClient.AnswerCallbackQueryAsync(
        //        callbackQueryId: callbackQuery.Id,
        //        text: $"Received {callbackQuery.Data}",
        //        cancellationToken: cancellationToken);

        //    await _botClient.SendTextMessageAsync(
        //        chatId: callbackQuery.Message!.Chat.Id,
        //        text: $"Received {callbackQuery.Data}",
        //        cancellationToken: cancellationToken);
        //}

        #region Inline Mode

        //private async Task BotOnInlineQueryReceived(InlineQuery inlineQuery, CancellationToken cancellationToken)
        //{
        //    _logger.LogInformation("Received inline query from: {InlineQueryFromId}", inlineQuery.From.Id);

        //    InlineQueryResult[] results = {
        //    // displayed result
        //    new InlineQueryResultArticle(
        //        id: "1",
        //        title: "TgBots",
        //        inputMessageContent: new InputTextMessageContent("hello"))
        //};

        //    await _botClient.AnswerInlineQueryAsync(
        //        inlineQueryId: inlineQuery.Id,
        //        results: results,
        //        cacheTime: 0,
        //        isPersonal: true,
        //        cancellationToken: cancellationToken);
        //}

        //private async Task BotOnChosenInlineResultReceived(ChosenInlineResult chosenInlineResult, CancellationToken cancellationToken)
        //{
        //    _logger.LogInformation("Received inline result: {ChosenInlineResultId}", chosenInlineResult.ResultId);

        //    await _botClient.SendTextMessageAsync(
        //        chatId: chosenInlineResult.From.Id,
        //        text: $"You chose result with Id: {chosenInlineResult.ResultId}",
        //        cancellationToken: cancellationToken);
        //}

        #endregion
        private Task UnknownUpdateHandlerAsync(Update update, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Unknown update type: {UpdateType}", update?.Type ?? 0);
            return Task.CompletedTask;
        }
    }
}
