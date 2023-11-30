using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Telegram.Bot.Types;
using TgHelpDesk.Services.Bot;
using TgHelpDesk.Validators;

namespace TgHelpDesk.Controllers
{
    public class BotController : Controller
    {
        [HttpPost]
        [ValidateTelegramBot]
        public async Task<IActionResult> Post(
            [FromBody] object update,
            [FromServices] UpdateHandlers handleUpdateService,
            CancellationToken cancellationToken)
        {
            var upd = JsonConvert.DeserializeObject<Update>(update.ToString());
            await handleUpdateService.HandleUpdateAsync(upd, cancellationToken);
            return Ok();
        }
    }
}
