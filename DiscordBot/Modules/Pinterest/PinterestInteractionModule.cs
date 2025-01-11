using Discord;
using Discord.Interactions;
using DiscordBot.Services;
using HtmlAgilityPack;
using static SQLite.SQLite3;

namespace DiscordBot.Modules.Pinterest
{
    public class PinterestInteractionModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly PinterestDBController _controller;

        public PinterestInteractionModule(CommandHandlingService handler)
        {
            _controller = new PinterestDBController(handler._sqlite);
        }

        [RequireBotPermission(GuildPermission.SendMessages)]
        [SlashCommand(name: "pin", description: "Преобразовывает ссылку Pinterest на читаемую дискордом контент типа изображение или видео")]
        public async Task Pin(string url)
        {
            var result = string.Empty;
            var isEphemeral = false;
            try
            {
                result = await GetContentFromPin(url);
            }
            catch (Exception ex)
            {
                isEphemeral = true;
                result = ex.Message;
            }
            
            await RespondAsync(result, ephemeral: isEphemeral);
        }

        public static async Task<string> GetContentFromPin(string url)
        {
            var result = string.Empty;

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

            return result;
        }

        [DefaultMemberPermissions(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.ManageMessages | GuildPermission.SendMessages)]
        [SlashCommand(name: "pin-allow-modify-messages", description: "Позволяет боту редактировать ссылки на Pinterest в сообщениях автоматически")]
        public async Task PinAllowModifyMessages(bool allow)
        {
            await RespondAsync("Выполняю настройку...", ephemeral: true);

            var settings = _controller.GetSettings(Context.Guild.Id);
            if (settings != null)
            {
                settings.GuildName = Context.Guild.Name;
                settings.AllowAutoModifyMessages = allow;
                _controller.Update(settings);
                await ModifyOriginalResponseAsync(x => x.Content = $"Настройки модуля обновлены");
            }
            else
            {
                settings = new Entities.Pinterest.PinterestSettingsModel
                {
                    GuildName = Context.Guild.Name,
                    GuildId = Context.Guild.Id.ToString(),
                    AllowAutoModifyMessages = allow
                };
                _controller.Add(settings);
                await ModifyOriginalResponseAsync(x => x.Content += $"Новые настройки модуля сохранены");
            }
        }
    }
}