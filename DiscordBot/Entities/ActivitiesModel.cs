using DiscordBot.Entities.Base;

namespace DiscordBot.Entities
{
    [SQLite.Table("UserActivityTable")]
    public class UserActivityModel : DataEntityBase
    {
        public int ActivityId { get; set; }

        public int UserId { get; set; }

        public TimeSpan ActivityTime { get; set; }

        public DateTime? LastActivityTime { get; set;}
    }
}
