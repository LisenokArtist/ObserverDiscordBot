using DiscordBot.Entities.Base;
using SQLite;

namespace DiscordBot.Entities.Monitor
{
    [Table("MessageLoggerLoggedToChannel")]
    public class MessageLoggerLoggedToChannelModel : ChannelBase
    {
        public string GuildId { get; set; }
    }
}
