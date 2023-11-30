using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types.Enums;
using TgHelpDesk.Models.Core;
using TgHelpDesk.Models.Statics;
using TgHelpDesk.Services.Bot;
using TgHelpDesk.Services.HelpRequests;
using TgHelpDesk.Services.TgUsers;

namespace TgHelpDesk.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HelpRequestsService _helpRequestsService;
        private readonly TgUsersService _tgUsersService;
        private readonly BotMethods _botMethods;

        public HomeController(ILogger<HomeController> logger, HelpRequestsService helpRequestsService, TgUsersService tgUsersService, BotMethods botMethods)
        {
            _logger = logger;
            _helpRequestsService = helpRequestsService;
            _tgUsersService = tgUsersService;
            _botMethods = botMethods;
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromBody]HelpWebRequest request)
        {
            if (!await _tgUsersService.CheckTgUserExistence(request.TgId))
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Title))
                return BadRequest();

            var req = await _helpRequestsService.AddHelpRequest(request.Title, request.Description, request.TgId, request.Priority);

            await _botMethods.SendMessage
                ("Ваша заявка принята.\n" +
                $"<b>Номер заявки:</b> #id{req.Id}\n" +
                $"<b>Тема:</b> {req.Title}\n" +
                $"<b>Описание:</b> {req.Text}\n" +
                $"<b>Дата подачи:</b> {req.CreationDateTime}\n" +
                $"<b>Приоритет:</b> {req.GetPriority()}\n" +
                $"С вами свяжутся, а о изменение статуса будет отправлено дополнительное сообщение.",
                req.TelegramUser.TelegramId);


            return Ok();
        }
    }
}
