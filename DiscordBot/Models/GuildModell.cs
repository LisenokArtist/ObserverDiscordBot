namespace DiscordBot.Models
{
    public class GuildModell
    {
        public ulong Id { get; set; }
        
        public required string Name { get; set; }

        public List<ChannelModell> Channels { get; set; } = new List<ChannelModell>();
    }
}
