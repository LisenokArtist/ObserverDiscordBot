using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot.Interfaces;
using Newtonsoft.Json;

namespace DiscordBot.Structures
{
    public abstract class BaseModule<TSettings> : InteractionModuleBase<SocketInteractionContext>, IModule, IDisposable where TSettings : BaseModuleSettings
    {
        private readonly string _path = $"{Environment.CurrentDirectory}\\{nameof(TSettings)}.json";
        

        internal IEnumerable<TSettings> _guildsSettings;

        protected BaseModule()
        {
            _guildsSettings = Enumerable.Empty<TSettings>();

            LoadSettings();
        }

        public void LoadSettings() => LoadSettings<TSettings>();
        private void LoadSettings<T>() where T : TSettings
        {
            var isExists = File.Exists(_path);
            if (isExists)
            {
                var json = File.ReadAllText(_path);
                var settings = JsonConvert.DeserializeObject<IEnumerable<T>>(json);
                _guildsSettings = settings ?? Enumerable.Empty<TSettings>();
            }
        }

        public void SaveSettings() => SaveSettings<TSettings>();
        private void SaveSettings<T>() where T : TSettings
        {
            var json = JsonConvert.SerializeObject(_guildsSettings, Formatting.Indented);
            File.WriteAllText(_path, json);
        }

        public void Dispose()
        {
            SaveSettings();
        }
    }
}
