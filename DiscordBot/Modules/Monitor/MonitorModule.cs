using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using SQLite;

namespace DiscordBot.Modules.Monitor
{
    public class MonitorModule : DiscordModuleBase<MonitorInteractionModule>
    {
        private MonitorDBController _controller;
        
        public MonitorModule(DiscordSocketClient client, IServiceProvider services) : base(client, services)
        {
            _controller = new MonitorDBController(services.GetRequiredService<SQLiteConnection>());

            //services.GetRequiredService<CommandHandlingService>()._sqlite.TableChanged += TableChanged;
        }

        internal async Task InitializeAsync()
        {
            _client.MessageDeleted += MessageDeleted;
            _client.MessagesBulkDeleted += MessagesBulkDeleted;
            _client.MessageUpdated += MessageUpdated;
            _client.MessageReceived += MessageReceived;
        }

        private Task MessageReceived(SocketMessage arg)
        {
            Console.WriteLine($"{arg.Author.GlobalName}: {arg.Content}");
            return Task.CompletedTask;
        }

        private async Task MessageDeleted(Discord.Cacheable<Discord.IMessage, ulong> cachedMessage, Discord.Cacheable<Discord.IMessageChannel, ulong> channel)
        {
            if (channel.HasValue)
                await TryRespondMessage(cachedMessage, channel.Value);
            //Console.WriteLine($"{title}: {content}");
            return;
        }

        private Task MessagesBulkDeleted(IReadOnlyCollection<Discord.Cacheable<Discord.IMessage, ulong>> arg1, Discord.Cacheable<Discord.IMessageChannel, ulong> arg2)
        {
            return Task.CompletedTask;
        }

        private async Task MessageUpdated(Discord.Cacheable<Discord.IMessage, ulong> cachedMessage, SocketMessage message, ISocketMessageChannel channel)
        {
            await TryRespondMessage(cachedMessage, channel, message);
            //Console.WriteLine($"{title}: {content}");
            return;
        }

        private async Task<bool> TryRespondMessage(
            Discord.Cacheable<Discord.IMessage, ulong> cachedMessage, 
            IMessageChannel channel,
            SocketMessage? message = null)
        {
            //Проверим гилду
            var guild = (channel as SocketGuildChannel)?.Guild;
            if (guild == null) return false;

            //Проверим, можем ли отправить ответ в канал для ответов
            var channelToResponse = _controller.GetLoggedToChannel(guild.Id);
            if (channelToResponse == null) return false;

            //Получим из гилды нужную сущность канала
            var socketGuildChannel = guild.GetTextChannel(Convert.ToUInt64(channelToResponse.ChannelId));
            if (socketGuildChannel == null) return false;

            var embedBuilder = GetMessageEmbedBuilder(cachedMessage.Value, message);
            await socketGuildChannel.SendMessageAsync(embed: embedBuilder.Build());
            return true;
        }

        private EmbedBuilder GetMessageEmbedBuilder(IMessage cachedMessage, IMessage? message = null)
        {
            if (message != null)
                return GetMessageUpdatedEmbedBuilder(cachedMessage, message);
            else
                return GetMessageDeletedEmbedBuilder(cachedMessage);
        }

        private EmbedBuilder GetMessageUpdatedEmbedBuilder(IMessage? cachedMessage, IMessage message)
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

        private EmbedBuilder GetMessageDeletedEmbedBuilder(IMessage? cachedMessage)
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
    }
}
