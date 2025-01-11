using DiscordBot.Entities.Base;
using SQLite;

namespace DiscordBot.Entities.Monitor
{
    [Table("MonitorChannels")]
    public class MonitorChannelModel : ChannelBase
    {
        public string GuildId { get; set; }
    }
}
