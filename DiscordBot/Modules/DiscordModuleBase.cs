using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot.Modules
{
    /// <summary>
    /// Базовый модуль дискорда. Описывает действия и набор команд
    /// </summary>
    public abstract class DiscordModuleBase<T> where T : IInteractionModuleBase
    {
        internal readonly DiscordSocketClient _client;
        private readonly IServiceProvider _services;

        internal DiscordModuleBase(DiscordSocketClient client, IServiceProvider services)
        {
            _client = client;
            _services = services;
            //var service = _services.GetRequiredService<T>();
            //if (service == null) 
            //    throw new Exception($"Для работы модуля требуется добавить {nameof(T)} в коллекцию сервисов");
        }
    }
}
