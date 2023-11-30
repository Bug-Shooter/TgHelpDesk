using System.ComponentModel.DataAnnotations.Schema;
using TgHelpDesk.Models.Users;
using static TgHelpDesk.Models.Core.HelpRequest;

namespace TgHelpDesk.Models.Core
{
    public class HelpRequest
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public DateTime CreationDateTime { get; set; } = DateTime.UtcNow;
        public TelegramUser TelegramUser { get; set; }

        public _Priority Priority { get; set; }
        #region Priority
        public enum _Priority
        {
            Low = 1,
            Medium,
            High
        }
        public string GetPriority()
        {
            switch (Priority)
            {
                case _Priority.Low:
                    return "Низкий";
                case _Priority.Medium:
                    return "Средний";
                case _Priority.High:
                    return "Высокий";
                default:
                    return "Ошибка";
            }
        }
        #endregion

        public _Status Status { get; set; }
        #region Status
        public enum _Status
        {
            Received = 1,
            InWork,
            Completed
        }
        public string GetStatus()
        {
            switch (Status)
            {
                case _Status.Received:
                    return "Получена";
                case _Status.InWork:
                    return "В работе";
                case _Status.Completed:
                    return "Завершена";
                default:
                    return "Ошибка";
            }
        }
        #endregion
    }

    public static class HelpRequestRadzenData //Сервисный класс. нужен для отображения в Radzen.
    {
        public static readonly IEnumerable<PriorityElement> PriorityList = new[]
{
            new PriorityElement { Pd = _Priority.Low, Title = "Низкий"},
            new PriorityElement { Pd = _Priority.Medium, Title = "Средний"},
            new PriorityElement { Pd = _Priority.High, Title = "Высокий"}
        };

        public static readonly IEnumerable<StatusElement> StatusList = new[]
        {
            new StatusElement { Pd = _Status.Received, Title = "Получен"},
            new StatusElement { Pd = _Status.InWork, Title = "В работе"},
            new StatusElement { Pd = _Status.Completed, Title = "Завершен"}
        };
        public class StatusElement
        {
            public _Status Pd { get; set; }
            public string Title { get; set; } = string.Empty;
        }
        public class PriorityElement
        {
            public _Priority Pd { get; set; }
            public string Title { get; set; } = string.Empty;
        }
    }

}
