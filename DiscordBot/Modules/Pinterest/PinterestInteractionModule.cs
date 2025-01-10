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

            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var str = await response.Content.ReadAsStreamAsync();
                        var document = new HtmlDocument();
                        document.LoadHtml(content);

                        var nodes = document.DocumentNode.SelectNodes("//div[@class='OVX lnZ mQ8 oy8 zI7 iyn Hsu']/descendant::div/img|//div[@class='OVX lnZ mQ8 oy8 zI7 iyn Hsu']/descendant::div/video");
                        if (nodes.Count() > 1) throw new Exception("Incorrect xPath - many nodes found");

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
                result = ex.Message;
            }
            finally
            {
                await RespondAsync(result);
            }
        }
    }
}
