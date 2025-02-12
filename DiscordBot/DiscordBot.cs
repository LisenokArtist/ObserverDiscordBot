using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using SQLite;
using Discord.Interactions;
using Microsoft.Extensions.DependencyInjection;
using DiscordBot.Core;
using DiscordBot.Core.DiscordNetExtensions;

namespace DiscordBot
{
    public class DiscordBot : IDisposable
    {
        #region SQL
        public SQLiteConnection Connection { get; private set; }
        private DateTime? _connectedDate;
        public DateTime? ConnectedDate { get { return _connectedDate; } }
        #endregion

        public DiscordSocketClient Client { get; private set; }
        public InteractionService Commands { get; private set; }
        private ServiceProvider _services;

        private readonly DiscordSocketConfig _config = new DiscordSocketConfig
        {
            MessageCacheSize = 100,
            AlwaysDownloadUsers = true,
            HandlerTimeout = 10000,
            LogLevel = LogSeverity.Info,
            GatewayIntents = GatewayIntents.All,
        };

        public bool IsDisposed { get; private set; }

        #region Contructor
        public DiscordBot()
        {
            InitializeDataBase();
            InitializeAsync();
        }

        private ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton(x => new SQLiteConnection(new SQLiteConnectionString(Path.Combine(Environment.CurrentDirectory, "NewDataBase.db"))))
                .AddSingleton(x => new DiscordSocketClient(_config))
                .AddSingleton(x => new InteractionServiceExtended(
                    x.GetRequiredService<DiscordSocketClient>(), 
                    x.GetRequiredService<SQLiteConnection>()))
                .AddSingleton<CommandHandlingService>()
                .BuildServiceProvider();
        }

        private void InitializeDataBase()
        {
            Connection = new SQLiteConnection(new SQLiteConnectionString(Path.Combine(Environment.CurrentDirectory, "DataBase.db")));
        }

        private async Task InitializeAsync()
        {
            _services = ConfigureServices();
            Client = _services.GetRequiredService<DiscordSocketClient>();
            Commands = _services.GetRequiredService<InteractionServiceExtended>();

            Client.Log += Client_Log;
            Commands.Log += Client_Log;

            Client.Ready += async () =>
            {
                foreach (var guild in Client.Guilds)
                {
                    await Commands.RegisterCommandsToGuildAsync(guild.Id, true);
                }
            };

            var token = GetToken();
            await Client.LoginAsync(TokenType.Bot, token);
            await Client.StartAsync();
            await _services.GetRequiredService<CommandHandlingService>().InitializeAsync();
        }
        #endregion

        private Task Client_Log(LogMessage arg)
        {
            Console.WriteLine(arg.ToString());
            return Task.CompletedTask;
        }

        public async void Dispose()
        {
            if (IsDisposed) return;

            await Client.StopAsync();
            Client.Dispose();
            
            Connection.Dispose();

            IsDisposed = true;
        }

        private static readonly IConfigurationSection Secrets = ConfigurationBuilder();

        private static IConfigurationSection ConfigurationBuilder()
        {
        #if DEBUG
            return new ConfigurationBuilder()
                .AddUserSecrets(Assembly.GetExecutingAssembly())
                .Build()
                .GetSection("DiscordConfigurationsDebug");
        #else
            return new ConfigurationBuilder()
                .AddUserSecrets(Assembly.GetExecutingAssembly())
                .Build()
                .GetSection("DiscordConfigurations");
        #endif
        }

        private static string GetToken() => Secrets.GetSection("Token")?.Value ?? throw new NullReferenceException("Unable to grab token from user secrets config");
    }
}
