using DiscordBot.Interfaces;
using SQLite;
using System.Reflection;

namespace DiscordBot.Modules
{
    public abstract class BaseController<T1, T2> : BaseController<T1>
        where T1 : IDataEntity
        where T2 : IDataEntity
    {
        internal BaseController(SQLiteConnection connection) : base(connection) => _connection.CreateTable<T2>();

        internal string GetTableNameT2() => GetTableName<T2>();
    }

    public abstract class BaseController<T1> : BaseController
        where T1 : IDataEntity
    {
        internal BaseController(SQLiteConnection connection) : base(connection) => _connection.CreateTable<T1>();

        public int Add(object body) => _connection.Insert((T1)body);

        public int Update(object body)
        {
            ((T1)body).UpdatedDate = DateTime.Now;
            return _connection.Update((T1)body);
        }

        public int Remove(object body) => _connection.Delete((T1)body);

        /// <summary>
        /// Удаляет из базы указанные объекты
        /// </summary>
        /// <param name="bodies">Коллекция объектов к удалению</param>
        /// <param name="runInTransaction">Выполняет операцию в режиме транзакции</param>
        /// <returns>Возращает число удаленных записей</returns>
        public int RemoveAll(IEnumerable<T1> bodies, bool runInTransaction = true)
        {
            var QueryMethod = (ref int count) =>
            {
                count = 0;
                foreach (object body in bodies)
                {
                    count += _connection.Delete(body);
                };
            };

            int count = 0;
            if (runInTransaction)
            {
                _connection.RunInTransaction(() => QueryMethod(ref count));
            }
            else
            {
                QueryMethod(ref count);
            }

            return count;
        }

        internal string GetTableNameT1() => GetTableName<T1>();

        internal string GetTableName<T>()
        {
            var attribute = typeof(T).GetCustomAttribute(typeof(TableAttribute), false) ?? null;
            var inNull = attribute == null;
            if (inNull) throw new ArgumentException($"Не удалось извлечь аттрибут {nameof(TableAttribute)}");

            return ((TableAttribute)attribute).Name;
        }
    }

    public class BaseController : IController
    {
        internal SQLiteConnection _connection { get; set; }

        internal BaseController(SQLiteConnection connection)
        {
            _connection = connection;
        }
    }
}
