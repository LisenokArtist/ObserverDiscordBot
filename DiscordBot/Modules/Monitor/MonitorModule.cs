using Discord;
using Discord.WebSocket;

namespace DiscordBot.Modules.Monitor
{
    /// <summary>
    /// Модуль для автономной работы по отслеживанию изменений и удаление сообщений на сервере
    /// </summary>
    public class MonitorModule : DiscordModuleBase
    {
        internal MonitorDBController Controller { get { return (MonitorDBController)_controller; } }

        public MonitorModule(DiscordSocketClient client, MonitorDBController controller) : base(client, controller)
        {
            _client.MessageDeleted += MessageDeleted;
            _client.MessagesBulkDeleted += MessagesBulkDeleted;
            _client.MessageUpdated += MessageUpdated;
        }

        private async Task MessageDeleted(Discord.Cacheable<Discord.IMessage, ulong> cachedMessage, Discord.Cacheable<Discord.IMessageChannel, ulong> channel)
        {
            //Игнорировать сообщения от ботов
            if (cachedMessage.HasValue && cachedMessage.Value.Author.IsBot)
                return;

            if (channel.HasValue)
            {
                await TryRespondMessage(cachedMessage, channel.Value);
            }

            return;
        }

        private Task MessagesBulkDeleted(IReadOnlyCollection<Discord.Cacheable<Discord.IMessage, ulong>> arg1, Discord.Cacheable<Discord.IMessageChannel, ulong> arg2)
        {
            Console.WriteLine("MessagesBulkDeleted");
            return Task.CompletedTask;
        }

        private async Task MessageUpdated(Discord.Cacheable<Discord.IMessage, ulong> cachedMessage, SocketMessage message, ISocketMessageChannel channel)
        {
            //Игнорировать сообщения от ботов
            if (message.Author.IsBot)
                return;

            //Игнорировать если сообщение из кеша осталось без изменений
            if (cachedMessage.HasValue && cachedMessage.Value.Content.Equals(message.Content))
                return;

            await TryRespondMessage(cachedMessage, channel, message);
            return;
        }

        #region Приватные
        /// <summary>
        /// Если соблюдены условия, попытаться ответить в текстовый канал
        /// </summary>
        /// <param name="cachedMessage">Сообщение из кеша</param>
        /// <param name="channel">Канал</param>
        /// <param name="message">Сообщение</param>
        /// <returns></returns>
        private async Task<bool> TryRespondMessage(
            Discord.Cacheable<Discord.IMessage, ulong> cachedMessage, 
            IMessageChannel channel,
            SocketMessage? message = null)
        {
            //Проверим гилду
            var guild = (channel as SocketGuildChannel)?.Guild;
            if (guild == null) return false;

            //Проверим, можем ли отправить ответ в канал для ответов
            var channelToResponse = Controller.GetLoggedToChannel(guild.Id);
            if (channelToResponse == null) return false;

            //Получим из гилды нужную сущность канала
            var socketGuildChannel = guild.GetTextChannel(Convert.ToUInt64(channelToResponse.ChannelId));
            if (socketGuildChannel == null) return false;

            var embedBuilder = GetMessageEmbedBuilder(cachedMessage.Value, message);
            await socketGuildChannel.SendMessageAsync(embed: embedBuilder.Build());
            return true;
        }
        #endregion

        #region Статичные
        public static EmbedBuilder GetMessageEmbedBuilder(IMessage cachedMessage, IMessage? message = null)
        {
            if (message != null)
                return GetMessageUpdatedEmbedBuilder(cachedMessage, message);
            else
                return GetMessageDeletedEmbedBuilder(cachedMessage);
        }

        public static EmbedBuilder GetMessageUpdatedEmbedBuilder(IMessage? cachedMessage, IMessage message)
        {
            if (cachedMessage == null)
            {
                return new EmbedBuilder()
                    .WithTitle("Сообщение изменено")
                    .AddField("До", "Не удалось загрузить сообщение из кеша")
                    .AddField("После", message.Content)
                    .WithFooter($"{message.Id}");
            }
            else
            {
                return new EmbedBuilder()
                    .WithTitle("Сообщение изменено")
                    .AddField("Пользователь", $"{cachedMessage.Author.Mention ?? "Безымянный пользователь"}", true)
                    .AddField("Канал", $"{cachedMessage.Channel.Name ?? "Безымянный канал"}", true)
                    .AddField("До", cachedMessage.Content ?? "Не удалось загрузить сообщение из кеша")
                    .AddField("После", message.Content)
                    .WithFooter($"{message.Id}");
            }
        }

        public static EmbedBuilder GetMessageDeletedEmbedBuilder(IMessage? cachedMessage)
        {
            if (cachedMessage == null)
            {
                return new EmbedBuilder()
                    .WithTitle("Сообщение удалено")
                    .WithDescription("Не удалось восстановить содержимое сообщения из кеша");
            }
            else
            {
                return new EmbedBuilder()
                    .WithTitle("Сообщение удалено")
                    .AddField("Пользователь", $"{cachedMessage.Author.Mention}", true)
                    .AddField("Канал", $"{cachedMessage.Channel}", true)
                    //.AddField(string.Empty, cachedMessage.Content);
                    .WithDescription(cachedMessage.Content)
                    .WithFooter($"{cachedMessage.Id}");
            }
        }
        #endregion
    }
}
