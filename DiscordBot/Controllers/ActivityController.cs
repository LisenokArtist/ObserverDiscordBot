using DiscordBot.Entities;
using DiscordBot.Structures;
using SQLite;

namespace DiscordBot.Controllers
{
    public class ActivityController : BaseController<ActivityModel>
    {
        public ActivityController(SQLiteConnection connection) : base(connection)
        {
        }

        public override ActivityModel? Get(int id)
        {
            return _connection
                .Table<ActivityModel>()
                .SingleOrDefault(x =>
                {
                    return x.ID == id;
                });
        }

        public ActivityModel? Get(int id, DateTime sessionDate)
        {
            return _connection
                .Table<ActivityModel>()
                .SingleOrDefault(x =>
                {
                    return x.ID == id && x.SessionDate == sessionDate;
                });
        }

        public ActivityModel? Get(string activityName, string userId, DateTime sessionDate)
        {
            return _connection
                .Table<ActivityModel>()
                .SingleOrDefault(x =>
                {
                    return x.ActivityName == activityName 
                    && x.UserId == userId 
                    && x.EndedDate == null
                    && x.SessionDate == sessionDate;
                });
        }

        public ActivityModel? Get(string activityName)
        {
            return _connection
                .Table<ActivityModel>()
                .SingleOrDefault(x =>
                {
                    return x.ActivityName == activityName;
                });
        }

        public IEnumerable<ActivityModel> GetAllNonClosed()
        {
            return _connection
                .Table<ActivityModel>()
                .ToList();
        }
    }
}
