using DiscordBot.Entities.Pinterest;
using SQLite;

namespace DiscordBot.Modules.Pinterest
{
    public class PinterestDBController(SQLiteConnection connection) : BaseController<PinterestSettingsModel>(connection)
    {
        public PinterestSettingsModel? GetSettings(ulong guildId)
        {
            return _connection
                .Table<PinterestSettingsModel>()
                .SingleOrDefault(x => x.GuildId == guildId.ToString());
        }
    }
}
