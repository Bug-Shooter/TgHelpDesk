namespace TgHelpDesk.Services.Bot;

public class BotConfiguration
{
    public static readonly string Configuration = "BotConfiguration";
    public string BotToken { get; init; } = default!;
    public string HostAddress { get; init; } = default!;
    public string Route { get; init; } = default!;
    public string SecretToken { get; init; } = default!;
    public long LoggerChatId { get; init; } = default!;
    public int LoggerReplyMsg { get; init; } = default!;
}
