using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TgHelpDesk.Models.Core;
using TgHelpDesk.Repositories.Interface;
using TgHelpDesk.Services.TgUsers;

namespace TgHelpDesk.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IRepository<HelpRequest> _helpReqRepos;
        private readonly TgUsersService _tgUsersService;

        public HomeController(ILogger<HomeController> logger, IRepository<HelpRequest> repository, TgUsersService tgUsersService)
        {
            _logger = logger;
            _helpReqRepos = repository;
            _tgUsersService = tgUsersService;
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

            HelpRequest helpRequest = new()
            {
                TelegramUser = await _tgUsersService.GetTgUser(request.TgId),
                Title = request.Title,
                Text = request.Description,
                CreationDateTime = DateTime.UtcNow,
                Priority = (HelpRequest._Priority)request.Priority,
                Status = HelpRequest._Status.Received
            };
            await _helpReqRepos.CreateAsync(helpRequest);

            return Ok();
        }
    }
}
