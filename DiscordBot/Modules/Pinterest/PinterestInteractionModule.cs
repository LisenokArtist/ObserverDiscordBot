using Discord.Interactions;
using HtmlAgilityPack;

namespace DiscordBot.Modules.Pinterest
{
    public class PinterestInteractionModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand(name: "pin", description: "Парсит ссылку Pinterest и возвращает контент")]
        public async Task Pin(string url)
        {
            var result = string.Empty;
            var isEphemeral = false;
            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var document = new HtmlDocument();
                        document.LoadHtml(content);

                        var nodes = document.DocumentNode
                            .SelectNodes("//video|//img");
                        var node = nodes.First();
                        var imgSrc = node.Attributes["src"].Value;
                        result = node.Name switch
                        {
                            "img" => node.Attributes["src"].Value,
                            "video" => node.Attributes["src"].Value.Replace("hls", "720p").Replace("m3u8", "mp4"),
                            _ => throw new NotImplementedException($"Not implemented exception of {node.Name} element"),
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                isEphemeral = true;
                result = ex.Message;
            }
            finally
            {
                await RespondAsync(result, ephemeral: isEphemeral);
            }
        }
    }
}
