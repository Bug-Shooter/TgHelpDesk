using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using Microsoft.Extensions.Options;
using TgHelpDesk.Services.Bot;
using TgHelpDesk.Models.Core;
using TgHelpDesk.Services.TgUsers;
using TgHelpDesk.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Text;

namespace TgHelpDesk.Services.Bot
{
    public class UpdateHandlers
    {
        private readonly ILogger<UpdateHandlers> _logger;
        private readonly TgUsersService _tgUsersService;
        private readonly TgHelpDeskDbContext _dbContext;
        private readonly BotMethods _botMethods;

        public UpdateHandlers(ILogger<UpdateHandlers> logger, BotMethods botMethods, TgUsersService tgUsersService, TgHelpDeskDbContext dbContext)
        {
            _tgUsersService = tgUsersService;
            _dbContext = dbContext;
            _logger = logger;
            _botMethods = botMethods;
        }

        public Task HandleErrorAsync(Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            _logger.LogInformation("HandledError: {ErrorMessage}", ErrorMessage);
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
            if (message.Chat.IsForum == true)
            {
                _ = BotNotForChatUsage(message, _botMethods, cancellationToken);
                return;
            }

            if (message.Text is not { } messageText)
                return;


            var action = messageText.Split(' ')[0] switch
            {
                "/start" => SendStartMessage(message, _tgUsersService, _botMethods, cancellationToken),
                "/help" => Help(message, _botMethods, cancellationToken),
                "/status" => Status(message, _botMethods, _dbContext, cancellationToken),
                "/old" => Old(message, _botMethods, _dbContext, cancellationToken),
                //"/inline_keyboard" => SendInlineKeyboard(_botClient, message, cancellationToken),
                //"/keyboard" => SendReplyKeyboard(_botClient, message, cancellationToken),
                //"/remove" => RemoveKeyboard(_botClient, message, cancellationToken),
                //"/photo" => SendFile(_botClient, message, cancellationToken),
                //"/request" => RequestContactAndLocation(_botClient, message, cancellationToken),
                //"/inline_mode" => StartInlineQuery(_botClient, message, cancellationToken),
                _ => Unknown(message, _botMethods, cancellationToken)
            };
            Message sentMessage = await action;
            _logger.LogInformation("The message was sent with id: {SentMessageId}", sentMessage.MessageId);

            static async Task<Message> SendStartMessage(Message message, TgUsersService _tgUsersService, BotMethods botMethods, CancellationToken cancellationToken)
            {
                if(!await _tgUsersService.CheckTgUserExistence(message.From.Id))
                    await _tgUsersService.RegistereNewUser(message.From.Id, message.Chat.Id, message.From.Username ?? "Unknown");

                //TODO: Вариативность в зависимости от статуса авторизации

                string StartMessage = "Привет!\n" +
                                      "Я Бот. Я отправлю ваш запрос IT-отделу\n" +
                                      "Жми <u>'Новый Запрос'</u>\n" +
                                      "/help - для дополнительной информации";

                await botMethods.SetMenuButton(message.Chat.Id,cancellationToken);
                return await botMethods.SendMessageAsync(StartMessage, message.Chat.Id, cancellationToken);
            }

            #region NotUsed
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
            #endregion

            static async Task<Message> Help(Message message, BotMethods botMethods, CancellationToken cancellationToken)
            {
                string Message = "Кнопка <u>'Новый Запрос'</u> вызовет форму запроса\n" +
                                 "Можете добавлять теги, чтобы обозначить предметную область\n" +
                                 "Пожалуйста, опишите задачу максимально полно\n" +
                                 "/status - покажет статус актуальных заявок\n" +
                                 "/old - архив ваших заявок";

                return await botMethods.SendMessageAsync(Message, message.Chat.Id, cancellationToken);
            }

            static async Task<Message> Status(Message message, BotMethods botMethods, TgHelpDeskDbContext tgHelpDeskDb, CancellationToken cancellationToken)
            {

                var Requests = await tgHelpDeskDb.HelpRequests.Include(x => x.TelegramUser).Where(x => x.TelegramUser.TelegramId == message.Chat.Id && x.Status != Models.Core.HelpRequest._Status.Completed).AsNoTracking().ToListAsync();
                string Message = "";                             

                if(Requests.Count == 0)
                {
                    Message = "У вас нет активных заявок";
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append($"У вас {Requests.Count} активных заявок.\n");
                    sb.Append($"<ol>");                 
                    foreach(var request in Requests)
                    {
                        sb.Append($"#id{request.Id} - {request.Title}\n" +
                            $"Отправлена: {request.CreationDateTime}" +
                            $"Статус: {HelpRequest.GetStatus(request.Status)}");
                    }
                    sb.Append("</ol>");
                    Message = sb.ToString();
                }

                return await botMethods.SendMessageAsync(Message, message.Chat.Id, cancellationToken);
            }

            static async Task<Message> Old(Message message, BotMethods botMethods, TgHelpDeskDbContext tgHelpDeskDb, CancellationToken cancellationToken)
            {

                var Requests = await tgHelpDeskDb.HelpRequests.Include(x => x.TelegramUser).Where(x => x.TelegramUser.TelegramId == message.Chat.Id && x.Status == Models.Core.HelpRequest._Status.Completed).AsNoTracking().ToListAsync();
                string Message = "";

                if (Requests.Count == 0)
                {
                    Message = "Вы еще не отправляли заявок";
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append($"У вас {Requests.Count} завершенных заявок.\n");
                    sb.Append($"<ol>");
                    foreach (var request in Requests)
                    {
                        sb.Append($"#id{request.Id} - {request.Title}\n" +
                            $"Отправлена: {request.CreationDateTime.ToShortDateString()}" +
                            $"Текст: {request.Text}");
                    }
                    sb.Append("</ol>");
                    Message = sb.ToString();
                }



                return await botMethods.SendMessageAsync(Message, message.Chat.Id, cancellationToken);
            }

            static async Task<Message> Unknown(Message message, BotMethods botMethods, CancellationToken cancellationToken)
            {
                //const string usage = "Usage:\n" +
                //                     "/inline_keyboard - send inline keyboard\n" +
                //                     "/keyboard    - send custom keyboard\n" +
                //                     "/remove      - remove custom keyboard\n" +
                //                     "/photo       - send a photo\n" +
                //                     "/request     - request location or contact\n" +
                //                     "/inline_mode - send keyboard with Inline Query";

                return await botMethods.SendMessageAsync("Неизвестная команда", message.Chat.Id, cancellationToken);
            }
            static async Task<Message> BotNotForChatUsage(Message message, BotMethods botMethods, CancellationToken cancellationToken)
            {
                return await botMethods.SendReplyMsg("Бот не может работать в групповых чатах и форумах", message.Chat.Id, message.MessageId, cancellationToken);
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
