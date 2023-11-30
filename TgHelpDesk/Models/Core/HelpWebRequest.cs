using TgHelpDesk.Models.Users;

namespace TgHelpDesk.Models.Core
{
    public class HelpWebRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public long TgId { get; set; }
        public int Priority {  get; set; }
    }
}
