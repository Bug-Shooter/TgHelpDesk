using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Web;
using TgHelpDesk.Models.Authorization;
using TgHelpDesk.Models.Statics;
using TgHelpDesk.Services.Bot;
using TgHelpDesk.Services.TgUsers;

namespace TgHelpDesk.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthorizationController : Controller
    {
        const string constantKey = "WebAppData"; // Constant key to genrate secret key.

        private readonly BotConfiguration _botConfig;
        private readonly TgUsersService _tgUsersService;
        private readonly ILogger<AuthorizationController> _logger; //TODO:Add Logging

        public AuthorizationController(IOptions<BotConfiguration> botOptions, TgUsersService tgUsersService, ILogger<AuthorizationController> logger)
        {
            _botConfig = botOptions.Value;
            _tgUsersService = tgUsersService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Post(AuthRequestModel model)
        {
            // Parse string initData from telegram.
            var data = HttpUtility.ParseQueryString(model.initData.ToString());

            // Put data in a alphabetically sorted dict.
            var dataDict = new SortedDictionary<string, string>(
                data.AllKeys.ToDictionary(x => x!, x => data[x]!),
                StringComparer.Ordinal);

            // https://core.telegram.org/bots/webapps#validating-data-received-via-the-web-app:
            // Data-check-string is a chain of all received fields, sorted alphabetically in the format key=<value> with a line feed character ('\n', 0x0A) used as separator. e.g., 'auth_date=<auth_date>\nquery_id=<query_id>\nuser=<user>'
            var dataCheckString = string.Join(
                '\n', dataDict.Where(x => x.Key != "hash") // Hash should be removed.
                    .Select(x => $"{x.Key}={x.Value}")); // like auth_date=<auth_date> ..

            // secrecKey is the HMAC-SHA-256 signature of the bot's token with the constant string WebAppData used as a key.
            var secretKey = HMACSHA256.HashData(
                Encoding.UTF8.GetBytes(constantKey),
                Encoding.UTF8.GetBytes(_botConfig.BotToken));

            var generatedHash = HMACSHA256.HashData(
                secretKey,
                Encoding.UTF8.GetBytes(dataCheckString));

            // Convert received hash from telegram to a byte array.
            var actualHash = Convert.FromHexString(dataDict["hash"]);

            // Compare our hash with the one from telegram.
            if (!actualHash.SequenceEqual(generatedHash))
                return Unauthorized();

            if (!await _tgUsersService.CheckTgUserExistence(model.TgId))
                return Unauthorized();

            var token = _tgUsersService.CreateJwtForId(model.TgId);

            HttpContext.Session.SetString(SessionKeys.TgId, model.TgId.ToString());

            _logger.LogInformation($"TgId {model.TgId} Authorized successfully.");

            return Ok(token);
        }
    }
}
