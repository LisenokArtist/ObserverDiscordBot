using DiscordBot.Entities.Pinterest;
using DiscordBot.Structures;
using SQLite;

namespace DiscordBot.Modules.Pinterest
{
    public class PinterestDBController : BaseController<PinterestSettingsModel>
    {
        public PinterestDBController(SQLiteConnection connection) : base(connection) { }

        public PinterestSettingsModel? GetSettings(ulong guildId)
        {
            return _connection
                .Table<PinterestSettingsModel>()
                .SingleOrDefault(x => x.GuildId == guildId.ToString());
        }
    }
}
