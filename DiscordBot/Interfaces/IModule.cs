namespace DiscordBot.Interfaces
{
    public class BaseModuleSettings
    {
        public ulong GuildId { get; set; }

        public BaseModuleSettings(ulong guildId)
        {
            GuildId = guildId;
        }
    }

    public interface IModule
    {
        public void LoadSettings();

        public void SaveSettings();
    }
}
