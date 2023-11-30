using Microsoft.EntityFrameworkCore;
using System.Threading;
using TgHelpDesk.Data;
using TgHelpDesk.Models.Core;
using TgHelpDesk.Services.Bot;
using TgHelpDesk.Services.TgUsers;

namespace TgHelpDesk.Services.HelpRequests
{
    public class HelpRequestsService
    {
        private readonly TgHelpDeskDbContext _dbContext;
        private readonly TgUsersService _tgUsersService;
        private readonly BotMethods _botMethods;
        private readonly ILogger<HelpRequestsService> _logger;
        public HelpRequestsService(TgHelpDeskDbContext tgHelpDeskDb, TgUsersService tgUsersService, ILogger<HelpRequestsService> logger)
        {
            _dbContext = tgHelpDeskDb;
            _tgUsersService = tgUsersService;
            _logger = logger;
        }

        public async Task<HelpRequest> AddHelpRequest(string Title, string Text, long TgId, int Priority)
        {
            HelpRequest helpRequest = new()
            {
                TelegramUser = await _tgUsersService.GetTgUser(TgId),
                Title = Title,
                Text = Text,
                CreationDateTime = DateTime.UtcNow,
                Priority = (HelpRequest._Priority)Priority,
                Status = HelpRequest._Status.Received
            };
            _dbContext.HelpRequests.Add(helpRequest);

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("New help request received from id(" + TgId + ") with title (" + Title + ").");

            return helpRequest;
        }
    }
}
