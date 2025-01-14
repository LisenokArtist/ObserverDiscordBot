using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using SQLite;
using DiscordBot.Controllers;
using DiscordBot.Entities;
using Discord.Interactions;
using Microsoft.Extensions.DependencyInjection;
using DiscordBot.Services;
using DiscordBot.Modules.Monitor;
using DiscordBot.Entities.Pinterest;

namespace DiscordBot
{
    public class DiscordBot : IDisposable
    {
        #region SQL
        public SQLiteConnection Connection { get; private set; }

        public UserController UserController { get; private set; }

        public ActivityController ActivityController { get; private set; }

        private DateTime? _connectedDate;
        public DateTime? ConnectedDate { get { return _connectedDate; } }
        #endregion

        #region Modules
        private MonitorInteractionModule _messageModule;
        #endregion

        public DiscordSocketClient Client { get; private set; }
        public InteractionService Commands { get; private set; }

        private readonly DiscordSocketConfig _config = new DiscordSocketConfig
        {
            MessageCacheSize = 100,
            AlwaysDownloadUsers = true,
            HandlerTimeout = 10000,
            LogLevel = LogSeverity.Info,
            GatewayIntents =    GatewayIntents.All,
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

        private ServiceProvider _services;
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
            //var monitor = _services.GetRequiredService<MonitorModule>();
            //await monitor.InitializeAsync();
            //var pinterest = _services.GetRequiredService<PinterestModule>();
            //await pinterest.InitializeAsync();
        }
        #endregion

        #region Activities detection
        private void ResumeAllActivities(SocketGuild guild)
        {
            //Console.WriteLine(nameof(ResumeAllActivities));

            //foreach (var socketUser in guild.Users)
            //{
            //    if (socketUser.IsBot || socketUser.IsWebhook) return;

            //    foreach (var activity in socketUser.Activities)
            //    {
            //        if (activity.Type == ActivityType.Playing)
            //        {
            //            var user = AddOrUpdateUser(socketUser);
            //            ConvertToActivityModel(user, activity, true);
            //        }
            //    }
            //}
        }

        private Task GuildMembersDownloaded(SocketGuild guild)
        {
            //Console.WriteLine(nameof(GuildMembersDownloaded));

            //ResumeAllActivities(guild);

            return Task.CompletedTask;
        }
        
        private Task PresenceUpdated(SocketUser socketUser, SocketPresence activityBefore, SocketPresence activityAfter)
        {
            Console.WriteLine(nameof(PresenceUpdated));

            //if (socketUser.IsBot || socketUser.IsWebhook) return Task.CompletedTask;

            //var user = AddOrUpdateUser(socketUser);

            //if (activityBefore.Activities != null)
            //{
            //    foreach (var activity in activityBefore.Activities)
            //    {
            //        if (activity.Type == ActivityType.Playing)
            //        {
            //            ConvertToActivityModel(user, activity, false);
            //        }
            //    }
            //}

            //if (activityAfter.Activities != null)
            //{
            //    foreach (var activity in activityAfter.Activities)
            //    {
            //        if (activity.Type == ActivityType.Playing)
            //        {
            //            ConvertToActivityModel(user, activity, true);
            //        }
            //    }
            //}

            return Task.CompletedTask;
        }

        private void ConvertToActivityModel(UserModel user, IActivity activity, bool isAfter)
        {
            //if (ConnectedDate == null) throw new NotImplementedException();

            //var activityModel = ActivityController.Get(activity.Name, user.UserId, (DateTime)ConnectedDate);
            //if (activityModel == null)
            //{
            //    activityModel = new ActivityModel
            //    {
            //        ActivityName = activity.Name,
            //        UserId = user.UserId,
            //        SessionDate = (DateTime)ConnectedDate,
            //    };

            //    ActivityController.Add(activityModel);

            //    Console.WriteLine("Новая активность: {0} {1} {2}", user.ID, user.UserName, activityModel.ActivityName);
            //}
            //else
            //{
            //    if (!isAfter) { activityModel.EndedDate = DateTime.Now; }

            //    ActivityController.Update(activityModel);

            //    TimeSpan timeSpan = TimeSpan.Zero;
            //    if (activityModel.EndedDate != null)
            //    {
            //        timeSpan = ((DateTime)activityModel.EndedDate).Subtract(activityModel.CreatedDate);
            //    }
                
            //    Console.WriteLine("Активность обновлена: {0} {1} {2} {3}", user.ID, user.UserName, activityModel.ActivityName, timeSpan);
            //}
        }

        private void CloseAllActivities()
        {
            //var activities = ActivityController.GetAllNonClosed();

            //foreach (var activity in activities)
            //{
            //    activity.EndedDate = ConnectedDate;
            //}

            //ActivityController.UpdateAll(activities);
        }
        #endregion

        /// <summary>
        /// Обновляет данные о пользователе
        /// </summary>
        /// <param name="socketUser"></param>
        /// <returns>Данные, записанные в базу</returns>
        private UserModel AddOrUpdateUser(SocketUser socketUser)
        {
            if (ConnectedDate == null) throw new NotImplementedException();

            var userId = socketUser.Id.ToString();
            var user = UserController.Get(userId);

            if (user == null)
            {
                user = new UserModel
                {
                    UserId = userId,
                    UserName = socketUser.GlobalName,
                    SessionDate = (DateTime)ConnectedDate,
                };

                UserController.Add(user);
            }
            else
            {
                user.UserName = socketUser.GlobalName;
                user.UpdatedDate = DateTime.Now;
                
                UserController.Update(user);
            }

            return user;
        }


        private Task GuildMemberUpdated(Cacheable<SocketGuildUser, ulong> arg1, SocketGuildUser arg2)
        {
            Console.WriteLine(arg1.ToString());
            Console.WriteLine(arg2.ToString());
            return Task.CompletedTask;
        }

        private Task MessageReceived(SocketMessage arg)
        {
            Console.WriteLine(arg.ToString());
            return Task.CompletedTask;
        }
        

        private Task Client_Log(LogMessage arg)
        {
            Console.WriteLine(arg.ToString());
            return Task.CompletedTask;
        }

        public async void Dispose()
        {
            if (IsDisposed) return;

            CloseAllActivities();

            await Client.StopAsync();
            Client.Dispose();
            //_messageModule.Dispose();

            IsDisposed = true;
        }

        private static readonly IConfigurationSection Secrets =
            new ConfigurationBuilder()
            .AddUserSecrets(Assembly.GetExecutingAssembly())
            .Build()
            .GetSection("DiscordConfigurations");

        private static string GetToken() => Secrets.GetSection("Token")?.Value ?? throw new NullReferenceException("Unable to grab token from user secrets config");
    }
}
