namespace YoutubeCleanupTool.Interfaces
{
    public interface IPersister
    {
        bool DataExists(string playlistFile);
        T GetData<T>(string playlistFile);
        void SaveData<T>(string playlistFile, T saveThis);
    }
}