using DiscordBot.Interfaces;

namespace DiscordBot.Structures
{
    public class DataEntityBase : IDataEntity
    {
        [SQLite.PrimaryKey, SQLite.AutoIncrement]
        public int ID { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime UpdatedDate { get; set; } = DateTime.Now;

        public DateTime SessionDate { get; set; }
    }
}
