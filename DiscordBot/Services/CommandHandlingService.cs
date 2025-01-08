using Discord.Interactions;
using Discord.WebSocket;
using SQLite;
using System.Reflection;

namespace DiscordBot.Services
{
    public class CommandHandlingService
    {
        internal readonly DiscordSocketClient _client;
        private readonly InteractionService _commands;
        private readonly IServiceProvider _services;
        internal readonly SQLiteConnection _sqlite;

        public CommandHandlingService(SQLiteConnection sqlite, DiscordSocketClient client, InteractionService commands, IServiceProvider services)
        {
            _sqlite = sqlite;
            _client = client;
            _commands = commands;
            _services = services;
        }

        public async Task InitializeAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            _client.InteractionCreated += HandleInteraction;

            _commands.SlashCommandExecuted += SlashCommandExecuted;
            _commands.ContextCommandExecuted += ContextCommandExecuted;
            _commands.ComponentCommandExecuted += ComponentCommandExecuted;
        }

        private Task ComponentCommandExecuted(ComponentCommandInfo arg1, Discord.IInteractionContext arg2, Discord.Interactions.IResult arg3)
        {
            return Task.CompletedTask;
            //throw new NotImplementedException();
        }

        private Task ContextCommandExecuted(ContextCommandInfo arg1, Discord.IInteractionContext arg2, Discord.Interactions.IResult arg3)
        {
            return Task.CompletedTask;
            //throw new NotImplementedException();
        }

        private Task SlashCommandExecuted(SlashCommandInfo arg1, Discord.IInteractionContext arg2, Discord.Interactions.IResult arg3)
        {
            return Task.CompletedTask;
            //throw new NotImplementedException();
        }

        private async Task HandleInteraction(SocketInteraction arg)
        {
            try
            {
                var sic = new SocketInteractionContext(_client, arg);
                await _commands.ExecuteCommandAsync(sic, _services);
            }
            catch (Exception ex)
            {
                if (arg.Type == Discord.InteractionType.ApplicationCommand)
                {
                    await arg.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
                }
            }
            //throw new NotImplementedException();
        }
    }
}
