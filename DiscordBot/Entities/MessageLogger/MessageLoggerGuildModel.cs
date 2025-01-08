using DiscordBot.Entities.Base;
using SQLite;

namespace DiscordBot.Entities.MessageLogger
{
    [Table("MessageLoggerGuildsSettings")]
    public class MessageLoggerGuildModel : GuildBase
    {
        public string RespondChannelId { get; set; }
    }
}
