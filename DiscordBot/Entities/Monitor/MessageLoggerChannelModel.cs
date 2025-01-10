using DiscordBot.Entities.Base;
using SQLite;

namespace DiscordBot.Entities.Monitor
{
    [Table("MessageLoggerLoggedChannels")]
    public class MessageLoggerChannelModel : ChannelBase
    {
        public string GuildId { get; set; }
    }
}
