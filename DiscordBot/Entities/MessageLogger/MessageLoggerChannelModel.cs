using DiscordBot.Entities.Base;
using SQLite;

namespace DiscordBot.Entities.MessageLogger
{
    [Table("MessageLoggerLoggedChannels")]
    public class MessageLoggerChannelModel : ChannelBase
    {
        public string GuildId { get; set; }
    }
}
