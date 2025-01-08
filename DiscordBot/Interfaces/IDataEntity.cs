namespace DiscordBot.Interfaces
{
    public interface IDataEntity
    {
        [SQLite.PrimaryKey, SQLite.AutoIncrement]
        public int ID { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime UpdatedDate { get; set; }

        public DateTime SessionDate { get; set; }
    }
}