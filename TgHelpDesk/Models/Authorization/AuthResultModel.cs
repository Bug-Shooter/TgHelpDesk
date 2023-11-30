namespace TgHelpDesk.Models.Authorization
{
    public record AuthResultModel
    {
        public string? access_token { get; set; }
        public long TgId { get; set; } //TODO:Заменить на имя курьера
    }
}
