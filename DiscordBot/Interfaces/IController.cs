namespace DiscordBot.Interfaces
{
    public interface IController<T>
    {
        public int Add(T body);

        public int Update(T body);

        public int Remove(T body);

        public T? Get(int id);
    }
}
