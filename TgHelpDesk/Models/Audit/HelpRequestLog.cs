namespace TgHelpDesk.Models.Audit
{
    public class HelpRequestLog
    {
        public int Id { get; set; }

        public string? EntityName { get; set; }

        public DateTime OperatedAt { get; set; }

        public string? KeyValues { get; set; }

        public string? OldValues { get; set; }

        public string? NewValues { get; set; }
    }
}
