using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot.Interfaces;
using SQLite;

namespace DiscordBot.Core.DiscordNetExtensions
{
    public class InteractionServiceExtended : InteractionService
    {
        internal DiscordSocketClient Client;
        internal SQLiteConnection SQLite;
        public IReadOnlyCollection<KeyValuePair<IDiscordModule, IController>> DiscordModules { get { return _discordModules; } }
        internal HashSet<KeyValuePair<IDiscordModule, IController>> _discordModules = new HashSet<KeyValuePair<IDiscordModule, IController>>();

        public InteractionServiceExtended(
            DiscordSocketClient client,
            SQLiteConnection sqlite,
            InteractionServiceConfig? config = null) : base(client.Rest, config)
        {
            Client = client;
            SQLite = sqlite;
        }

        internal bool AddDiscordModule(KeyValuePair<IDiscordModule, IController> pair)
        {
            return _discordModules.Add(pair);
        }

        internal bool AddDiscordModule(IDiscordModule module, IController controller)
        {
            var pair = new KeyValuePair<IDiscordModule, IController>(module, controller);
            return AddDiscordModule(pair);
        }
    }
}
