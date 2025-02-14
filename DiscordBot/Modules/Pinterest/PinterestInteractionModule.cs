using Discord;
using Discord.Interactions;
using DiscordBot.Core;
using DiscordBot.Core.DiscordNetExtensions;
using DiscordBot.Interfaces;
using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Channels;
using static System.Net.Mime.MediaTypeNames;

namespace DiscordBot.Modules.Pinterest
{
    public class PinterestInteractionModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly PinterestDBController _controller;

        public PinterestInteractionModule(CommandHandlingService handler)
        {
            var service = handler
                ._services
                .GetRequiredService<InteractionServiceExtended>();

            _controller = (PinterestDBController)service
                .DiscordModules
                .SingleOrDefault(x => { return x.Value is PinterestDBController; }).Value;

            if (_controller == null)
            {
                var pair = CreateModules(service);
                service.AddDiscordModule(pair);
                _controller = (PinterestDBController)pair.Value;
            }
        }

        #region Команды
        [SlashCommand(name: "pin", description: "Преобразовывает ссылку Pinterest на читаемую дискордом контент типа изображение или видео")]
        public async Task Pin(string url)
        {
            var ephemeral = true;
            await DeferAsync(ephemeral: ephemeral);

            var result = string.Empty;
            try
            {
                result = await GetContentFromPin(url);
                ephemeral = false;
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            if (ephemeral)
            {
                await FollowupAsync(text: result, ephemeral: ephemeral);
            }
            else
            {
                await DeleteOriginalResponseAsync();
                await Context.Channel.SendMessageAsync(text: result, ephemeral);
            }
        }

        [DefaultMemberPermissions(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.SendMessagesInThreads | GuildPermission.SendMessages)]
        [CommandContextType([InteractionContextType.Guild])]
        [SlashCommand(name: "pin-autoparse", description: "Автоматически преобразует ссылки Pinterest в сообщениях")]
        public async Task PinAutoparse(bool allow)
        {
            var ephemeral = true;
            await DeferAsync(ephemeral: ephemeral);
            
            var settings = _controller.GetSettings(Context.Guild.Id);
            if (settings != null)
            {
                settings.GuildName = Context.Guild.Name;
                settings.AllowAutoModifyMessages = allow;
                _controller.Update(settings);
                await FollowupAsync(text: "Настройки модуля обновлены", ephemeral: ephemeral);
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
                await FollowupAsync(text: "Новые настройки модуля сохранены", ephemeral: ephemeral);
            }
        }
        #endregion

        #region Статичные
        private static KeyValuePair<IDiscordModule, IController> CreateModules(InteractionServiceExtended serviceExtended)
        {
            var controller = new PinterestDBController(serviceExtended.SQLite);
            var module = new PinterestModule(serviceExtended.Client, controller);
            return new KeyValuePair<IDiscordModule, IController>(module, controller);
        }

        /// <summary>
        /// Извлекает из ссылки на пинтерест и возвращает прямую ссылку на контент, который нормально воспринимается дискордом
        /// </summary>
        /// <param name="url">Ссылка на пинтерест</param>
        /// <returns>Прямая ссылка на контент</returns>
        /// <exception cref="NotImplementedException"></exception>
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
                        //"video" => node.Attributes["src"].Value.Replace("hls", "720p").Replace("m3u8", "mp4"),
                        "video" => await ModifyAndValidateVideoLink(client, node.Attributes["src"].Value),
                        _ => throw new NotImplementedException($"Not implemented exception of {node.Name} element"),
                    };
                    
                }
            }

            return result;
        }

        private static async Task<string> ModifyAndValidateVideoLink(HttpClient client, string url)
        {
            var tuples = new List<(string, string, string)>()
            {
                ("iht", "720p", "mp4"),
                ("mc", "720p", "mp4"),
            };

            foreach (var t in tuples)
            {
                var newUrl = url.Replace("iht", t.Item1).Replace("hls", t.Item2).Replace("m3u8", t.Item3);
                var isSuccess = await ValidateLink(client, newUrl);
                if (isSuccess)
                {
                    return newUrl;
                }
            }

            throw new NotImplementedException();
        }

        public static async Task<bool> ValidateLink(HttpClient client, string url)
        {
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }
        #endregion
    }
}