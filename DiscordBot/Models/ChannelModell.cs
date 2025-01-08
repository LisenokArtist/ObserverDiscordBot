using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Models
{
    public class ChannelModell
    {
        public ulong Id { get; set; }
        public required string Name { get; set; }
    }
}
