using DiscordBot.Entities.Base;
using SQLite;

namespace DiscordBot.Entities.Pinterest
{
    [Table("PinterestSettings")]
    public class PinterestSettingsModel : GuildBase
    {
        public bool AllowAutoModifyMessages { get; set; }
    }
}
