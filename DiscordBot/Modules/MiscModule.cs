using Discord.Commands;
using Discord.Interactions;
using DiscordBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Modules
{

    public class MiscModule : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService Commands { get; set; }
        private CommandHandlingService _handler;
        
        public MiscModule(CommandHandlingService handler)
        {
            _handler = handler;
        }

        //[SlashCommand("echo", "recieve a message")]
        //public async Task EchoAsync([Remainder] string text)
        //{
        //    await ReplyAsync('\u200B' + text);
        //}

        [SlashCommand("echo", "Echo an input")]
        public async Task Echo(string input)
        {
            await RespondAsync(input);
        }
    }
}
