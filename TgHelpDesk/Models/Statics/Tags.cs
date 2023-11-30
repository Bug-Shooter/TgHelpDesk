using System.Collections.ObjectModel;

namespace TgHelpDesk.Models.Statics
{
    public static class Tags
    {
        public static readonly ReadOnlyCollection<string> tags = new ReadOnlyCollection<string>(new[]
        {
            "#1C",
            "#Почта",
            "#Печать",
            "#Удаленка",
            "#Сертификат",
        });
    }
}
