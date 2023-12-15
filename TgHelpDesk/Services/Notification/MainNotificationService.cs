using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TgHelpDesk.Data;
using TgHelpDesk.Models.Audit;
using TgHelpDesk.Models.Core;
using TgHelpDesk.Models.Users;
using TgHelpDesk.Services.Bot;
using TgHelpDesk.Services.Notification.Interfaces;
using static TgHelpDesk.Models.Core.HelpRequest;

namespace TgHelpDesk.Services.Notification
{
    public class MainNotificationService : INotificationService<HelpRequest>
    {
        private readonly ILogger<MainNotificationService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly BotMethods _botMethods;
        public MainNotificationService(ILogger<MainNotificationService> logger, BotMethods botMethods, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _botMethods = botMethods;
        }

        public async Task OnCreate(HelpRequest helpRequest, CancellationToken cancellationToken = default)
        {
            TelegramUser tgUser;
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TgHelpDeskDbContext>();
                tgUser = await dbContext.TelegramUsers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == helpRequest.TelegramUser.Id);
            }

            string msg = "Заявка принята!\n" +
            $"<b>Номер заявки:</b> #id{helpRequest.Id}\n" +
            $"<b>Тема:</b> {helpRequest.Title}\n" +
            $"<b>Описание:</b> {helpRequest.Text}\n" +
            $"<b>Дата подачи:</b> {helpRequest.CreationDateTime}\n" +
            $"<b>Приоритет:</b> {GetPriority(helpRequest.Priority)}\n" +
            $"Мы уже работаем над вашей задачей) Об изменениях статуса будут присылаться уведомления.";
            await _botMethods.SendMessageAsync(msg, tgUser.TelegramId, cancellationToken);
            await _botMethods.SendLogMsg(msg + "\n*\n <b>Получена от: </b>" + tgUser.Name + "(" + tgUser.TgUsername + ")" + $"({tgUser.TelegramId})");

        }


        public async Task OnUpdate(HelpRequest helpRequest, HelpRequest OldHelpRequest, CancellationToken cancellationToken = default)
        {
            TelegramUser TgUser;
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TgHelpDeskDbContext>();
                TgUser = await dbContext.TelegramUsers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == helpRequest.TelegramUser.Id);
            }

            if (helpRequest.Status != OldHelpRequest.Status)
            {
                //Уведомление Юзера
                await _botMethods.SendMessageAsync($"<b>Статус</b> вашего запроса (#id{helpRequest.Id}) <i>\"{helpRequest.Title}\"</i> изменен на <i>\"{GetStatus(helpRequest.Status)}\"</i>.", TgUser.ChatId, cancellationToken);

                //Уведомление в лог
                await _botMethods.SendLogMsg($"<b>Статус</b> запроса (#id{helpRequest.Id}) <i>\"{helpRequest.Title}\"</i> изменен на <i>\"{GetStatus(helpRequest.Status)}\"</i>.");
            }

            if (helpRequest.Priority != OldHelpRequest.Priority)
            {
                //Уведомление Юзера
                await _botMethods.SendMessageAsync($"<b>Приоритет</b> вашего запроса (#id{helpRequest.Id}) <i>\"{helpRequest.Title}\"</i> изменен на <i>\"{GetPriority(helpRequest.Priority)}\"</i>.", TgUser.ChatId, cancellationToken);

                //Уведомление в лог
                await _botMethods.SendLogMsg($"<b>Приоритет</b> запроса (#id{helpRequest.Id} ) <i>\" {helpRequest.Title}\"</i> изменен на <i>\"{GetPriority(helpRequest.Priority)}\"</i>.");
            }
        }

        public async Task OnDelete(HelpRequest helpRequest, CancellationToken cancellationToken = default)
        {
            TelegramUser user;
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TgHelpDeskDbContext>();
                user = await dbContext.TelegramUsers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == helpRequest.TelegramUser.Id);
            }
            string msg = $"Заявка #id{helpRequest.Id} с темой {helpRequest.Title} удалена.";
            await _botMethods.SendMessageAsync(msg, user.ChatId);
            await _botMethods.SendLogMsg(msg);
        }
    }
}
