using Discord.WebSocket;
using DiscordBot.Modules;
using DiscordBot.Modules.Pinterest;
using System.Text.RegularExpressions;

namespace DiscordBot.Entities.Pinterest
{
    public class PinterestModule : DiscordModuleBase
    {
        internal PinterestDBController Controller { get { return (PinterestDBController)_controller; } }

        public PinterestModule(DiscordSocketClient client, PinterestDBController controller) : base(client, controller)
        {
            _client.MessageReceived += MessageReceived;
        }

        private async Task MessageReceived(SocketMessage message)
        {
            var channel = message.Channel as SocketGuildChannel;
            if (channel == null) return;

            var settings = Controller.GetSettings(channel.Guild.Id);
            if (settings == null) return;

            if (settings.AllowAutoModifyMessages)
            {
                var pinLinks = ExtractPinterestLinks(message.Content);
                if (pinLinks.Any())
                {
                    var pinLinksModified = await ModifyPinterestLinks(pinLinks);
                    if (pinLinksModified.Any())
                    {
                        await message.Channel.SendMessageAsync(
                            text: string.Join(Environment.NewLine, pinLinksModified),
                            messageReference: new Discord.MessageReference(messageId: message.Id));
                    }
                }
            }
        }

        #region Static
        public static async Task<IEnumerable<string>> ModifyPinterestLinks(IEnumerable<string> urls)
        {
            var result = new List<string>();
            foreach (var url in urls)
            {
                var pin = await PinterestInteractionModule.GetContentFromPin(url);
                if (pin != string.Empty)
                    result.Add(pin);
            }
            return result.AsEnumerable();
        }

        public static IEnumerable<string> ExtractPinterestLinks(string text)
        {
            string[] whiteList = [
                "pinterest.com",
                "pinterest.ca",
                "pinterest.co.uk",
                "pinterest.fr",
                "pinterest.de",
                "pinterest.es",
                "pin.it",
                "pinterest.com.au",
                "pinterest.ph",
                "pinterest.ch",
                "pinterest.com.mx",
                "pinterest.dk",
                "pinterest.pt",
                "pinterest.ru",
                "pinterest.it",
                "pinterest.at",
                "pinterest.jp",
                "pinterest.cl",
                "pinterest.ie",
                "pinterest.co.kr",
                "pinterest.nz",
                "pintrest.com",
                "pinterest100.com",
                "pinterestcareers.com",
                "pinterst.com",
                "pinimg.com",
                "pinterest.vn",
                "pinterest.co",
                "pinterest.com.uy",
                "pinterest.com.pe",
                "pinterest.nl",
                "lecker-in-bestform.de",
                "pinterestmail.com",
                "pinterest.co.id",
                "pinterestpresents.co.uk",
                "pinterestpresents.de",
                "pinterest-mail.com",
                "pinterestpresents.com.au",
                "winterhus.ch",
                "pinterestpredicts.com",
                "pintrist.com",
                "traeumezimmer.ch",
                "pinterest.com.py",
                "http://pinterest.co.nz/pinterest.be",
                "pinterest.hu",
                "pintergration.com",
                "pinterest.kr",
                "pinterest.co.in",
                "pinterest.pe",
                "pinterest.engineering",
                "pinttest.com",
                "pintereststatus.com",
                "pinterest.uk",
                "pinterest.com.vn",
                "pinterest.tw",
                "pinterest.mx",
                "pinterest.com.ec",
                "pinterest.biz",
                "pinterest.com.bo",
                "pintertools.com",
                "querybook.org",
                "pinterest.ec",
                "pinterest.id",
                "pinterest.in",
                "pinterest.th",
                "pinterest.co.at",
                "pinterest.com.pt",
                "pinterest.info"];

            string pattern = @"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-z]{2,4}\b([-a-zA-Z0-9@:%_\+.~#?&//=]*)";
            var matches = Regex.Matches(text, pattern);
            var result = from m in matches
                         where whiteList.Any(x => m.Value.Contains(x))
                         select m.Value;
            return result;
        }
        #endregion
    }
}