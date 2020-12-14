namespace YoutubeCleanupTool.Interfaces
{
    public interface IPersister
    {
        bool DataExists(string name);
        T GetData<T>(string name) where T : new();
        void SaveData<T>(string name, T saveThis);
    }
}