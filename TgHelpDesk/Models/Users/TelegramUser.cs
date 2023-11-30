using TgHelpDesk.Models.Core;

namespace TgHelpDesk.Models.Users
{
    public class TelegramUser
    {
        public int Id { get; set; }

        public string Name { get; set; } = "Не задано";
        public string TgUsername { get; set; } = string.Empty;
        public long TelegramId { get; set; } //TODO: Обеспечить уникальнось
        public long ChatId { get; set; }

        public List<HelpRequest> HelpRequests { get; set; }

    }
}
