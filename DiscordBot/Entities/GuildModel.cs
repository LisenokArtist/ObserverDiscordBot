using DiscordBot.Entities.Base;

namespace DiscordBot.Entities
{
    [SQLite.Table("Guilds")]
    public class GuildModel : DataEntityBase
    {
        public ulong GuildId { get; set; }
        public string GuildName { get; set; }
    }
}
