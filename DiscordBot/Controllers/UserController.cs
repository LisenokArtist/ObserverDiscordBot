using DiscordBot.Entities;
using DiscordBot.Structures;
using SQLite;

namespace DiscordBot.Controllers
{
    public class UserController : BaseController<UserModel>
    {
        public UserController(SQLiteConnection connection) : base(connection) { }

        public UserModel? Get(string id)
        {
            return _connection
                .Table<UserModel>()
                .SingleOrDefault(x =>
                {
                    return x.UserId == id;
                });
        }
    }
}
