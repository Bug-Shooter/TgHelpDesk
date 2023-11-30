using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace TgHelpDesk.Models.Statics
{
    //TODO: Забирать эти данные с JSON
    public static class AuthOptions
    {
        public const string ISSUER = "TgHelpDesk"; // издатель токена
        public const string AUDIENCE = "TgHelpDesk.Client"; // потребитель токена
        const string KEY = "mysupersecret_secretkey!123";   // ключ для шифрации
        public static SymmetricSecurityKey GetSymmetricSecurityKey() =>
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY));
    }
}
