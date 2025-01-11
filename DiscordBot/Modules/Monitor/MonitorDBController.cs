using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot.Entities.Monitor;
using DiscordBot.Structures;
using SQLite;

namespace DiscordBot.Modules.Monitor
{
    public class MonitorChannelsToManipulate
    {
        public ulong GuildId { get; private set; }
        public ulong ChannelId { get; private set; }
        public string ChannelName { get; set; }
        public MonitorChannelsToManipulate(ulong guildId, ulong channelId)
        {
            GuildId = guildId;
            ChannelId = channelId;
        }
        public MonitorChannelsToManipulate(ulong guildId, ulong channelId, string channelName)
        {
            GuildId = guildId;
            ChannelId = channelId;
            ChannelName = channelName;
        }
    }

    public class MonitorDBController 
        : BaseController<MonitorChannelModel, MonitorReportToChannelModel>
    {
        #region События
        public event EventHandler OnTableChanged;
        #endregion

        public MonitorDBController(SQLiteConnection connection) : base(connection)
        {

        }


        #region Методы проверки
        /// <summary>
        /// Возвращает элемент по идентификатору канала
        /// </summary>
        /// <param name="channelId">Идентификатор канала</param>
        /// <returns>Канал</returns>
        public MonitorChannelModel? GetChannelById(ulong channelId)
        {
            return _connection.Table<MonitorChannelModel>().SingleOrDefault(x => x.ChannelId == channelId.ToString());
        }

        /// <summary>
        /// Возвращает группу элементов по идентификаторам каналов
        /// </summary>
        /// <param name="channelIds">Идентификатор каналов</param>
        /// <returns>Список каналов</returns>
        public IEnumerable<MonitorChannelModel> GetChannelsByIds(IEnumerable<ulong> channelIds)
        {
            var query = $"select * from \"{GetLoggedChannelsTableName()}\" where \"{nameof(MonitorChannelModel.ChannelId)}\" in ({String.Join(',', channelIds.Select(x => x.ToString()))})";
            var result = _connection.Query<MonitorChannelModel>(query).AsEnumerable();
            if (result.Count() > 0)
                return result;

            return Enumerable.Empty<MonitorChannelModel>();
        }
        
        /// <summary>
        /// Возвращает список каналов, принадлежащие указанному серверу
        /// </summary>
        /// <param name="guildId">Идентификатор сервера</param>
        /// <returns>Список каналов</returns>
        public IEnumerable<MonitorChannelModel> GetChannelsByGuildId(ulong guildId)
        {
            var query = $"select * from \"{GetLoggedChannelsTableName()}\" where \"{nameof(MonitorChannelModel.GuildId)}\" == {guildId}";
            var result = _connection.Query<MonitorChannelModel>(query).AsEnumerable();
            if (result.Count() > 0)
                return result;
            return Enumerable.Empty<MonitorChannelModel>();
        }
        #endregion

        #region Манипулятивные методы
        
        #region MessageLoggerChannelModel
        /// <summary>
        /// Добавляет запись в базу данных
        /// </summary>
        /// <param name="channelsToAdd">Каналы к добавлению</param>
        /// <param name="fireEvent">Вызвать события, привязанные к этому методу</param>
        /// <returns></returns>
        public int Add(IEnumerable<MonitorChannelsToManipulate> channelsToAdd, bool fireEvent = true)
        {
            if (channelsToAdd.Count() == 0) return 0;

            var channels = from c in channelsToAdd
                           select new MonitorChannelModel
                           {
                               GuildId = c.GuildId.ToString(),
                               ChannelId = c.ChannelId.ToString(),
                               ChannelName = c.ChannelName,
                           };

            if (channels.Count() > 0)
            {
                var countChanges = _connection.InsertAll(channels);

                TryFireOnTableChanged(fireEvent, countChanges);

                return countChanges;
            }

            return 0;
        }

        /// <summary>
        /// Обновляет записи в базе данных
        /// </summary>
        /// <param name="channelsToUpdate">Коллекция к обновлению</param>
        public int Update(IEnumerable<MonitorChannelsToManipulate> channelsToUpdate, bool fireEvent = true)
        {
            if (channelsToUpdate.Count() == 0) return 0;

            var query = GetChannelsByIds(channelsToUpdate.Select(x => x.ChannelId));

            if (query != null && query?.Count() > 0)
            {
                foreach (var channel in query)
                {
                    channel.ChannelName = channelsToUpdate.First(x => x.ChannelId.ToString() == channel.ChannelId).ChannelName;
                }

                var countChanges = _connection.UpdateAll(query);

                TryFireOnTableChanged(fireEvent, countChanges);

                return countChanges;
            }

            return 0;
        }

        /// <summary>
        /// Удаляет записи из базы данных
        /// </summary>
        /// <param name="channelsToRemove">Коллекция к удалению</param>
        /// /// <param name="fireEvent">Вызвать события, привязанные к этому методу</param>
        /// <returns>Число удаленных записей</returns>
        public int Remove(IEnumerable<MonitorChannelsToManipulate> channelsToRemove, bool fireEvent = true)
        {
            return RemoveAllByChannelIds(channelsToRemove.Select(x => x.ChannelId.ToString()), fireEvent);
        }

        /// <summary>
        /// Удаляет записи из базы данных по идентификатору канала
        /// </summary>
        /// <param name="channelIds">Коллекция к удалению</param>
        /// <param name="fireEvent">Вызвать события, привязанные к этому методу</param>
        /// <returns>Число удаленных записей</returns>
        public int RemoveAllByChannelIds(IEnumerable<string> channelIds, bool fireEvent = true)
        {
            if (channelIds.Count() == 0) return 0;

            var query = $"delete from \"{GetLoggedChannelsTableName()}\" where \"{nameof(MonitorChannelModel.ChannelId)}\" in ({String.Join(',', channelIds)})";
            var countChanges = _connection.Execute(query);

            TryFireOnTableChanged(fireEvent, countChanges);

            return countChanges;
        }
        
        /// <summary>
        /// Совмещает операции добавления, обновления и удаления, вызывая событие OnTableChanged один раз
        /// </summary>
        /// <param name="toAdd">Список каналов к добавлению</param>
        /// <param name="toUpdate">Список каналов к обновлению</param>
        /// <param name="toRemove">Список каналов к удалению</param>
        /// <param name="countAdded">Число добавленных каналов</param>
        /// <param name="countUpdated">Число обновленных каналов</param>
        /// <param name="countRemoved">Число удаленных каналов</param>
        public void Change(
            IEnumerable<MonitorChannelsToManipulate> toAdd,
            IEnumerable<MonitorChannelsToManipulate> toUpdate,
            IEnumerable<MonitorChannelsToManipulate> toRemove,
            out int countAdded, out int countUpdated, out int countRemoved)
        {
            countRemoved = Remove(toRemove, false);
            countUpdated = Update(toUpdate, false);
            countAdded   = Add(toAdd, false);

            TryFireOnTableChanged(true, countRemoved + countUpdated + countAdded);
        }
        #endregion

        #region MessageLoggerLoggedToChannelModel
        private int RemoveLoggedToChannelByGuildId(ulong guildId, bool fireEvent = true)
        {
            var query = $"delete from \"{GetLoggedToChannelTableName()}\" where \"{nameof(MonitorReportToChannelModel.GuildId)}\" == {guildId}";
            var countChanges = _connection.Execute(query);

            TryFireOnTableChanged(fireEvent, countChanges);

            return countChanges;
        }

        public void ChangeChannelToLog(SocketInteractionContext context, SocketTextChannel? channel,
            out int countAdded, out int countUpdated, out int countRemoved)
        {
            countAdded = 0;
            countUpdated = 0;
            countRemoved = 0;

            if (channel == null)
            {
                countRemoved = RemoveLoggedToChannelByGuildId(context.Guild.Id, false);
            }
            else
            {
                var channelLoggedTo = GetLoggedToChannel(channel.Guild.Id);

                if (channelLoggedTo != null)
                {
                    channelLoggedTo.ChannelId = channel.Id.ToString();
                    channelLoggedTo.ChannelName = channel.Name;

                    countUpdated = _connection.Update(channelLoggedTo);
                }
                else
                {
                    var newInstance = new MonitorReportToChannelModel()
                    {
                        GuildId = channel.Guild.Id.ToString(),
                        ChannelId = channel.Id.ToString(),
                        ChannelName = channel.Name,
                    };
                    countAdded = _connection.Insert(newInstance);
                }
            }

            TryFireOnTableChanged(true, countAdded + countUpdated + countRemoved);
        }
        #endregion
        
        #endregion

        #region Закрытые методы
        /// <summary>
        /// Вызывает событие OnTableChanged если условия входящих параметров выполняются
        /// </summary>
        /// <param name="isFireEventAllowed">Разрешение на вызов событие</param>
        /// <param name="countedChanges">Кол-во изменений в таблице</param>
        private void TryFireOnTableChanged(bool isFireEventAllowed, int countedChanges)
        {
            if (isFireEventAllowed && countedChanges > 0)
                OnTableChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Возвращает канал, куда будут отправляться сообщения с логами
        /// </summary>
        /// <param name="guildId">Идентификатор сервера</param>
        /// <returns>Канал</returns>
        public MonitorReportToChannelModel? GetLoggedToChannel(ulong guildId)
        {
            var query = $"select * from \"{GetLoggedToChannelTableName()}\" where \"{nameof(MonitorReportToChannelModel.GuildId)}\" == {guildId}";
            var result = _connection.Query<MonitorReportToChannelModel>(query).FirstOrDefault();
            return result;
        }

        private string GetLoggedChannelsTableName()
        {
            return GetTableNameT1();
        }

        private string GetLoggedToChannelTableName()
        {
            return GetTableNameT2();
        }
        #endregion
    }
}
