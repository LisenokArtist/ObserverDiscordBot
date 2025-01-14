using DiscordBot.Entities.Base;

namespace DiscordBot.Entities
{
    [SQLite.Table("ActivityTable")]
    public class ActivityModel : DataEntityBase
    {
        public string UserId { get; set; } = string.Empty;

        public string? ActivityName { get; set; }

        public DateTime? EndedDate { get; set; }
    }
}
