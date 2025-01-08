using Discord.Interactions;

namespace DiscordBot.Modules.TestModule
{
    public class TestInteractionModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand(name: "test", description: "Выполняет тестовую команду")]
        public async Task Test()
        {
            await RespondAsync("Test command executed");
        }
    }
}
