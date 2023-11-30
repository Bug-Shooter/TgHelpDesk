using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TgHelpDesk.Data;
using TgHelpDesk.Models.Authorization;
using TgHelpDesk.Models.Statics;
using TgHelpDesk.Models.Users;

namespace TgHelpDesk.Services.TgUsers
{
    public class TgUsersService
    {
        private readonly TgHelpDeskDbContext _dbContext;
        public TgUsersService(TgHelpDeskDbContext helpDeskDbContext) 
        {
            _dbContext = helpDeskDbContext;
        }

        public async Task<TelegramUser> RegistereNewUser(long TgUserId, long ChatId, string Username)
        {
            if (await _dbContext.TelegramUsers.AnyAsync(x => x.TelegramId == TgUserId))
                return await _dbContext.TelegramUsers.FirstOrDefaultAsync(x=>x.TelegramId == TgUserId);

            var TelegramUser = new TelegramUser()
            {
                ChatId = ChatId,
                TelegramId = TgUserId,
                TgUsername = Username,
            };
      
            _dbContext.TelegramUsers.Add(TelegramUser);
            await _dbContext.SaveChangesAsync();

            return TelegramUser;
        }

        public async Task<bool> CheckTgUserExistence(long TgUserId) => await _dbContext.TelegramUsers.AnyAsync(x=>x.TelegramId==TgUserId);

        public AuthResultModel CreateJwtForId(long Id)
        {
            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.Name, Id.ToString()));

            // создаем JWT-токен
            var jwt = new JwtSecurityToken(
                    issuer: AuthOptions.ISSUER,
                    audience: AuthOptions.AUDIENCE,
            claims: claims,
            expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(2)),
                    signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            // формируем ответ
            AuthResultModel response = new()
            {
                access_token = encodedJwt,
                TgId = Id
            };

            return response;
        }

        public async Task<TelegramUser> GetTgUser(long TgId) => await _dbContext.TelegramUsers.FirstOrDefaultAsync(x => x.TelegramId == TgId);
    }
}
