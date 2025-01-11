using DiscordBot.Entities.Base;
using SQLite;

namespace DiscordBot.Entities.Monitor
{
    [Table("MonitorSettings")]
    public class MonitorSettingsModel : GuildBase
    {
        public string ReportToChannelId { get; set; }
    }
}
