using Discord.WebSocket;
using DiscordBot.Interfaces;

namespace DiscordBot.Modules
{
    public class DiscordModuleBase : IDiscordModule
    {
        internal readonly DiscordSocketClient _client;
        internal readonly BaseController _controller;

        internal DiscordModuleBase(DiscordSocketClient client, BaseController controller)
        {
            _client = client;
            _controller = controller;
        }
    }
}
