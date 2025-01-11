using DiscordBot.Entities.Base;
using SQLite;

namespace DiscordBot.Entities.Monitor
{
    [Table("MonitorReportToChannel")]
    public class MonitorReportToChannelModel : ChannelBase
    {
        public string GuildId { get; set; }
    }
}
