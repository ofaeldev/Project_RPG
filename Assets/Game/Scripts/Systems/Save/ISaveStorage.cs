namespace RPGProject.Systems
{
    public interface ISaveStorage
    {
        void Save(string key, string payload);
        bool TryLoad(string key, out string payload);
        void Delete(string key);
    }
}
