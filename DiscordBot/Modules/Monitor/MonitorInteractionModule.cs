using Discord.Interactions;
using Discord.WebSocket;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using DiscordBot.Interfaces;
using DiscordBot.Core;
using DiscordBot.Core.DiscordNetExtensions;

namespace DiscordBot.Modules.Monitor
{
    public class MonitorInteractionModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly MonitorDBController _controller;

        public MonitorInteractionModule(CommandHandlingService handler)
        {
            var service = handler
                ._services
                .GetRequiredService<InteractionServiceExtended>();

            _controller = (MonitorDBController)service
                .DiscordModules
                .SingleOrDefault(x => { return x.Value is MonitorDBController; }).Value;

            if (_controller == null)
            {
                var pair = CreateModules(service);
                service.AddDiscordModule(pair);
                _controller = (MonitorDBController)pair.Value;
            }
        }

        #region Команды
        [DefaultMemberPermissions(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.ReadMessageHistory | GuildPermission.ViewChannel)]
        [SlashCommand(name: "monitor-channel", description: "Отслеживает сообщения в канале.")]
        public async Task MonitorChannel(IGuildChannel? include = null, IGuildChannel? exclude = null)
        {
            IEnumerable<SocketGuildChannel> toInsert;
            IEnumerable<SocketGuildChannel> toUpdate;
            IEnumerable<SocketGuildChannel> toRemove;
            RankingChannels(
                GetSocketGuildChannels(include), 
                GetSocketGuildChannels(exclude),
                out toInsert, out toUpdate, out toRemove);

            _controller.Change(
                toInsert.Select(x => new MonitorChannelsToManipulate(x.Guild.Id, x.Id, x.Name)),
                toUpdate.Select(x => new MonitorChannelsToManipulate(x.Guild.Id, x.Id, x.Name)),
                toRemove.Select(x => new MonitorChannelsToManipulate(x.Guild.Id, x.Id)), 
                out int countAdded, out int countUpdated, out int countRemoved);
            
            var sum = countAdded + countUpdated + countRemoved;

            if (sum > 0)
            {
                await RespondAsync(@$"Добавлено {countAdded}, обновлено {countUpdated}, удалено {countRemoved}");
            }
            else
            {
                var builder = GetBuilderLoggedChannelsList();
                await RespondAsync(embed: builder.Build(), ephemeral: true);
            }
        }

        [DefaultMemberPermissions(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.ViewChannel | GuildPermission.SendMessages)]
        [SlashCommand(name: "monitor-channel-responses", description: "Выводит информацию об изменениях сообщений в отслеживаемых каналах")]
        public async Task MonitorChannelResponses(SocketTextChannel? channel)
        {
            _controller.ChangeChannelToLog(Context, channel,
                out int countAdded, out int countUpdated, out int countRemoved);
            var sum = countAdded + countUpdated + countRemoved;
            if (sum > 0)
            {
                await RespondAsync(@$"Добавлено {countAdded}, обновлено {countUpdated}, удалено {countRemoved}");
            }
            else
            {
                await RespondAsync("Команда выполнена, но, похоже, ничего не произошло...");
            }
        }
        #endregion

        #region Приватные
        /// <summary>
        /// Получает конструктор сообщения со списком отслеживаемых каналов
        /// </summary>
        /// <returns></returns>
        private EmbedBuilder GetBuilderLoggedChannelsList()
        {
            var guildId = Context.Guild.Id;
            var channelsToLogged = _controller.GetChannelsByGuildId(guildId);
            var channels = Context.Guild.Channels.Where(x => channelsToLogged.Any(y => x.Id.ToString() == y.ChannelId));
            var mentions = channels.Select(x => ((SocketTextChannel)x).Mention ?? $"<#{x.Id}>");

            return new EmbedBuilder()
                .WithTitle("Отслеживаемые каналы")
                .WithDescription(string.Join(",\n", mentions))
                .WithColor(Color.Green)
                .WithCurrentTimestamp();
        }

        /// <summary>
        /// Выполняет ранжировку двух поступающих коллекций каналов по трем категориям: на добавление, на обновление и на удаление
        /// </summary>
        /// <param name="channelsToInclude">Список каналов на добавление</param>
        /// <param name="channelsToExclude">Список каналов на удаление</param>
        /// <param name="toAdd">Коллекция на добавление</param>
        /// <param name="toUpdate">Коллекция на обновление</param>
        /// <param name="toRemove">Коллекция на удаление</param>
        private void RankingChannels(
            IEnumerable<SocketGuildChannel> channelsToInclude,
            IEnumerable<SocketGuildChannel> channelsToExclude,
            out IEnumerable<SocketGuildChannel> toAdd,
            out IEnumerable<SocketGuildChannel> toUpdate,
            out IEnumerable<SocketGuildChannel> toRemove)
        {
            //Сводим коллекции в одну и удаляем те каналы, которые были влючены, а затем исключены
            var exclude = from e in channelsToExclude
                          select new KeyValuePair<string, SocketGuildChannel>("exclude", e);
            var include = from i in channelsToInclude
                          select new KeyValuePair<string, SocketGuildChannel>("include", i);

            var mergedCollection = new List<KeyValuePair<string, SocketGuildChannel>>();
            mergedCollection.AddRange(exclude);
            mergedCollection.AddRange(include);
            mergedCollection = mergedCollection.DistinctBy(x => x.Value).ToList();

            //Ранжируем результат по категориям
            var existedRecords = _controller.GetChannelsByIds(mergedCollection.Select(x => x.Value.Id));

            var toRemove_ = mergedCollection
                .Where(x => x.Key == "exclude");
            mergedCollection = mergedCollection.Except(toRemove_).ToList();
            var toUpdate_ = mergedCollection
                .Where(x => x.Key == "include" && existedRecords.Any(y => y.ChannelId == x.Value.Id.ToString()));
            mergedCollection = mergedCollection.Except(toUpdate_).ToList();
            var toAdd_ = mergedCollection;

            toRemove = toRemove_.Select(x => x.Value);
            toUpdate = toUpdate_.Select(x => x.Value);
            toAdd = toAdd_.Select(x => x.Value);
        }
        #endregion

        #region Статичные
        private static KeyValuePair<IDiscordModule, IController> CreateModules(InteractionServiceExtended serviceExtended)
        {
            var controller = new MonitorDBController(serviceExtended.SQLite);
            var module = new MonitorModule(serviceExtended.Client, controller);
            return new KeyValuePair<IDiscordModule, IController>(module, controller);
        }

        /// <summary>
        /// Извлекает из унаследованных от IGuildChannel список каналов
        /// </summary>
        /// <param name="guildChannel"></param>
        /// <returns></returns>
        public static IEnumerable<SocketGuildChannel> GetSocketGuildChannels(IGuildChannel? guildChannel)
        {
            if (guildChannel != null)
            {
                switch (guildChannel)
                {
                    case SocketTextChannel socketTextChannel:
                        return new List<SocketGuildChannel>() { socketTextChannel };
                    case SocketCategoryChannel socketCategoryChannel:
                        return socketCategoryChannel.Channels;
                    default: throw new NotImplementedException("Не удалось обработать интерфейс IGuildChannel");
                }
            }

            return Enumerable.Empty<SocketGuildChannel>();
        }
        #endregion
    }
}
