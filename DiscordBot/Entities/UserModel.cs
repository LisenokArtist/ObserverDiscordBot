using DiscordBot.Entities.Base;

namespace DiscordBot.Entities
{
    [SQLite.Table("UserTable")]
    public class UserModel : DataEntityBase
    {
        public string UserId { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;
    }
}
